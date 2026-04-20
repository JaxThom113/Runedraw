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

    public List<CardSO> GetRandomCardsEnemy(CardSO ultimateCard, int amount)
    {
        List<CardSO> chosenCards = new List<CardSO>();
        CardSO firstCard = cards[Random.Range(0, cards.Count)];
        chosenCards.Add(firstCard);
        chosenCards.Add(ultimateCard);

        CardSO thirdCard = cards[Random.Range(0, cards.Count)];
        if (thirdCard == firstCard || thirdCard == ultimateCard)
        {
            int nextIndex = (cards.IndexOf(thirdCard) + 1) % cards.Count;
            thirdCard = cards[nextIndex];
            if (thirdCard == firstCard || thirdCard == ultimateCard)
            {
                thirdCard = cards[(nextIndex + 1) % cards.Count];
            }
        }
        chosenCards.Add(thirdCard);

        return chosenCards;
    }
}
