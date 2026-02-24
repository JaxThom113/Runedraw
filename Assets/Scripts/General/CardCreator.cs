using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
public class CardCreator : Singleton<CardCreator>
{  
    //Both enemy and player can use 
    [SerializeField] private Canvas canvas;
    [SerializeField] private ApplyCard applyCardPrefab; 
    public float cardScale = 0.25f;
    public ApplyCard CreateCard(Card card, Vector3 position, Quaternion rotation, bool isEnemy)
    {
        ApplyCard applyCard = Instantiate(applyCardPrefab, canvas.transform); 
        applyCard.transform.position = position; 
        applyCard.transform.rotation = rotation;  
        applyCard.Setup(card); 
        applyCard.InventoryCard = isEnemy;  
        if(isEnemy){
          applyCard.wrapper.transform.Find("CardTypeIcon").gameObject.SetActive(false); 
        }
        cardScale = isEnemy ? 0.15f : 0.25f;
        applyCard.transform.DOScale(Vector3.one*cardScale, 0.15f ); 
        return applyCard; 
    }
}
