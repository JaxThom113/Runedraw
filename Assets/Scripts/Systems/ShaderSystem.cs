using System.Collections;
using UnityEngine;
using UnityEngine.Rendering.Universal;
using UnityEngine.VFX;
using DG.Tweening;

public class ShaderSystem : Singleton<ShaderSystem>
{
    static readonly int TotalDistortionPropertyId = Shader.PropertyToID("_TotalDistortion");
    const float TotalDistortionTweenDuration = 2f;
    const float DistortionOutTarget = 0.7f;

    [Tooltip("Forward Renderer Data asset that contains your Full Screen Pass Renderer Feature (e.g. Ultra_PipelineAsset_ForwardRenderer).")]
    [SerializeField] UniversalRendererData forwardRendererData;

    private FullScreenPassRendererFeature fullScreenPassFeature;

    [SerializeField] Material[] screenSpaceMaterials = new Material[7]; 

    [SerializeField] VisualEffect playerSpellCast; 
    [SerializeField] VisualEffect enemySpellCast; 

    [SerializeField] float spawnRandom = 0.5f;

    public float spellCastDelay = 0.2f;
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
    void OnEnable()
    {
        ActionSystem.SubscribeReaction<SpellCastGA>(SpellCastPreReaction, ReactionTiming.PRE);
        ActionSystem.SubscribeReaction<StartRoundGA>(StartRoundPreReaction, ReactionTiming.PRE);
        ActionSystem.SubscribeReaction<KillEnemyGA>(KillEnemyPostReaction, ReactionTiming.POST);
    }
    void OnDisable()
    {
        ActionSystem.UnsubscribeReaction<SpellCastGA>(SpellCastPreReaction, ReactionTiming.PRE);
        ActionSystem.UnsubscribeReaction<StartRoundGA>(StartRoundPreReaction, ReactionTiming.PRE);
        ActionSystem.UnsubscribeReaction<KillEnemyGA>(KillEnemyPostReaction, ReactionTiming.POST);

        DOTween.Kill(this, false);
        if (fullScreenPassFeature != null
            && fullScreenPassFeature.passMaterial != null
            && fullScreenPassFeature.passMaterial.HasProperty(TotalDistortionPropertyId))
        {
            fullScreenPassFeature.passMaterial.SetFloat(TotalDistortionPropertyId, DistortionOutTarget);
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

    void Update()
    {
        SyncPassMaterialToAreaType();
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

        lastSyncedAreaType = areaType;
        fullScreenPassFeature.passMaterial = selected;
    } 
    public IEnumerator PlaySpellCastVfx(int spellIndex, bool isPlayer)
    {
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
        StartCoroutine(PlaySpellCastVfx(spellCastGA.spellIndex, spellCastGA.isPlayer));
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
        DOTween.Kill(this, false);
        DOTween.To(
            () => material.GetFloat(TotalDistortionPropertyId),
            x => material.SetFloat(TotalDistortionPropertyId, x),
            0f,
            TotalDistortionTweenDuration
        ).SetEase(Ease.InOutSine).SetTarget(this);
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
        DOTween.Kill(this, false);
        DOTween.To(
            () => material.GetFloat(TotalDistortionPropertyId),
            x => material.SetFloat(TotalDistortionPropertyId, x),
            DistortionOutTarget,
            TotalDistortionTweenDuration
        ).SetEase(Ease.InOutSine).SetTarget(this);
    }

    void KillEnemyPostReaction(KillEnemyGA killEnemyGA)
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
