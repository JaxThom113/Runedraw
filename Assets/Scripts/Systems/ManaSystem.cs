using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[DefaultExecutionOrder(-100)]  // Run before other scripts so Instance is set before anyone accesses it
public class ManaSystem : Singleton<ManaSystem>
{
    [SerializeField] public ManaUI manaUI; 
  

    public int maxMana = 1; 
    private int currentMana;   
    public int additionalMana;
    private int startingMana = 1;
    private Coroutine borderIncreaseCoroutine;
    private const float BorderIncreaseSeconds = 3f;

    void Start() { }
    public void InitializeMana()
    { 
        startingMana = maxMana;
        currentMana = maxMana;  
        //additionalMana = 0;
        manaUI.UpdateMana(currentMana);        
        manaUI.SetManaBorderIncrease(false);
        if (manaUI.gameObject.activeInHierarchy)
            StartCoroutine(manaUI.StartRound());
    }
    private void OnEnable(){ 
        ActionSystem.AttachPerformer<SpendManaGA>(SpendManaPerformer);
        ActionSystem.AttachPerformer<RefillManaGA>(RefillManaPerformer); 
        ActionSystem.SubscribeReaction<ApplyStatusGA>(ApplyStatusPostReaction, ReactionTiming.POST);
        ActionSystem.SubscribeReaction<KillEnemyGA>(KillEnemyPostReaction, ReactionTiming.POST);
    }  
    private void OnDisable(){ 
        if (borderIncreaseCoroutine != null) StopCoroutine(borderIncreaseCoroutine);
        borderIncreaseCoroutine = null;

        ActionSystem.DetachPerformer<RefillManaGA>(); 
        ActionSystem.DetachPerformer<SpendManaGA>();
        ActionSystem.UnsubscribeReaction<ApplyStatusGA>(ApplyStatusPostReaction, ReactionTiming.POST);
        ActionSystem.UnsubscribeReaction<KillEnemyGA>(KillEnemyPostReaction, ReactionTiming.POST);
    }
    public bool HasEnoughMana(int manaAmount) {  
        return currentMana >= GetModifiedManaCost(manaAmount);
    } 

    public int GetModifiedManaCost(int manaAmount)
    {
        return manaAmount + additionalMana;
    }

    public void SetAdditionalMana(int amount)
    { 
        additionalMana = Mathf.Max(0, amount);
    }
 
    private IEnumerator SpendManaPerformer(SpendManaGA spendManaGA)
    { 
        int totalSpend = spendManaGA.manaAmount + additionalMana;
        currentMana -= totalSpend; 
        yield return StartCoroutine(manaUI.SpendManaCoroutine(totalSpend));
        manaUI.UpdateMana(currentMana);
    } 
    private IEnumerator RefillManaPerformer(RefillManaGA refillManaGA)
    {
        currentMana = refillManaGA.manaAmount;
        manaUI.UpdateMana(currentMana);
        if (manaUI.gameObject.activeInHierarchy)
            yield return StartCoroutine(manaUI.StartRound());
    } 

    private void ApplyStatusPostReaction(ApplyStatusGA applyStatusGA) 
    {  
        maxMana++;
        RefillManaGA refillManaGA = new(maxMana); // add one extra mana for each turn
        ActionSystem.Instance.AddReaction(refillManaGA); 
       

       
    } 
    
    private void KillEnemyPostReaction(KillEnemyGA killEnemyGA)
    {
        maxMana = startingMana; 
        additionalMana = 0;
        manaUI.ResetMana(maxMana);

        if (borderIncreaseCoroutine != null) StopCoroutine(borderIncreaseCoroutine);
        borderIncreaseCoroutine = null;

        manaUI.SetManaBorderIncrease(false);
    }

   

  
}
