using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[System.Serializable]
public abstract class Effect 
{
    [SerializeField] public bool isPlayer;
    [SerializeField] public bool playWhenDrawnByEnemy;
    public abstract GameAction GetGameAction(); 
    public abstract string GetDescription();
}
