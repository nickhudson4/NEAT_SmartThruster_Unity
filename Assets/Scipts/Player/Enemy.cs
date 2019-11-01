using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enemy : MonoBehaviour
{
    NeatManager neatManager;
    Counter switchTargetCounter;

    Player target;

    public float speed;

    void Start(){
        neatManager = GameObject.Find("NeatManager").GetComponent<NeatManager>();
        switchTargetCounter = new Counter(10.0f);
    }
    void Update(){
        if (neatManager.players.Count == 0){ return; }
        target = neatManager.players[Random.Range(0, neatManager.players.Count)];
        if (target == null){ return; }

        if (switchTargetCounter.isOver()){
            target = getClosestPlayer();
        }

        followTarget();

        switchTargetCounter.incriment();
    }

    private void followTarget(){
        transform.LookAt(target.player_GO.transform);

        transform.Translate(transform.forward * speed * Time.deltaTime, Space.World);
    }

    private Player getClosestPlayer(){
        Player closest_player = null;
        float closest_dist = -1.0f;
        foreach (Player p in neatManager.players){
            float dist = Vector3.Distance(transform.position, p.player_GO.transform.position);
            if (dist < closest_dist){
                closest_player = p;
                closest_dist = dist;
            }
        }

        return closest_player;
    }
}
