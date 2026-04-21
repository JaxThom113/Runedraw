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

    // Magnitude is tracked dynamically from the element's card-track value, not a serialized field.
    public override int Magnitude => GetTrackedDamageAmount();

    public override GameAction GetGameAction()
    { 
        int damageAmount = GetTrackedDamageAmount();
        DealDamageGA dealDamageGA = new(damageAmount, isPlayer);
        return dealDamageGA;
    }
    protected override string GetBaseDescription()
    {
        int damageAmount = GetDisplayDamageAmount(GetTrackedDamageAmount());
        return $"Deal {damageAmount} damage for each {element} card played this turn";
    }
}
