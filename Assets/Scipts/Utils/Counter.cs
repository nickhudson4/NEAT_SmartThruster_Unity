

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Counter 
{
    public float maxTime;
    private float counter;
    public Counter(float maxTime){
        this.maxTime = maxTime;
        this.counter = 0.0f;
    }

    public void incriment(){
        counter += Time.deltaTime;
    }

    public void reset(){
        counter = 0.0f;
    }

    public bool isOver(){
        if (counter >= maxTime){
            return true;
        }
        return false;
    }

    public float currentCount(){
        return counter;
    }
}