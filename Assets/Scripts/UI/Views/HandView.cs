using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine; 
using DG.Tweening;
using UnityEngine.Splines; 
using UnityEngine.Events;

public class HandView : Singleton<HandView>
{
    [SerializeField] private SplineContainer splineContainer; 
    public float duration = 0.5f;  
    private List<ApplyCard> cards = new();  
    public bool IsTweening {get; set;} = false;  
    //public static UnityEvent<ApplyCard> OnHandUpdated = new UnityEvent<ApplyCard>();

    // private void Awake() { 
    //    //OnHandUpdated.AddListener(AddCardHelper); 
        
    // }  
    void OnEnable()
    {
        ActionSystem.AttachPerformer<UpdateApplyCardGA>(UpdateApplyCardPerformer);
    }

    void OnDisable()
    {
        ActionSystem.DetachPerformer<UpdateApplyCardGA>();
        foreach(var card in cards)
        {
            Destroy(card.gameObject);
        }
        cards.Clear();
    }
    public void AddCardHelper(ApplyCard card) {  
        if(!gameObject.activeInHierarchy) return;
        StartCoroutine(AddCard(card)); 
    }
    public IEnumerator AddCard(ApplyCard card)
    {
        cards.Add(card);
        yield return UpdateCardPositions(card); 
    } 
    public ApplyCard RemoveCard(Card card)
    { 
        ApplyCard applyCard = GetApplyCard(card); 
        if (applyCard == null) return null;
        cards.Remove(applyCard);
        if(!gameObject.activeInHierarchy) return null;
        StartCoroutine(UpdateCardPositions(null));
        return applyCard;
        
    } 

    public void RefreshVisibleCardCosts()
    {
        cards.RemoveAll(c => c == null || c.gameObject == null);
        foreach (ApplyCard applyCard in cards)
        { 
            
            applyCard.RefreshManaCostText();
        }
        CardViewHoverSystem.Instance?.RefreshHoverManaIfVisible();
    } 
    public void RefreshVisibleCardDescriptions()
    {
        int playerVunerableBonus = VunerableSystem.Instance != null ? VunerableSystem.Instance.GetTotalAdditionalDamage(true) : 0;
        int enemyVunerableBonus = VunerableSystem.Instance != null ? VunerableSystem.Instance.GetTotalAdditionalDamage(false) : 0;
        RefreshVisibleCardDescriptions(playerVunerableBonus, enemyVunerableBonus);
    }

    public void RefreshVisibleCardDescriptions(int playerVunerableBonus, int enemyVunerableBonus)
    {
        foreach (ApplyCard applyCard in cards)
        {
            applyCard.RefreshDescriptionText(playerVunerableBonus, enemyVunerableBonus);
        }
    }

    private IEnumerator UpdateApplyCardPerformer(UpdateApplyCardGA updateApplyCardGA)
    {
        int playerVunerableBonus = VunerableSystem.Instance != null ? VunerableSystem.Instance.GetTotalAdditionalDamage(true) : 0;
        int enemyVunerableBonus = VunerableSystem.Instance != null ? VunerableSystem.Instance.GetTotalAdditionalDamage(false) : 0;
        RefreshVisibleCardCosts();
        RefreshVisibleCardDescriptions(playerVunerableBonus, enemyVunerableBonus);
        yield return null;
    }

    private ApplyCard GetApplyCard(Card card){ 
        return cards.Where(applyCard => applyCard.card == card).FirstOrDefault();
    }
    
    private IEnumerator UpdateCardPositions(ApplyCard card)
    {  
        cards.RemoveAll(c => c == null || c.gameObject == null); 

       
        if(cards.Count == 0) yield break; // Stop if no cards
        float cardSpacing = 1.2f/10f; // spacing between cards along the spline
        float firstCardPosition = 0.5f - (cards.Count-1)*cardSpacing/2f; //Finds the center to place the first card 
        //spline is percentage based, so 0.5 is the center, but takes into account number of cards so new card will always be in the center
        Spline spline = splineContainer.Spline; 
        for (int i = 0; i < cards.Count; i++) { 
            float p = firstCardPosition + i*cardSpacing; 
            Vector3 splinePosition = spline.EvaluatePosition(p); //gets world position of spline
            Vector3 forwardSpline = spline.EvaluateTangent(p); // we need forward and up vectors to rotate the card
            Vector3 outSpline = spline.EvaluateUpVector(p);  
            Quaternion rotation = Quaternion.LookRotation(-outSpline, Vector3.Cross(-outSpline, forwardSpline).normalized); // rotates card alone the spline axis
            cards[i].transform.DOMove(splinePosition +transform.position + 0.01f*i*Vector3.back, duration); 
            cards[i].transform.DORotateQuaternion(rotation, duration);
        } 
        yield return new WaitForSeconds(duration); 
      
    }

}
