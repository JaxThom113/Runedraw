using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : Entity
{
    public List<CardSO> playerDeck;

    public void Setup(PlayerSO playerData)
    {
        SetupBase(playerData); 
        playerDeck = new List<CardSO>(playerData.playerDeck);
    }  
    public void SetupDeck(List<CardSO> deck)
    {
        playerDeck = new List<CardSO>(deck);
    }
    public void AddCardToDeck(CardSO card)
    { 
        if(card == null) return;
        playerDeck.Add(card);
    }
    public void RemoveCardFromDeck(CardSO card)
    { 
        if(card == null) return;
        playerDeck.Remove(card);
    }
}
