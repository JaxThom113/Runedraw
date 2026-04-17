using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpecialEffect : Effect
{
    [SerializeField] public Material specialSpriteMaterial;
    [SerializeField] public Material domainExpansionMaterial;

    public override GameAction GetGameAction()
    {
        return new SpecialGA(specialSpriteMaterial, domainExpansionMaterial);
    }

    public override string GetDescription()
    {
        return "Unleash special";
    }
}
