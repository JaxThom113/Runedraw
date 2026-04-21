using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DrawCardGA : GameAction
{
    public int magnitude { get; set; }
    public DrawCardGA(int magnitude){ 
        this.magnitude = magnitude;  
    }
}
