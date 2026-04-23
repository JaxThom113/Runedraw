using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SeedSystem : PersistentSingleton<SeedSystem>
{
    [Header("Special Seeds")]
    [SerializeField] public List<SpecialSeedSO> specialSeeds = new List<SpecialSeedSO>();

    public SpecialSeedSO GetSpecialSeed(string seedName)
    {
        return specialSeeds.Find(e => e.name == seedName);
    }
}
