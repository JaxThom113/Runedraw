using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LootCardBank : MonoBehaviour
{
    public List<CardSO> cards;  
    
    public List<CardSO> GetRandomCards(int amount)
    { 
        List<CardSO> chosenCards = new List<CardSO>(); 

        while(chosenCards.Count < amount)
        {
            CardSO chosenCard = cards[Random.Range(0, cards.Count)];

            if(!chosenCards.Contains(chosenCard))
                chosenCards.Add(chosenCard);  
        } 

        return chosenCards;
    }

    public List<CardSO> GetRandomCardsEnemy(CardSO ultimateCard)
    {
        List<CardSO> chosenCards = new List<CardSO>();
        if (ultimateCard != null)
            chosenCards.Add(ultimateCard);
        return chosenCards;
    }
}
