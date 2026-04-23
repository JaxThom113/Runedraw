using System.Collections;
using System.Collections.Generic;
using SerializeReferenceEditor;
using UnityEngine;

public enum AreaType
{ 
    Random,
    Neutral, 
    Fire, 
    Water, 
    Earth, 
    Wind
} 

[System.Serializable]
public class Area
{
    [SerializeField] public AreaType areaType; // dungeon element
    [SerializeField] public List<EnemySO> enemies; // if left empty, will use default enemies for the current area
    [SerializeField] public List<EnemySO> rareEnemies;  // if left empty, will use default rare enemies for the current area
    [SerializeField] public TextAsset levelCsv; // if left null, random level layout
    [SerializeField] public int numTorches; // default for a regular random dungeon is 30, set this in the SO
    [SerializeField] public bool fog; // choose whether to have fog enabled or disabled

    public int GetAreaTypeIndex()
    {
        return areaType switch
        {
            AreaType.Random => UnityEngine.Random.Range(1, 6),
            AreaType.Neutral => 1,
            AreaType.Fire => 2,
            AreaType.Wind => 3,
            AreaType.Water => 4,
            AreaType.Earth => 5,
            _ => 1
        };
    }
}