
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Species
{
    public Player mascot;
    public List<Player> members;
    public float speciesFitness;

    public Species(Player mascot){
        this.mascot = mascot;
        this.members = new List<Player>();
        this.members.Add(mascot);
        this.speciesFitness = 0.0f;
    }

    public void setSpeciesFitness(float newFitVal){
        this.speciesFitness += newFitVal;
    }

    public void reset(){
        this.mascot = members[Random.Range(0, members.Count)];
        members.Clear();
        this.speciesFitness = 0.0f;
    }



}