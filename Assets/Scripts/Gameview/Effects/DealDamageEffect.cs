using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class DealDamageEffect : Effect
{
    [SerializeField] public int damageAmount;   
    public override GameAction GetGameAction()
    {
        DealDamageGA dealDamageGA = new(damageAmount, isPlayer);  
        return dealDamageGA;

    } 
    public override string GetDescription()
    {
        return $"Deal {GetDisplayDamageAmount(damageAmount)} damage";
    }
}