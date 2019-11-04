
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player
{
    public NeatNetwork network;
    public NeatManager manager;
    public GameObject player_GO;
    public PlayerController controller;

    public Species species;

    public  float score;
    public float fitness;

    //STATE VARS
    public bool isDead;
    public Vector3 lastPos;

    //NO MOVEMENT (STUCK) VARS
    public Counter stuckCounter;

    //INFITE LOOP VARS
    public Counter outerCounter;
    public Counter innerCounter;
    Vector3 previousPos = Vector3.zero;
    int maxFailedBeforeEnding = 10;
    float cutOffDist = 10.0f;
    int failedCounter = 0;


    public Player(NeatNetwork network, GameObject player_GO, NeatManager manager){
        this.network = network;
        this.player_GO = player_GO;
        this.manager = manager;
        this.controller = player_GO.GetComponent<PlayerController>();

        this.score = 0;
        this.fitness = 0;
        this.isDead = false;
        this.stuckCounter = new Counter(2.0f);
        
        this.outerCounter = new Counter(6.0f);
        this.innerCounter = new Counter(1.0f);
        this.previousPos = this.player_GO.transform.position;
    }

    public void reset(Vector3 pos){
        player_GO.transform.position = pos;
        player_GO.transform.rotation = Quaternion.identity;
        controller.rb.velocity = Vector3.zero;
    }

    public void checkIfStuck(int player_index){
        Vector3 newPos = player_GO.transform.position;
        float changeInPos = Vector3.Distance(lastPos, newPos);


        if (stuckCounter.isOver()){
            manager.onDeath(player_index);
            // Debug.Log("Player is stuck: " + player_GO, player_GO);
        }

        if (changeInPos <= 0.01f){
            stuckCounter.incriment();
        }
        else {
            stuckCounter.reset();
        }
    }

    public void checkIfInLoop(int player_index){
        if (outerCounter.isOver()){
            previousPos = player_GO.transform.position;

            outerCounter.reset();
        }

        if (innerCounter.isOver()){
            float dist = Vector3.Distance(player_GO.transform.position, previousPos);
            Debug.Log("dist: " + dist, manager.players[player_index].player_GO);
            if (dist < cutOffDist){
                failedCounter++;
                Debug.Log("player: " + manager.players[player_index].player_GO + " failed ", manager.players[player_index].player_GO);
            }
            else {
                failedCounter = 0;
            }

            if (failedCounter >= maxFailedBeforeEnding){
                Debug.Log("player: " + manager.players[player_index].player_GO + " killed ", manager.players[player_index].player_GO);
                failedCounter = 0;
                manager.onDeath(player_index);
            }

            innerCounter.reset();
        }
        // Debug.Log("COUNT: " + innerCounter.currentCount());

        outerCounter.incriment();
        innerCounter.incriment();
    }
}