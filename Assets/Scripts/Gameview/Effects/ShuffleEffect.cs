using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class ShuffleEffect : Effect
{ 
    [SerializeField] public int shuffleAmount;
    public override GameAction GetGameAction()
    {
        ShuffleGA shuffleGA = new(shuffleAmount);
        return shuffleGA;
    }

    public override string GetDescription()
    {
        return $"Discard your hand, shuffle that same amount of cards";
    }
}
