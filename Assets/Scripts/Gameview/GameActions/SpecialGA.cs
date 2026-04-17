using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpecialGA : GameAction
{
    public Material specialSpriteMaterial;
    public Material domainExpansionMaterial;

    public SpecialGA(Material specialSpriteMaterial, Material domainExpansionMaterial)
    {
        this.specialSpriteMaterial = specialSpriteMaterial;
        this.domainExpansionMaterial = domainExpansionMaterial;
    }
}
