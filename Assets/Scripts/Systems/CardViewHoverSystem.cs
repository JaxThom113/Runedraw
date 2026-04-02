using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
public class CardViewHoverSystem : Singleton<CardViewHoverSystem>
{ 
    [SerializeField] private ApplyCard ApplyCardHover;
    // Start is called before the first frame update
    public void Show(Card card, Vector3 position)
    { 
        ApplyCardHover.transform.DOKill();
        ApplyCardHover.gameObject.SetActive(true);  
        ApplyCardHover.transform.DOScale(0.4f, 0.3f).SetEase(Ease.OutBack); 
        ApplyCardHover.transform.DOLocalMoveY(-125f, 0.3f).SetEase(Ease.OutBack);
        // Move to top of hierarchy so it renders on top of other UI elements
       
        
        ApplyCardHover.IsEnemyCard = false;
        ApplyCardHover.InventoryCard = false;
        ApplyCardHover.LootCard = false;
        ApplyCardHover.Setup(card); 
        ApplyCardHover.transform.position = position; 
        
    }

    public void Hide()
    {
        ApplyCardHover.gameObject.SetActive(false); 
        ApplyCardHover.transform.DOScale(0.2f, 0.3f).SetEase(Ease.InBack);
       // ApplyCardHover.transform.DOLocalMoveY(-25f, 0.3f).SetEase(Ease.InBack);
    }

    /// <summary>Call when stun (or other) changes modified mana so an open hover shows the new cost.</summary>
    public void RefreshHoverManaIfVisible()
    {
        if (ApplyCardHover == null || !ApplyCardHover.gameObject.activeInHierarchy || ApplyCardHover.card == null)
            return;
        int playerVunerableBonus = VunerableSystem.Instance != null ? VunerableSystem.Instance.GetTotalAdditionalDamage(true) : 0;
        int enemyVunerableBonus = VunerableSystem.Instance != null ? VunerableSystem.Instance.GetTotalAdditionalDamage(false) : 0;
        ApplyCardHover.RefreshManaCostText();
        ApplyCardHover.RefreshDescriptionText(playerVunerableBonus, enemyVunerableBonus);
    }
}
