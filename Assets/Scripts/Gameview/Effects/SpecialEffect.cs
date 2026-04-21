using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpecialEffect : Effect
{
    [SerializeField] public Material specialSpriteMaterial;
    [SerializeField] public Material domainExpansionMaterial;
    [SerializeField] public bool Boss = false;

    public override GameAction GetGameAction()
    {
        return new SpecialGA(specialSpriteMaterial, domainExpansionMaterial, isPlayer, Boss);
    }

    protected override string GetBaseDescription()
    {
        return "Unleash special";
    }
}
