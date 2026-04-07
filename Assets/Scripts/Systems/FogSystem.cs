using System.Collections;
using UnityEngine;
using UnityEngine.VFX;
using DG.Tweening;

public class FogSystem : Singleton<FogSystem>
{
    const string FogHideDistanceProperty = "HideDistance";
    const string PlayerPositionProperty = "PlayerPosition";

    [Header("Fog VFX (one active by area type)")]
    public VisualEffect normalFog;
    public VisualEffect fireFog;
    public VisualEffect windFog;
    public VisualEffect waterFog;
    public VisualEffect earthFog;
    public VisualEffect finalBossFog;

    [Header("Hide distance tween")]
    public float hideDistanceLowerBound = 0.3f;
    public float hideDistanceUpperBound = 5.0f;
    public float hideDistanceTweenDuration = 1f;

    [Header("Fallback if LevelSystem has no playerMovement.viewTarget")]
    public Transform viewPoint;

    private VisualEffect currentFog;
    private int lastSyncedAreaType = int.MinValue;
    Tween fogHideDistanceTween;

    void Update()
    {
        SyncFogToAreaType();

        if (currentFog == null)
            return;

        if (CameraTransitionSystem.Instance != null && CameraTransitionSystem.Instance.inBattleScene)
            return;

        Transform view = ResolveViewTransform();
        if (view != null)
            currentFog.SetVector3(PlayerPositionProperty, view.position);
    }

    Transform ResolveViewTransform()
    {
        if (LevelSystem.Instance != null &&
            LevelSystem.Instance.playerMovement != null &&
            LevelSystem.Instance.playerMovement.viewTarget != null)
            return LevelSystem.Instance.playerMovement.viewTarget;
        return viewPoint;
    }

    void SyncFogToAreaType()
    {
        if (LevelSystem.Instance == null)
            return;

        int areaType = LevelSystem.Instance.CurrentAreaType;
        if (areaType == lastSyncedAreaType && currentFog != null)
            return;

        lastSyncedAreaType = areaType;
        ApplyAreaTypeFog(areaType);
    }

    void ApplyAreaTypeFog(int areaType)
    {
        VisualEffect selected;
        switch (areaType)
        {
            case 0:
            case 1:
                selected = normalFog;
                break;
            case 2:
                selected = fireFog;
                break;
            case 3:
                selected = windFog;
                break;
            case 4:
                selected = waterFog;
                break;
            case 5:
                selected = earthFog;
                break;
            case 6:
                selected = finalBossFog;
                break;
            default:
                selected = normalFog;
                break;
        }

        if (selected == null)
            selected = normalFog;

        VisualEffect[] all = { normalFog, fireFog, windFog, waterFog, earthFog, finalBossFog };
        foreach (VisualEffect fx in all)
        {
            if (fx == null)
                continue;
            bool on = fx == selected;
            if (fx.gameObject.activeSelf != on)
                fx.gameObject.SetActive(on);
        }

        currentFog = selected;
    }

    void KillFogHideDistanceTween()
    {
        fogHideDistanceTween?.Kill();
        fogHideDistanceTween = null;
    }

    public void TweenFogHideDistanceToUpper()
    {
        SyncFogToAreaType();
        if (currentFog == null)
            return;

        KillFogHideDistanceTween();
        currentFog.SetFloat(FogHideDistanceProperty, hideDistanceLowerBound);
        fogHideDistanceTween = DOTween.To(
            () => currentFog.GetFloat(FogHideDistanceProperty),
            x => currentFog.SetFloat(FogHideDistanceProperty, x),
            hideDistanceUpperBound,
            hideDistanceTweenDuration
        ).SetTarget(this);
    }

    public void BeginFogHideDistanceTweenToLower()
    {
        SyncFogToAreaType();
        if (currentFog == null)
            return;

        KillFogHideDistanceTween();
        fogHideDistanceTween = DOTween.To(
            () => currentFog.GetFloat(FogHideDistanceProperty),
            x => currentFog.SetFloat(FogHideDistanceProperty, x),
            hideDistanceLowerBound,
            hideDistanceTweenDuration
        ).SetTarget(this);
    }

    public IEnumerator FogHideDistanceTweenToLowerRoutine()
    {
        BeginFogHideDistanceTweenToLower();
        if (fogHideDistanceTween == null)
            yield break;
        yield return fogHideDistanceTween.WaitForCompletion();
        fogHideDistanceTween = null;
    }

    void OnDestroy()
    {
        KillFogHideDistanceTween();
    }
}
