using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine; 
using DG.Tweening;
using UnityEngine.Splines;

public class EnemyHandView : Singleton<EnemyHandView>
{ 
    [SerializeField] private SplineContainer splineContainer;
    [SerializeField]
    [Tooltip("If on, cards match this object's rotation each layout tick (put on your world-space hand/canvas root). If off, use spline tilt like the player hand.")]
    private bool alignCardRotationToHand = true;
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
        if (applyCard == null) yield break; 
        if (applyCard.card.IsUltimate)
        {
            cards.Remove(applyCard);
            yield return StartCoroutine(UpdateCardPositions(null));
            yield return applyCard.StartCoroutine(applyCard.UltimateWindupRoutine());
            Destroy(applyCard.gameObject);
            yield break;
        }
        Transform discard = EnemySystem.Instance.enemyDiscardPileTransform;
        applyCard.transform.DOMove(discard.position, duration);
        applyCard.transform.DORotateQuaternion(discard.rotation, duration);
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

    public ApplyCard GetApplyCardForCard(Card card) => GetApplyCard(card);
    
    private IEnumerator UpdateCardPositions(ApplyCard card)
    {  
        cards.RemoveAll(c => c == null || c.gameObject == null); 

        if (splineContainer == null)
            yield break;

        if (cards.Count == 0)
            yield break;

        Spline spline = splineContainer.Spline;
        Transform splineTransform = splineContainer.transform;

        float cardSpacing = 1.5f / 10f;
        float firstCardPosition = 0.5f - (cards.Count - 1) * cardSpacing / 2f;

        for (int i = 0; i < cards.Count; i++)
        {
            float t = firstCardPosition + i * cardSpacing;
            Vector3 localPos = spline.EvaluatePosition(t);
            Vector3 worldPos = splineTransform.TransformPoint(localPos);
            float depth = 0.01f * i;
            worldPos -= transform.forward * depth;

            cards[i].transform.DOMove(worldPos, duration);

            if (alignCardRotationToHand)
                cards[i].transform.DORotateQuaternion(transform.rotation, duration);
            else
            {
                Vector3 forwardSpline = splineTransform.TransformDirection(spline.EvaluateTangent(t)).normalized;
                Vector3 outSpline = splineTransform.TransformDirection(spline.EvaluateUpVector(t)).normalized;
                Quaternion rotation = Quaternion.LookRotation(-outSpline, Vector3.Cross(-outSpline, forwardSpline).normalized);
                cards[i].transform.DORotateQuaternion(rotation, duration);
            }
        }

        yield return new WaitForSeconds(duration);
    }

}
