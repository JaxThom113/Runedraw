using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Card 
{ 
    // If we want to apply status effects to a card, we cannot apply them to the  ScriptableObject  
    // that would effect the actual card data. and other cards that are being played 
    // Would be a very restrictive architecture.
    // (IE the enemies cards)
    public string cardName => data.cardName; 
    public Sprite cardBorder => data.cardBorder;
    public Sprite cardIcon => data.cardIcon; 
    public Sprite cardTypeIcon => data.cardTypeIcon; 
    public Sprite cardElementIcon => data.cardElementIcon;
    public List<Effect> effects => data.effects;
    public AudioClip sound => data.sound;
    public bool IsUltimate => data != null && data.IsUltimate;
    //public Sprite cardTypeIcon => data.cardTypeIcon;
    //public Sprite cardElementIcon => data.cardElementIcon;
    public string cardDescription {get; private set; } 
    public int cardCost {get; private set; }
    public Element cardElement {get; private set; } 
    public CardType cardType {get; private set; } 

    public CardSO data;
    public Card(CardSO dataSO) { 
        data = dataSO; 
        cardElement = dataSO.cardElement;  
        DescribeCard();
        cardCost = dataSO.cardCost;     
        cardType = dataSO.cardType; 
    } 

    void DescribeCard()
    { 
        cardDescription = "";

        if(effects != null)
        { 
            foreach(var effect in effects)
            {
                cardDescription += effect.GetDescription() + ", "; 
            }

            if(cardElement != Element.None)
                cardDescription += cardElement.ToString();
            else
                cardDescription = cardDescription.TrimEnd(',', ' ');
        }
    } 
    public int GetElementIndex(){
        switch(cardElement){
            case Element.None:
                return 1;
            case Element.Fire:
                return 2;
            case Element.Air:
                return 3;
            case Element.Water:
                return 4;
            case Element.Earth:
                return 5; 
            default:
                return 1;
        }
    }
}
