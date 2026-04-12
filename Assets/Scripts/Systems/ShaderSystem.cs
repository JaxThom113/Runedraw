using System.Collections;
using UnityEngine;
using UnityEngine.Rendering.Universal;
using UnityEngine.VFX;

public class ShaderSystem : Singleton<ShaderSystem>
{
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
    }
    void OnDisable()
    {
        ActionSystem.UnsubscribeReaction<SpellCastGA>(SpellCastPreReaction, ReactionTiming.PRE);
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
       

        int areaType = LevelSystem.Instance.CurrentAreaType;
      

        Material selected = screenSpaceMaterials[areaType];
       

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
            StartCoroutine(SpawnPositionRandomizer(enemySpellCast));
            yield return new WaitForSeconds(spellCastDelay);
            enemySpellCast.SendEvent("OnPlay");
            enemySpellCast.SetInt("ElementType", spellIndex);
        }
    }

    void SpellCastPreReaction(SpellCastGA spellCastGA)
    {
        StartCoroutine(PlaySpellCastVfx(spellCastGA.spellIndex, spellCastGA.isPlayer));
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
