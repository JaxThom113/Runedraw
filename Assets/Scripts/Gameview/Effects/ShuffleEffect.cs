using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

[System.Serializable]
public class ShuffleEffect : Effect
{ 
    [FormerlySerializedAs("shuffleAmount")]
    [SerializeField] public int magnitude;
    public override int Magnitude => magnitude;

    public override GameAction GetGameAction()
    {
        ShuffleGA shuffleGA = new(magnitude);
        return shuffleGA;
    }

    protected override string GetBaseDescription()
    {
        return $"Draw a new hand of cards";
    }
}
