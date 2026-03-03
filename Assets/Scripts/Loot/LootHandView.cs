using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine; 
using DG.Tweening;
using UnityEngine.Splines; 
using UnityEngine.Events;


public class LootHandView : Singleton<LootHandView>
{
    [SerializeField] private SplineContainer splineContainer; 
    public float duration = 0.01f;  
    private List<ApplyCard> cards = new();  
    void OnDisable()
    {
        foreach(var card in cards)
        {
            Destroy(card.gameObject);
        }
        cards.Clear();
    }
    public void AddCardHelper(ApplyCard card) { 
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
        StartCoroutine(UpdateCardPositions(null));
        return applyCard;
        
    } 
    private ApplyCard GetApplyCard(Card card){ 
        return cards.Where(applyCard => applyCard.card == card).FirstOrDefault();
    }
    
    private IEnumerator UpdateCardPositions(ApplyCard card)
    {  
        cards.RemoveAll(c => c == null || c.gameObject == null); 

       
        if(cards.Count == 0) yield break; // Stop if no cards
        float cardSpacing = 3f/10f; // spacing between cards along the spline
        float firstCardPosition = 0.5f - (cards.Count-1)*cardSpacing/2f; //Finds the center to place the first card 
        //spline is percentage based, so 0.5 is the center, but takes into account number of cards so new card will always be in the center
        Spline spline = splineContainer.Spline; 
        for (int i = 0; i < cards.Count; i++) { 
            float p = firstCardPosition + i*cardSpacing; 
            Vector3 splinePosition = spline.EvaluatePosition(p); //gets world position of spline
            Vector3 forwardSpline = spline.EvaluateTangent(p); // we need forward and up vectors to rotate the card
            Vector3 outSpline = spline.EvaluateUpVector(p);  
            cards[i].transform.DOMove(splinePosition +transform.position + 0.01f*i*Vector3.back, duration); 
        } 
        yield return new WaitForSeconds(duration); 
      
    }
}
