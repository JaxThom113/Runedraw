using System.Collections;
using UnityEngine;
using UnityEngine.VFX;
using DG.Tweening;

public class FogSystem : Singleton<FogSystem>
{
    const string TransformLocalProperty = "transform_Local";
    const string TransformWorldProperty = "transform_World";
    const string TotalAlphaProperty = "TotalAlpha";

    [Header("Debug")]
    public bool DisableFog = true;

    [Header("Fog VFX")]
    [SerializeField] private VisualEffect vfx;
    [SerializeField] float fogTweenDuration = 0.5f;

    [Header("Fog Presets by Area Type")]
    [Tooltip("Index guide: 0 Tutorial, 1 Neutral, 2 Fire, 3 Wind, 4 Water, 5 Earth, 6 Final Boss")]
    public FogPreset[] FogPresets = new FogPreset[7];

    [Header("Fallback if LevelSystem has no playerMovement.viewTarget")]
    public Transform viewPoint;

    // Mimics camera damping for smoother fog motion.
    public float fogSmoothing = 0.2f;
    private Vector3 currentFogPosition;
    private Vector3 currentVelocity;

    [SerializeField] private FogPreset currentPreset;
    private int lastSyncedAreaType = int.MinValue;
    private bool firstUpdate = true;
    private bool loggedMissingVfxError = false;

    private void OnEnable()
    {
        ActionSystem.SubscribeReaction<NextAreaGA>(NextAreaPostReaction, ReactionTiming.POST);
        ActionSystem.SubscribeReaction<StartRoundGA>(StartRoundPreReaction, ReactionTiming.PRE);
        ActionSystem.SubscribeReaction<LootCardPickupGA>(LootCardPickupPostReaction, ReactionTiming.POST);
        EnsureVfxReference();
        SyncFogToAreaType();
    }

    private void OnDisable()
    {
        ActionSystem.UnsubscribeReaction<NextAreaGA>(NextAreaPostReaction, ReactionTiming.POST);
        ActionSystem.UnsubscribeReaction<StartRoundGA>(StartRoundPreReaction, ReactionTiming.PRE);
        ActionSystem.UnsubscribeReaction<LootCardPickupGA>(LootCardPickupPostReaction, ReactionTiming.POST);
        if (vfx != null)
            DOTween.Kill(vfx, false);
    }

    private void Update()
    {
        if (!EnsureVfxReference())
            return;

        if (DisableFog)
        {
            if (vfx.gameObject.activeSelf)
                vfx.gameObject.SetActive(false);
            return;
        }

        if (!vfx.gameObject.activeSelf)
            vfx.gameObject.SetActive(true);

        if (CameraTransitionSystem.Instance != null && CameraTransitionSystem.Instance.inBattleScene)
            return;

        Transform view = ResolveViewTransform();
        if (view != null)
            UpdateFogPosition(view);
    }

    private void UpdateFogPosition(Transform view)
    {
        if (firstUpdate)
        {
            currentFogPosition = view.position;
            vfx.SetVector3(TransformWorldProperty, currentFogPosition);
            Vector3 localPos = vfx.transform.InverseTransformPoint(currentFogPosition);
            vfx.SetVector3(TransformLocalProperty, localPos);
            firstUpdate = false;
            return;
        }

        Vector3 target = view.position;
        currentFogPosition = Vector3.SmoothDamp(currentFogPosition, target, ref currentVelocity, fogSmoothing);
        vfx.SetVector3(TransformWorldProperty, currentFogPosition);
        Vector3 smoothedLocalPos = vfx.transform.InverseTransformPoint(currentFogPosition);
        vfx.SetVector3(TransformLocalProperty, smoothedLocalPos);
    }

    private Transform ResolveViewTransform()
    {
        if (LevelSystem.Instance != null &&
            LevelSystem.Instance.playerMovement != null &&
            LevelSystem.Instance.playerMovement.viewTarget != null)
        {
            return LevelSystem.Instance.playerMovement.viewTarget;
        }

        return viewPoint;
    }

    private void NextAreaPostReaction(NextAreaGA nextAreaGA)
    {
        StartCoroutine(PostReactionRoutine());
    }
    private void StartRoundPreReaction(StartRoundGA startRoundGA)
    {
        FogIn();
    }
    private void LootCardPickupPostReaction(LootCardPickupGA lootCardPickupGA)
    {
        FogOut();
    }
    private void FogIn()
    {
        if (!EnsureVfxReference() || !vfx.HasFloat(TotalAlphaProperty))
            return;

        DOTween.Kill(vfx, false);
        DOTween.To(
            () => vfx.GetFloat(TotalAlphaProperty),
            x => vfx.SetFloat(TotalAlphaProperty, x),
            0f,
            fogTweenDuration
        ).SetEase(Ease.InOutSine).SetTarget(vfx);
    }

    private void FogOut()
    {
        if (!EnsureVfxReference() || !vfx.HasFloat(TotalAlphaProperty))
            return;

        DOTween.Kill(vfx, false);
        DOTween.To(
            () => vfx.GetFloat(TotalAlphaProperty),
            x => vfx.SetFloat(TotalAlphaProperty, x),
            0.5f,
            fogTweenDuration
        ).SetEase(Ease.InOutSine).SetTarget(vfx);
    }

    IEnumerator PostReactionRoutine(){ 
        firstUpdate = true;
        SyncFogToAreaType();
        yield return new WaitForSeconds(3f);
        RestartFogSimulation();
    }
    // Cheapest reset path: restart simulation on the same VFX component (no instantiate/destroy).
    private void RestartFogSimulation()
    {
        if (!EnsureVfxReference() || DisableFog)
            return;

        vfx.Reinit();
        vfx.Play();
    }

    private void SyncFogToAreaType()
    {
        if (LevelSystem.Instance == null)
            return;

        int areaType = LevelSystem.Instance.CurrentAreaType;
        if (areaType == lastSyncedAreaType && currentPreset != null)
            return;

        ApplyAreaTypeFog(areaType);
    }

    private void ApplyAreaTypeFog(int areaType)
    {
        if (FogPresets == null || areaType < 0 || areaType >= FogPresets.Length)
            return;

        FogPreset selected = FogPresets[areaType];
        if (selected == null)
            return;

        lastSyncedAreaType = areaType;
        ApplyPreset(selected);
    }

    public void ApplyPreset(FogPreset preset)
    {
        if (!EnsureVfxReference() || preset == null)
            return;

        currentPreset = preset;
        Debug.Log($"FogSystem: Loaded FogPreset '{preset.name}' for area type {lastSyncedAreaType}.", this);

        // Spawn Settings
        vfx.SetFloat("SpawnSize", preset.spawnSize);
        vfx.SetFloat("ParticleSize", preset.particleSize);
        vfx.SetFloat("KillRadius", preset.killRadius);
        vfx.SetFloat("SingleBurstAmount", preset.singleBurstAmount);

        // Distance & Edges
        vfx.SetVector2("SmoothStepEdges", preset.smoothStepEdges);
        vfx.SetVector2("OutRemap", preset.outRemap);

        // Procedural Noise
        vfx.SetFloat("NoiseScale", preset.noiseScale);
        vfx.SetFloat("NoiseStrength", preset.noiseStrength);
        vfx.SetFloat("NoiseSpeed", preset.noiseSpeed);
        vfx.SetVector2("NoiseTiling", preset.noiseTiling);
        vfx.SetFloat("NoiseSmoothing", preset.noiseSmoothing);

        // Elemental Weights
        vfx.SetFloat("GradientStrength", preset.gradientStrength);
        vfx.SetFloat("SimpleStrength", preset.simpleStrength);
        vfx.SetFloat("VoronoiStrength", preset.voronoiStrength);
        vfx.SetVector4("FogTint", preset.fogTint);
        vfx.SetFloat("TintIntensity", preset.tintIntensity);

        // Texture Displacement
        if (preset.fogTexture != null)
            vfx.SetTexture("FogTexture", preset.fogTexture);

        vfx.SetVector2("TextureTiling", preset.textureTiling);
        vfx.SetFloat("TextureSpeed1", preset.textureSpeed1);
        vfx.SetFloat("TextureSpeed2", preset.textureSpeed2);
        vfx.SetFloat("GridSize", preset.gridSize);

        firstUpdate = true;
        currentVelocity = Vector3.zero;
    }

    private bool EnsureVfxReference()
    {
        if (vfx == null)
            vfx = GetComponent<VisualEffect>();

        if (vfx == null)
            vfx = GetComponentInChildren<VisualEffect>(true);

        if (vfx != null)
        {
            loggedMissingVfxError = false;
            return true;
        }

        if (!loggedMissingVfxError)
        {
            Debug.LogError("FogSystem: No VisualEffect found. Assign 'vfx' in inspector or place a VisualEffect on this object/child.", this);
            loggedMissingVfxError = true;
        }

        return false;
    }

}
