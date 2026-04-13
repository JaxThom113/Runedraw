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
    public ApplyCard CreateCard(Card card, Vector3 position, Quaternion rotation, bool isEnemy, Transform handParent = null)
    {
        Transform parent = handParent != null ? handParent : canvas.transform;
        ApplyCard applyCard = Instantiate(applyCardPrefab, position, rotation, parent);
        applyCard.IsEnemyCard = isEnemy;
        applyCard.InventoryCard = isEnemy;  
        applyCard.Setup(card); 
        if(isEnemy){
          applyCard.wrapper.transform.Find("CardTypeIcon").gameObject.SetActive(false); 
        }
        cardScale = isEnemy ? 0.2f : 0.25f;
        applyCard.transform.DOScale(Vector3.one*cardScale, 0.15f ); 
        return applyCard; 
    }
}
