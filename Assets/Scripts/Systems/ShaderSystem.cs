using System.Collections;
using UnityEngine;
using UnityEngine.Rendering.Universal;
using UnityEngine.VFX;
using DG.Tweening;

public class ShaderSystem : Singleton<ShaderSystem>
{
    static readonly int TotalDistortionPropertyId = Shader.PropertyToID("_TotalDistortion");
    static readonly int RangePropertyId = Shader.PropertyToID("_Range");
    static readonly int TwirlStepPropertyId = Shader.PropertyToID("_TwirlStep");

    [Header("Selected Material Distortion")]
    [SerializeField] float selectedMaterialDistortionDuration = 2f;
    [SerializeField] float selectedMaterialDistortionTarget = 0.7f;

    [Header("Transition Material Distortion")]
    [SerializeField] float transitionTweenDuration = 2f;
    [SerializeField] float transitionMidDelay = 1f;
    [SerializeField] float preTransitionDistortionClearDuration = 0.15f;
    const float TransitionTotalDistortionForwardTarget = 1f;
    const float TransitionTotalDistortionReverseTarget = 0f;
    const float TransitionRangeForwardTarget = 0f;
    const float TransitionRangeReverseTarget = 1f;
    const float TransitionTwirlForwardTarget = 50f;
    const float TransitionTwirlReverseTarget = 0f;

    [Tooltip("Forward Renderer Data asset that contains your Full Screen Pass Renderer Feature (e.g. Ultra_PipelineAsset_ForwardRenderer).")]
    [SerializeField] UniversalRendererData forwardRendererData;

    private FullScreenPassRendererFeature fullScreenPassFeature;

    [SerializeField] Material[] screenSpaceMaterials = new Material[7];
    [SerializeField] Material transitionScreenSpaceMaterial;

    [SerializeField] VisualEffect playerSpellCast; 
    [SerializeField] VisualEffect enemySpellCast; 

    [SerializeField] float spawnRandom = 0.5f;

    public float spellCastDelay = 0.2f;
    [Tooltip("Extra delay added before the spellcast VFX when the card has a SpecialEffect, giving the domain expansion time to land.")]
    public float specialEffectSpellCastExtraDelay = 1f;
    /*
            0 = Tutorial level
            1 = neutral
            2 = fire
            3 = wind
            4 = water
            5 = earth
            6 = FinalBoss level
    */

    int lastSyncedAreaType = int.MinValue;
    Coroutine areaTransitionRoutine;
    bool isAreaTransitionTweenRunning;
    void OnEnable()
    {
        ActionSystem.SubscribeReaction<SpellCastGA>(SpellCastPreReaction, ReactionTiming.PRE);
        ActionSystem.SubscribeReaction<StartRoundGA>(StartRoundPreReaction, ReactionTiming.PRE);
        ActionSystem.SubscribeReaction<LootCardGA>(LootCardPreReaction, ReactionTiming.PRE);
        ActionSystem.SubscribeReaction<LootCardPickupGA>(LootCardPickupPostReaction, ReactionTiming.POST);
        ActionSystem.SubscribeReaction<NextAreaGA>(NextAreaPreReaction, ReactionTiming.PRE);
        ActionSystem.SubscribeReaction<NextAreaGA>(NextAreaPostReaction, ReactionTiming.POST);
        SyncPassMaterialToAreaType();
    }
    void OnDisable()
    {
        ActionSystem.UnsubscribeReaction<SpellCastGA>(SpellCastPreReaction, ReactionTiming.PRE);
        ActionSystem.UnsubscribeReaction<StartRoundGA>(StartRoundPreReaction, ReactionTiming.PRE);
        ActionSystem.UnsubscribeReaction<LootCardGA>(LootCardPreReaction, ReactionTiming.PRE);
        ActionSystem.UnsubscribeReaction<LootCardPickupGA>(LootCardPickupPostReaction, ReactionTiming.POST);
        ActionSystem.UnsubscribeReaction<NextAreaGA>(NextAreaPreReaction, ReactionTiming.PRE);
        ActionSystem.UnsubscribeReaction<NextAreaGA>(NextAreaPostReaction, ReactionTiming.POST);

        DOTween.Kill(this, false);
        if (fullScreenPassFeature != null
            && fullScreenPassFeature.passMaterial != null
            && fullScreenPassFeature.passMaterial.HasProperty(TotalDistortionPropertyId))
        {
            fullScreenPassFeature.passMaterial.SetFloat(TotalDistortionPropertyId, selectedMaterialDistortionTarget);
        }
    }
    protected override void Awake()
    {
        base.Awake();
        if (fullScreenPassFeature == null && forwardRendererData != null)
        {
            foreach (ScriptableRendererFeature feature in forwardRendererData.rendererFeatures)
            {
                if (feature is FullScreenPassRendererFeature fs)
                {
                    fullScreenPassFeature = fs;
                    break;
                }
            }
        }

        if (fullScreenPassFeature == null)
            Debug.LogWarning($"{nameof(ShaderSystem)}: No {nameof(FullScreenPassRendererFeature)} found. Assign {nameof(forwardRendererData)} or {nameof(fullScreenPassFeature)}.", this);
    }

    void SyncPassMaterialToAreaType()
    {
        if (fullScreenPassFeature == null || LevelSystem.Instance == null)
            return;

        int areaType = LevelSystem.Instance.CurrentAreaType;
        if (screenSpaceMaterials == null || areaType < 0 || areaType >= screenSpaceMaterials.Length)
            return;

        Material selected = screenSpaceMaterials[areaType];
        if (selected == null)
            return;

        if (areaType == lastSyncedAreaType && fullScreenPassFeature.passMaterial == selected)
            return;

        lastSyncedAreaType = areaType;
        fullScreenPassFeature.passMaterial = selected;
    }

    void NextAreaPostReaction(NextAreaGA nextAreaGA)
    {
        if (isAreaTransitionTweenRunning)
            return;

        SyncPassMaterialToAreaType();
        TweenSelectedMaterialTotalDistortionIn();
    }

    void NextAreaPreReaction(NextAreaGA nextAreaGA)
    {
        if (areaTransitionRoutine != null)
            StopCoroutine(areaTransitionRoutine);

        areaTransitionRoutine = StartCoroutine(PlayAreaTransitionDistortion());
    }

    IEnumerator PlayAreaTransitionDistortion()
    {
        if (fullScreenPassFeature == null || transitionScreenSpaceMaterial == null)
            yield break;

        isAreaTransitionTweenRunning = true;
        Material currentMaterial = fullScreenPassFeature.passMaterial;
        if (currentMaterial != null && currentMaterial.HasProperty(TotalDistortionPropertyId))
        {
            DOTween.Kill(currentMaterial, false);
            Tween preClearTween = DOTween.To(
                () => currentMaterial.GetFloat(TotalDistortionPropertyId),
                x => currentMaterial.SetFloat(TotalDistortionPropertyId, x),
                0f,
                preTransitionDistortionClearDuration
            ).SetEase(Ease.OutSine).SetTarget(currentMaterial);
            yield return preClearTween.WaitForCompletion();
        }

        DOTween.Kill(transitionScreenSpaceMaterial, false);
        fullScreenPassFeature.passMaterial = transitionScreenSpaceMaterial;

        transitionScreenSpaceMaterial.SetFloat(TotalDistortionPropertyId, TransitionTotalDistortionReverseTarget);
        transitionScreenSpaceMaterial.SetFloat(RangePropertyId, TransitionRangeReverseTarget);
        transitionScreenSpaceMaterial.SetFloat(TwirlStepPropertyId, TransitionTwirlReverseTarget);
        //Distortion In
        Sequence forward = DOTween.Sequence().SetTarget(transitionScreenSpaceMaterial);
        forward.Join(DOTween.To(
            () => transitionScreenSpaceMaterial.GetFloat(TotalDistortionPropertyId),
            x => transitionScreenSpaceMaterial.SetFloat(TotalDistortionPropertyId, x),
            TransitionTotalDistortionForwardTarget,
            transitionTweenDuration
        )); 
        //Range In
        forward.Join(DOTween.To(
            () => transitionScreenSpaceMaterial.GetFloat(RangePropertyId),
            x => transitionScreenSpaceMaterial.SetFloat(RangePropertyId, x),
            TransitionRangeForwardTarget,
            transitionTweenDuration
        )); 
        //Twirl Step In
        forward.Join(DOTween.To(
            () => transitionScreenSpaceMaterial.GetFloat(TwirlStepPropertyId),
            x => transitionScreenSpaceMaterial.SetFloat(TwirlStepPropertyId, x),
            TransitionTwirlForwardTarget,
            transitionTweenDuration
        )); 

        //Step 1 complete
        yield return forward.WaitForCompletion();

        yield return new WaitForSeconds(transitionMidDelay);
        //Distortion Out
        Sequence reverse = DOTween.Sequence().SetTarget(transitionScreenSpaceMaterial);
        reverse.Join(DOTween.To(
            () => transitionScreenSpaceMaterial.GetFloat(TotalDistortionPropertyId),
            x => transitionScreenSpaceMaterial.SetFloat(TotalDistortionPropertyId, x),
            TransitionTotalDistortionReverseTarget,
            transitionTweenDuration
        )); 
        //Range Out
        reverse.Join(DOTween.To(
            () => transitionScreenSpaceMaterial.GetFloat(RangePropertyId),
            x => transitionScreenSpaceMaterial.SetFloat(RangePropertyId, x),
            TransitionRangeReverseTarget,
            transitionTweenDuration
        )); 
        //Twirl Step Out
        reverse.Join(DOTween.To(
            () => transitionScreenSpaceMaterial.GetFloat(TwirlStepPropertyId),
            x => transitionScreenSpaceMaterial.SetFloat(TwirlStepPropertyId, x),
            TransitionTwirlReverseTarget,
            transitionTweenDuration
        ));
        //Step 2 complete
        yield return reverse.WaitForCompletion();

        isAreaTransitionTweenRunning = false;
        areaTransitionRoutine = null; 
        //Sync the material to the area type
        SyncPassMaterialToAreaType();
        //Tween the selected material total distortion in 
        TweenSelectedMaterialTotalDistortionIn();
    }

    void TweenSelectedMaterialTotalDistortionIn()
    {
        if (fullScreenPassFeature == null || fullScreenPassFeature.passMaterial == null)
            return;

        Material selectedMaterial = fullScreenPassFeature.passMaterial;
        if (!selectedMaterial.HasProperty(TotalDistortionPropertyId))
            return;

        DOTween.Kill(selectedMaterial, false);
        selectedMaterial.SetFloat(TotalDistortionPropertyId, 0f);
        DOTween.To(
            () => selectedMaterial.GetFloat(TotalDistortionPropertyId),
            x => selectedMaterial.SetFloat(TotalDistortionPropertyId, x),
            selectedMaterialDistortionTarget,
            selectedMaterialDistortionDuration
        ).SetEase(Ease.InOutSine).SetTarget(selectedMaterial);
    }

    public IEnumerator PlaySpellCastVfx(int spellIndex, bool isPlayer, bool hasSpecialEffect = false)
    {
        if (hasSpecialEffect && specialEffectSpellCastExtraDelay > 0f)
            yield return new WaitForSeconds(specialEffectSpellCastExtraDelay);

        if (isPlayer)
        { 
            StartCoroutine(SpawnPositionRandomizer(playerSpellCast));
            playerSpellCast.SendEvent("OnPlay");
            playerSpellCast.SetInt("ElementType", spellIndex);
            yield return new WaitForSeconds(spellCastDelay);
            OverworldEnemy overworldEnemy = EnemySystem.Instance.overworldEnemy;
            UISystem.Instance.TransformShake(overworldEnemy.transform);
            
        }
        else
        { 
            
           
             yield return new WaitForSeconds(spellCastDelay);  
             yield return new WaitForSeconds(spellCastDelay); 
             StartCoroutine(SpawnPositionRandomizer(enemySpellCast));
            enemySpellCast.SendEvent("OnPlay");
            enemySpellCast.SetInt("ElementType", spellIndex);
        }
    }

    void SpellCastPreReaction(SpellCastGA spellCastGA)
    {
        StartCoroutine(PlaySpellCastVfx(spellCastGA.spellIndex, spellCastGA.isPlayer, spellCastGA.hasSpecialEffect));
    }

    void StartRoundPreReaction(StartRoundGA startRoundGA)
    {
        DistortionIn();
    }

    void DistortionIn()
    {
        if (fullScreenPassFeature == null)
            return;

        SyncPassMaterialToAreaType();

        if (fullScreenPassFeature.passMaterial == null
            || !fullScreenPassFeature.passMaterial.HasProperty(TotalDistortionPropertyId))
            return;

        Material material = fullScreenPassFeature.passMaterial;
        DOTween.Kill(material, false);
        DOTween.To(
            () => material.GetFloat(TotalDistortionPropertyId),
            x => material.SetFloat(TotalDistortionPropertyId, x),
            0f,
            selectedMaterialDistortionDuration
        ).SetEase(Ease.InOutSine).SetTarget(material);
    }

    void DistortionOut()
    {
        if (fullScreenPassFeature == null)
            return;

        SyncPassMaterialToAreaType();

        if (fullScreenPassFeature.passMaterial == null
            || !fullScreenPassFeature.passMaterial.HasProperty(TotalDistortionPropertyId))
            return;

        Material material = fullScreenPassFeature.passMaterial;
        DOTween.Kill(material, false);
        DOTween.To(
            () => material.GetFloat(TotalDistortionPropertyId),
            x => material.SetFloat(TotalDistortionPropertyId, x),
            selectedMaterialDistortionTarget,
            selectedMaterialDistortionDuration
        ).SetEase(Ease.InOutSine).SetTarget(material);
    }
    void LootCardPreReaction(LootCardGA lootCardGA)
    {
        DistortionIn();
    }
    void LootCardPickupPostReaction(LootCardPickupGA lootCardPickupGA)
    {
        DistortionOut();
    }
    IEnumerator SpawnPositionRandomizer(VisualEffect visualEffect){   
        Vector3 originalSpawnPosition = visualEffect.GetVector3("SpawnPosition");
        Vector3 SpawnPosition = visualEffect.GetVector3("SpawnPosition");  
        float SpawnRandom = visualEffect.GetFloat("SpawnRandom"); 
        Vector3 randomPosition = new Vector3(SpawnPosition.x + Random.Range(-spawnRandom, spawnRandom), SpawnPosition.y + Random.Range(-SpawnRandom, SpawnRandom), SpawnPosition.z); 
        visualEffect.SetVector3("SpawnPosition", randomPosition); 
        yield return new WaitForSeconds(spellCastDelay);
        visualEffect.SetVector3("SpawnPosition", originalSpawnPosition);

    }

  

}
