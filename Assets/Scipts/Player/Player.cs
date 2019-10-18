
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player
{
    public NeatNetwork network;
    public GameObject player_GO;
    public PlayerController controller;

    public Species species;

    public  float score;
    public float fitness;
    public bool isDead;

    public Player(NeatNetwork network, GameObject player_GO){
        this.network = network;
        this.player_GO = player_GO;
        this.controller = player_GO.GetComponent<PlayerController>();

        this.score = 0;
        this.fitness = 0;
        this.isDead = false;
    }

    public float getScore(){
        return Random.Range(0.0f, 20.0f);
    }
}