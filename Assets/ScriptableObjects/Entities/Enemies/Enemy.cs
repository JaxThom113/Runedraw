using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enemy : Entity
{
    public List<CardSOList> enemyDeck = new List<CardSOList>();
    public CardSO ultimateCard;
    public Material defaultEnemyMaterial;
    public Material enemyMaterial;

    public void Setup(EnemySO dataSO)
    {
        SetupBase(dataSO);
        enemyDeck = dataSO.enemyDeck;
        ultimateCard = dataSO.ultimateCard;
        defaultEnemyMaterial = dataSO.defaultEnemyMaterial;
        enemyMaterial = dataSO.ResolvedEnemyMaterial;
    }
}
