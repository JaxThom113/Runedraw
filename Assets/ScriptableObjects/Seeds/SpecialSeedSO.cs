using System.Collections;
using System.Collections.Generic;
using SerializeReferenceEditor;
using UnityEngine;

[CreateAssetMenu(fileName = "New Special Seed", menuName = "CustomLevels/SpecialSeed")]
public class SpecialSeedSO : ScriptableObject
{
    [Header("Areas")]
    [SerializeField] public List<Area> areas = new List<Area>();
}