
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Species
{
    public Player mascot;
    public List<Player> members;
    public float speciesFitness;
    public Color color;

    public int counter = 0;

    public Species(Player mascot){
        this.mascot = mascot;
        this.members = new List<Player>();
        this.members.Add(mascot);
        this.speciesFitness = 0.0f;
    }

    public void setSpeciesFitness(float newFitVal){
        this.speciesFitness += newFitVal;
    }

    public void setMascot(Player newPlayer){
        bool found = false;
        if (mascot != null){
            for (int i = 0; i < members.Count; i++){
                if (members[i].player_GO == mascot.player_GO){
                    members[i] = newPlayer;
                    found = true;
                }
            }
        }

        mascot = newPlayer;
        if (!found){
            members.Add(mascot);
        }
    }

    public void reset(){
        this.mascot = members[Random.Range(0, members.Count)];
        members.Clear();
        this.speciesFitness = 0.0f;
    }



}