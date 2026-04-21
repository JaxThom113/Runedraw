using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

public class ApplyShieldEffect : Effect
{
    [FormerlySerializedAs("shieldAmount")]
    [SerializeField] public int magnitude;
    public override int Magnitude => magnitude;

    public override GameAction GetGameAction()
    {
        return new ApplyShieldGA(magnitude, isPlayer);
    } 
    protected override string GetBaseDescription()
    {
        return $"Apply {magnitude} shield";
    }
}
