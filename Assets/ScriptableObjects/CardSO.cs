using System.Collections;
using System.Collections.Generic;
using SerializeReferenceEditor;
using UnityEngine;

public enum Element 
{ 
    None, 
    Fire, 
    Water, 
    Earth, 
    Air
} 
public enum CardType
{
    Attaction, 
    Action,
    Runes,
    Terrunes
}

public class CardSO : ScriptableObject
{
    [SerializeField] public string cardName; 
    [SerializeField] public int cardCost; 
    [SerializeField] public Sprite cardBorder; 
    [SerializeField] public Sprite cardIcon; 
    [SerializeField] public string cardDescription;  
    [SerializeField] public Element cardElement;  
    [SerializeField] public Sprite cardElementIcon;  
    [SerializeField] public CardType cardType; 
    [SerializeField] public Sprite cardTypeIcon;
    [SerializeReference, SR(typeof(Effect))]
    [SerializeField] public List<Effect> effects = new List<Effect>();  
    
    
}
