using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[System.Serializable]
public abstract class Effect 
{
    [SerializeField] public bool effectSelf;
    [System.NonSerialized] public bool isPlayer;
    [System.NonSerialized] public int displayAdditionalDamage;
    [SerializeField] public bool playWhenDrawnByEnemy;
    public abstract GameAction GetGameAction(); 
    public string GetDescription()
    {
        string description = GetBaseDescription();
        if (effectSelf) description += " to self";
        return description;
    }
    protected abstract string GetBaseDescription();

    // Numeric magnitude of this effect per application. Subclasses with a serialized amount override this.
    // Effects without a numeric magnitude (e.g. SpecialEffect, UltimateCardEffect) leave the default 0.
    public virtual int Magnitude => 0;

    protected int GetDisplayDamageAmount(int baseDamage)
    {
        return baseDamage + displayAdditionalDamage;
    }
}
