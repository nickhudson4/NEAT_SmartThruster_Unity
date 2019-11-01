
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
    public Counter stuckCounter;
    public Vector3 lastPos;


    public Player(NeatNetwork network, GameObject player_GO, NeatManager manager){
        this.network = network;
        this.player_GO = player_GO;
        this.manager = manager;
        this.controller = player_GO.GetComponent<PlayerController>();

        this.score = 0;
        this.fitness = 0;
        this.isDead = false;
        this.stuckCounter = new Counter(2.0f);
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
}