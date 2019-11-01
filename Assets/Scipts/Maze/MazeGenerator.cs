using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MazeGenerator : MonoBehaviour
{
    public GameObject mazeHolder;
    public Vector2 barrierSizeRange;
    public Vector2 barrierPosRange_x;
    public Vector2 barrierPosRange_y;
    public Vector2 barrierCountRange;

    // Start is called before the first frame update
    void Start()
    {

    }

    public void OnClickGenerateMaze(){
        for (int i = 0; i < mazeHolder.transform.childCount; i++){ //Remove old maze
            GameObject child = mazeHolder.transform.GetChild(i).gameObject;
            Destroy(child);
        }

        int randBarrierCount = Random.Range((int)barrierCountRange.x, (int)barrierCountRange.y);
        for (int i = 0; i < randBarrierCount; i++){
            GameObject barr_go = GameObject.CreatePrimitive(PrimitiveType.Cube);
            barr_go.transform.parent = mazeHolder.transform;
            barr_go.name = "Barrier" + i;
            barr_go.tag = "MazeWall";
            barr_go.layer = 9;
            barr_go.transform.position = new Vector3(Random.Range(barrierPosRange_x.x, barrierPosRange_x.y), 1, Random.Range(barrierPosRange_y.x, barrierPosRange_y.y));
            int randWidthAxis = Random.Range(0, 2);
            if (randWidthAxis == 0){
                barr_go.transform.localScale = new Vector3(Random.Range(barrierSizeRange.x, barrierSizeRange.y), 1, 1);

            }
            else {
                barr_go.transform.localScale = new Vector3(1, 1, Random.Range(barrierSizeRange.x, barrierSizeRange.y));
            }

        }

    }

}
