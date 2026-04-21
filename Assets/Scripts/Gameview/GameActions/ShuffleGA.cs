using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShuffleGA : GameAction

{ 
    public int magnitude { get; set; }
    public ShuffleGA(int magnitude){ 
        this.magnitude = magnitude;  
    }
}
