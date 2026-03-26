using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[System.Serializable]
public class CardSOList
{
    public List<CardSO> enemyHand = new List<CardSO>();
}

[CreateAssetMenu(fileName = "New Enemy", menuName = "Entities/Enemy")]
public class EnemySO : EntitySO
{
    [Tooltip("Each element is a list of cards (e.g. one list per phase or tier).")]
    public List<CardSOList> enemyDeck = new List<CardSOList>();

    public CardSO ultimateCard;
  
}
