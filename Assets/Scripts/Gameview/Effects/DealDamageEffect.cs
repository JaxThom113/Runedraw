using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;


public class DealDamageEffect : Effect
{
    [FormerlySerializedAs("damageAmount")]
    [SerializeField] public int magnitude;
    public override int Magnitude => magnitude;
    public override GameAction GetGameAction()
    {
        DealDamageGA dealDamageGA = new(magnitude, isPlayer);  
        return dealDamageGA;

    } 
    protected override string GetBaseDescription()
    {
        return $"Deal {GetDisplayDamageAmount(magnitude)} damage";
    }
}
