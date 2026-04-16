using System.Collections;
using UnityEngine;
using UnityEngine.VFX;

public class FogSystem : Singleton<FogSystem>
{
    const string TransformLocalProperty = "transform_Local";
    const string TransformWorldProperty = "transform_World";

    [Header("Debug")]
    public bool DisableFog = true;

    [Header("Fog VFX")]
    [SerializeField] private VisualEffect vfx;

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

    private void OnEnable()
    {
        ActionSystem.SubscribeReaction<NextAreaGA>(NextAreaPostReaction, ReactionTiming.POST);
        SyncFogToAreaType();
    }

    private void OnDisable()
    {
        ActionSystem.UnsubscribeReaction<NextAreaGA>(NextAreaPostReaction, ReactionTiming.POST);
    }

    private void Update()
    {
        if (vfx == null)
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
        SyncFogToAreaType();
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
        if (vfx == null || preset == null)
            return;

        currentPreset = preset;

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

}
