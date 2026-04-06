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

    public void RefreshHandLayout()
    {
        if(!gameObject.activeInHierarchy) return;
        StartCoroutine(UpdateCardPositions(null));
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

       
        if (splineContainer == null)
            yield break;

        if (cards.Count == 0)
            yield break;

        Spline spline = splineContainer.Spline;
        Transform splineTransform = splineContainer.transform;

        float cardSpacing = 1.2f / 10f;
        float firstCardPosition = 0.5f - (cards.Count - 1) * cardSpacing / 2f;

        for (int i = 0; i < cards.Count; i++)
        {
            float t = firstCardPosition + i * cardSpacing;
            Vector3 localPos = spline.EvaluatePosition(t);
            Vector3 worldPos = splineTransform.TransformPoint(localPos);
            worldPos -= transform.forward * (0.01f * i);

            Vector3 forwardSpline = splineTransform.TransformDirection(spline.EvaluateTangent(t)).normalized;
            Vector3 outSpline = splineTransform.TransformDirection(spline.EvaluateUpVector(t)).normalized;
            Quaternion rotation = Quaternion.LookRotation(-outSpline, Vector3.Cross(-outSpline, forwardSpline).normalized);

            cards[i].transform.DOMove(worldPos, duration);
            cards[i].transform.DORotateQuaternion(rotation, duration);
        }
        yield return new WaitForSeconds(duration); 
      
    }

}
