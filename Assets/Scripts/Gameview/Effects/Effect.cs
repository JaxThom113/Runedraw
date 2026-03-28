using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[System.Serializable]
public abstract class Effect 
{
    /// <summary>Runtime-only targeting side. Systems set this before GetGameAction() so nested effect assets do not expose misleading inspector state.</summary>
    [System.NonSerialized] public bool isPlayer;
    [System.NonSerialized] public int displayAdditionalDamage;
    [SerializeField] public bool playWhenDrawnByEnemy;
    public abstract GameAction GetGameAction(); 
    public abstract string GetDescription();

    protected int GetDisplayDamageAmount(int baseDamage)
    {
        return baseDamage + displayAdditionalDamage;
    }
}
