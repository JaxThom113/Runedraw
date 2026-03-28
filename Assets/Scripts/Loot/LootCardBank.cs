using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LootCardBank : MonoBehaviour
{
    public List<CardSO> cards;  
    
    public List<CardSO> GetRandomCards(int amount)
    { 
        List<CardSO> chosenCards = new List<CardSO>(); 
        while(amount > 0){
            CardSO chosenCard = cards[Random.Range(0, cards.Count)];
            if(!chosenCards.Contains(chosenCard)){
                chosenCards.Add(chosenCard);  
                cards.Remove(chosenCard);
                amount--;
            }
        } 
        cards.AddRange(chosenCards);
        return chosenCards;
    }

    public List<CardSO> GetRandomCardsEnemy(CardSO ultimateCard)
    { 
        const int lootCount = 3;
        List<CardSO> chosenCards = new List<CardSO>();
        List<CardSO> takenFromBank = new List<CardSO>();

        for (int i = 0; i < lootCount; i++)
        {
            if (i == 1 && ultimateCard != null && !chosenCards.Contains(ultimateCard))
            {
                chosenCards.Add(ultimateCard);
                continue;
            }

            if (cards == null || cards.Count == 0)
                break;

            while (cards.Count > 0)
            {
                CardSO candidate = cards[Random.Range(0, cards.Count)];
                if (chosenCards.Contains(candidate))
                    continue;
                chosenCards.Add(candidate);
                cards.Remove(candidate);
                takenFromBank.Add(candidate);
                break;
            }
        }

        cards.AddRange(takenFromBank);
        return chosenCards;
    }
}
