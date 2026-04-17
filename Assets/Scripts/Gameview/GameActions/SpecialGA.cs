using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpecialGA : GameAction
{
    public Material specialSpriteMaterial;
    public Material domainExpansionMaterial;
    public bool isPlayer;
    public bool isBoss;

    public SpecialGA(Material specialSpriteMaterial, Material domainExpansionMaterial, bool isPlayer, bool isBoss = false)
    {
        this.specialSpriteMaterial = specialSpriteMaterial;
        this.domainExpansionMaterial = domainExpansionMaterial;
        this.isPlayer = isPlayer;
        this.isBoss = isBoss;
    }
}
