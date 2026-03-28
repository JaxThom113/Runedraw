using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[CreateAssetMenu(fileName = "New Runes Card", menuName = "Card/Runes Card")]
public class RunesSO : CardSO
{
   [SerializeField] private int duration;
   public int Duration => duration;
}
