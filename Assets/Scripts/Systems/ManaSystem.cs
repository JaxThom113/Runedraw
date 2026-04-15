using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[DefaultExecutionOrder(-100)]  // Run before other scripts so Instance is set before anyone accesses it
public class ManaSystem : Singleton<ManaSystem>
{
    [SerializeField] public ManaUI manaUI; 
  

    [SerializeField] private int maxMana = 3; 
    private int currentMana;   
    public int additionalMana;
    
    private int originalMaxMana;
    private Coroutine borderIncreaseCoroutine;

    /// <summary>
    /// When true, the next <see cref="ApplyStatusGA"/> POST reaction will not bump max mana or queue refill.
    /// Used for the first battle <see cref="StartRoundGA"/>: that flow always runs an empty ApplyStatus pass, and the mana ramp
    /// (often +2 from 3→5) lines up with the draw phase and looks like the draw count (5 cards) — then <see cref="InitializeMana"/>
    /// resets back to baseline, causing a 3→5→3 flicker.
    /// </summary>
    private bool suppressNextApplyStatusManaRamp;

    protected override void Awake()
    {
        base.Awake();
        originalMaxMana = maxMana;
    }

    void Start() { }

    /// <summary>Call immediately before the first battle <see cref="StartRoundGA"/> Perform (e.g. from MatchSetup).</summary>
    public void SuppressNextApplyStatusManaRamp()
    {
        suppressNextApplyStatusManaRamp = true;
    }

  
    public void InitializeMana()
    { 
        maxMana = originalMaxMana;
        currentMana = maxMana;
        manaUI.ResetMana(maxMana);
        manaUI.UpdateMana(currentMana);        
        if (manaUI.gameObject.activeInHierarchy)
            manaUI.PlayStartRoundFireAndForget();
    }
    private void OnEnable(){ 
        ActionSystem.AttachPerformer<SpendManaGA>(SpendManaPerformer);
        ActionSystem.AttachPerformer<RefillManaGA>(RefillManaPerformer);
        ActionSystem.AttachPerformer<ManaRefreshGA>(ManaRefreshPerformer);
        ActionSystem.SubscribeReaction<ApplyStatusGA>(ApplyStatusPostReaction, ReactionTiming.POST);
        ActionSystem.SubscribeReaction<LootCardPickupGA>(LootCardPickupPostReaction, ReactionTiming.POST);
    }  
    private void OnDisable(){ 
        if (borderIncreaseCoroutine != null) StopCoroutine(borderIncreaseCoroutine);
        borderIncreaseCoroutine = null;

        ActionSystem.DetachPerformer<RefillManaGA>(); 
        ActionSystem.DetachPerformer<SpendManaGA>();
        ActionSystem.DetachPerformer<ManaRefreshGA>();
        ActionSystem.UnsubscribeReaction<ApplyStatusGA>(ApplyStatusPostReaction, ReactionTiming.POST);
        ActionSystem.UnsubscribeReaction<LootCardPickupGA>(LootCardPickupPostReaction, ReactionTiming.POST);
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
        yield return StartCoroutine(manaUI.WaitForSpendMana(totalSpend));
        manaUI.UpdateMana(currentMana);
    } 
    private IEnumerator RefillManaPerformer(RefillManaGA refillManaGA)
    {
        currentMana = refillManaGA.manaAmount;
        manaUI.UpdateMana(currentMana);
        if (manaUI.gameObject.activeInHierarchy)
            yield return StartCoroutine(manaUI.WaitForStartRound());
    } 

    private void ApplyStatusPostReaction(ApplyStatusGA applyStatusGA) 
    {   
        if (suppressNextApplyStatusManaRamp)
        {
            suppressNextApplyStatusManaRamp = false;
            return;
        }
        Debug.Log("Max Mana Increase: " + maxMana);
        maxMana+=2;
        RefillManaGA refillManaGA = new(maxMana); // add one extra mana for each turn
        ActionSystem.Instance.AddReaction(refillManaGA); 
       

       
    } 

    private IEnumerator ManaRefreshPerformer(ManaRefreshGA _)
    {
        PrepareManaUiForGameViewDisabled();
        yield return null;
        yield return new WaitForEndOfFrame();
    }
    
    private void LootCardPickupPostReaction(LootCardPickupGA lootCardPickupGA)
    { 
      
        maxMana = originalMaxMana;
        additionalMana = 0;
        currentMana = maxMana;
        manaUI.ResetMana(maxMana);
        manaUI.UpdateMana(currentMana);

        if (borderIncreaseCoroutine != null) StopCoroutine(borderIncreaseCoroutine);
        borderIncreaseCoroutine = null;

        manaUI.SetManaBorderIncrease(false);
    }

    public void RefreshManaUiNodes()
    {
        maxMana = originalMaxMana;
        additionalMana = 0;
        currentMana = maxMana;
        manaUI.ResetMana(maxMana);
        manaUI.UpdateMana(currentMana);
    }

    /// <summary>
    /// Resets mana to the battle baseline and stops running mana UI animations while the mana UI is still active.
    /// Call before disabling the game view so <see cref="ManaUI"/> is not torn down mid-animation.
    /// </summary>
    public void PrepareManaUiForGameViewDisabled()
    {
        if (borderIncreaseCoroutine != null)
        {
            StopCoroutine(borderIncreaseCoroutine);
            borderIncreaseCoroutine = null;
        }
        RefreshManaUiNodes();
    }

   

  
}
