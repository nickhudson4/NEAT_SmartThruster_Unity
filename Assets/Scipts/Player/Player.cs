
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
    int maxFailedBeforeEnding = 5;
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

        this.outerCounter = new Counter(4.0f);
        this.innerCounter = new Counter(0.3f);
        this.previousPos = this.player_GO.transform.position;
    }

    public void reset(Vector3 pos){
        player_GO.transform.position = pos;
        player_GO.transform.rotation = Quaternion.identity;
        controller.rb.velocity = Vector3.zero;
    }

    public void checkIfStuck(int player_index){
        // Vector3 newPos = player_GO.transform.position;
        // float changeInPos = Vector3.Distance(lastPos, newPos);
        // // Debug.Log("CHANGE: " + changeInPos + " COUNTER: " + stuckCounter.currentCount());


        // if (stuckCounter.isOver()){
        //     manager.onDeath(player_index);
        //     // Debug.Log("Player is stuck: " + player_GO, player_GO);
        // }

        // if (changeInPos <= manager.speed / 150.0f){
        //     stuckCounter.incriment();
        // }
        // else {
        //     stuckCounter.reset();
        // }
    }

    public void checkIfInLoop(int player_index){
        if (outerCounter.isOver()){
            previousPos = player_GO.transform.position;

            outerCounter.reset();
        }

        if (innerCounter.isOver()){
            float dist = Vector3.Distance(player_GO.transform.position, previousPos);
            if (dist < cutOffDist){
                failedCounter++;
            }
            else {
                failedCounter = 0;
            }

            if (failedCounter >= maxFailedBeforeEnding){
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