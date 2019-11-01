using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyController : MonoBehaviour
{
    public GameObject enemy_prefab;
    public int num_enemies;
    public Vector2 spawnPosRange_x;
    public Vector2 spawnPosRange_y;
    Counter spawnCounter;
    List<GameObject> enemies;

    void Start(){
        spawnCounter = new Counter(2.0f);
        enemies = new List<GameObject>();

        for (int i = 0; i < num_enemies; i++){
            spawnEnemy();
        }
    }

    void Update(){

        // spawnCounter.incriment();
        // if (spawnCounter.isOver()){
        //     spawnCounter.reset();
        //     spawnEnemy();
        // }
    }

    private void spawnEnemy(){
        if (enemy_prefab == null){ /*Debug.Log("RETURNING");*/ return; }
        Vector3 pos = new Vector3(Random.Range(spawnPosRange_x.x, spawnPosRange_x.y), 1.0f, Random.Range(spawnPosRange_y.x, spawnPosRange_y.y));
        GameObject en = Instantiate(enemy_prefab, pos, Quaternion.identity);

    }
}
