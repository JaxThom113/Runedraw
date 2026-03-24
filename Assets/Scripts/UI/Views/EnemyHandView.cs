using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine; 
using DG.Tweening;
using UnityEngine.Splines;

public class EnemyHandView : Singleton<EnemyHandView>
{ 
    //Same as HandView, but no card rotation
     [SerializeField] private SplineContainer splineContainer; 
    public float duration = 0.5f;  
    private List<ApplyCard> cards = new();  
    public bool IsTweening {get; set;} = false;
    public IEnumerator AddCard(ApplyCard card)
    {
        cards.Add(card);
        yield return UpdateCardPositions(card); 
    }  
    void OnDisable()
    {
        foreach(var card in cards)
        {
            Destroy(card.gameObject);
        }
        cards.Clear();
    }
    public IEnumerator RemoveEnemyCard(Card card)
    {  
        ApplyCard applyCard = GetApplyCard(card); 
        if (applyCard == null) { 
            Debug.LogError("Card not found in enemy hand");
            yield break;
        }; 
        if (applyCard.card.IsUltimate)
        {
            cards.Remove(applyCard);
            yield return StartCoroutine(UpdateCardPositions(null));
            yield return applyCard.StartCoroutine(applyCard.UltimateWindupRoutine());
            Destroy(applyCard.gameObject);
            yield break;
        }
        applyCard.transform.DOMove(Vector3.zero, duration);
        cards.Remove(applyCard);
        yield return StartCoroutine(UpdateCardPositions(null));
        Destroy(applyCard.gameObject, 0.5f);
        
    } 
    public void ClearEnemyHand() { 
        foreach(var card in cards) { 
            Destroy(card.gameObject, 0.5f);
        } 
        cards.Clear();
       
    }
    
   
    public List<Card> GetShownCards() { 
        return new List<Card>(cards.ConvertAll(applyCard => applyCard.card));
    } 
    private ApplyCard GetApplyCard(Card card){ 
        return cards.Where(applyCard => applyCard.card == card).FirstOrDefault();
    }
    
    private IEnumerator UpdateCardPositions(ApplyCard card)
    {  
        cards.RemoveAll(c => c == null || c.gameObject == null); 

       
        if(cards.Count == 0) yield break; // Stop if no cards
        float cardSpacing = 0.67f/10f; // spacing between cards along the spline
        float firstCardPosition = 0.5f - (cards.Count-1)*cardSpacing/2f; //Finds the center to place the first card 
        //spline is percentage based, so 0.5 is the center, but takes into account number of cards so new card will always be in the center
        Spline spline = splineContainer.Spline; 
        for (int i = 0; i < cards.Count; i++) { 
            float p = firstCardPosition + i*cardSpacing; 
            Vector3 splinePosition = spline.EvaluatePosition(p); //gets world position of spline
            Vector3 forwardSpline = spline.EvaluateTangent(p); // we need forward and up vectors to rotate the card
            Vector3 outSpline = spline.EvaluateUpVector(p);  
            //Quaternion rotation = Quaternion.LookRotation(-outSpline, Vector3.Cross(-outSpline, forwardSpline).normalized); // rotates card alone the spline axis
            cards[i].transform.DOMove(splinePosition +transform.position + 0.01f*i*Vector3.back, duration); 
            //cards[i].transform.DORotateQuaternion(rotation, duration);
        } 
        yield return new WaitForSeconds(duration); 
      
    }

}
