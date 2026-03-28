using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DealDamageFromTrackedElementEffect : Effect
{
    public Element element;  

    private int GetTrackedDamageAmount()
    {
        return isPlayer
            ? CardTrackSystem.Instance.GetPlayerCardTrackElement(element)
            : CardTrackSystem.Instance.GetEnemyCardTrackElement(element);
    }

    public override GameAction GetGameAction()
    { 
        int damageAmount = GetTrackedDamageAmount();
        DealDamageGA dealDamageGA = new(damageAmount, isPlayer);
        return dealDamageGA;
    }
    public override string GetDescription()
    {
        int damageAmount = GetDisplayDamageAmount(GetTrackedDamageAmount());
        return $"Deal {damageAmount} damage for each {element} card played this turn";
    }
}
