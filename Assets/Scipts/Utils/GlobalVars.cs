
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class GlobalVars
{
    // public static int innovation = 0;
    public static Dictionary<int, Vector2> pairs = new Dictionary<int, Vector2>();

    // public static int getInnov(){
    //     int innov_copy = innovation;
    //     innovation+=1;
    //     return innov_copy;
    // }

    public static int addPair(Vector2 pair){
        // Debug.Log("Adding pair: " + pair);
        foreach (var p in pairs){
            if (p.Value == pair){ //If already contains pair. return corresponing innov
            // Debug.Log("FOUND: " + p);
                return p.Key;
            }
        }
        int rtn = pairs.Count;
        pairs.Add(pairs.Count, pair);
        return rtn;
    }

    public static int getInnov(Vector2 pair){
        foreach (var p in pairs){
            if (p.Value == pair){ //If already contains pair. return corresponing innov
                return p.Key;
            }
        }

        return -1;
    }
}