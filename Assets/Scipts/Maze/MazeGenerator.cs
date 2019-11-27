using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class MazeGenerator : MonoBehaviour
{
    // public GameObject mazeHolder;
    // public Vector2 barrierSizeRange;
    // public Vector2 barrierPosRange_x;
    // public Vector2 barrierPosRange_y;
    // public Vector2 barrierCountRange;

    public GameObject pairs;
    public TMP_InputField numBarriersInput;
    private int numBarriers;
    public Vector2 floorRange_x;
    public Vector2 floorRange_z;


    public void OnClickGenerateMaze(){
        try {
            numBarriers = int.Parse(numBarriersInput.text);
        }
        catch {
            Debug.LogError("Failed To Get Save Num. Loading Save 0");
            numBarriers = 4;
        }

        float floorSize = Mathf.Abs(floorRange_x.x) + Mathf.Abs(floorRange_x.y);
        float sectionSize = floorSize / (numBarriers + 1);

        for (int i = 0; i < pairs.transform.childCount; i++){
            GameObject child = pairs.transform.GetChild(i).gameObject;
            Destroy(child);
        }

        for (int i = 1; i <= numBarriers; i++){

            float x_pos = floorRange_z.x + (i * sectionSize);
            float gate_opening_size = Random.Range(10.0f, 40.0f);
            float combined_scale = floorSize - gate_opening_size;

            GameObject prim1 = GameObject.CreatePrimitive(PrimitiveType.Cube);
            GameObject prim2 = GameObject.CreatePrimitive(PrimitiveType.Cube);
            float scale1 = Random.Range(20.0f, 80.0f);
            float scale2 = floorSize - scale1;
            scale1 -= gate_opening_size/2.0f;
            scale2 -= gate_opening_size/2.0f;
            prim1.transform.localScale = new Vector3(scale1, 1, 1);
            prim2.transform.localScale = new Vector3(scale2, 1, 1);

            prim1.transform.position = new Vector3(floorRange_x.x + (prim1.transform.localScale.x/2.0f), 1, floorRange_z.x + (i * sectionSize));
            prim2.transform.position = new Vector3(floorRange_x.y - (prim2.transform.localScale.x/2.0f), 1, floorRange_z.x + (i * sectionSize));

            prim1.transform.parent = pairs.transform;
            prim2.transform.parent = pairs.transform;

            prim1.tag = "MazeWall";
            prim2.tag = "MazeWall";
            prim1.layer = 9;
            prim2.layer = 9;


        }
        GameObject.Find("NeatManager").GetComponent<NeatManager>().getGatePositions();









        // for (int i = 0; i < mazeHolder.transform.childCount; i++){ //Remove old maze
        //     GameObject child = mazeHolder.transform.GetChild(i).gameObject;
        //     Destroy(child);
        // }

        // int randBarrierCount = Random.Range((int)barrierCountRange.x, (int)barrierCountRange.y);
        // for (int i = 0; i < randBarrierCount; i++){
        //     GameObject barr_go = GameObject.CreatePrimitive(PrimitiveType.Cube);
        //     barr_go.transform.parent = mazeHolder.transform;
        //     barr_go.name = "Barrier" + i;
        //     barr_go.tag = "MazeWall";
        //     barr_go.layer = 9;
        //     barr_go.transform.position = new Vector3(Random.Range(barrierPosRange_x.x, barrierPosRange_x.y), 1, Random.Range(barrierPosRange_y.x, barrierPosRange_y.y));
        //     int randWidthAxis = Random.Range(0, 2);
        //     if (randWidthAxis == 0){
        //         barr_go.transform.localScale = new Vector3(Random.Range(barrierSizeRange.x, barrierSizeRange.y), 1, 1);

        //     }
        //     else {
        //         barr_go.transform.localScale = new Vector3(1, 1, Random.Range(barrierSizeRange.x, barrierSizeRange.y));
        //     }

        // }

    }

}
