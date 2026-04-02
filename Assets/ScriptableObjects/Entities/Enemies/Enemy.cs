using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enemy : Entity
{
    public List<CardSOList> enemyDeck = new List<CardSOList>();
    public CardSO ultimateCard;
    public Material enemyMaterial;

    public void Setup(EnemySO dataSO)
    {
        SetupBase(dataSO);
        enemyDeck = dataSO.enemyDeck;
        ultimateCard = dataSO.ultimateCard;
        enemyMaterial = dataSO.enemyMaterial;
    }
}
