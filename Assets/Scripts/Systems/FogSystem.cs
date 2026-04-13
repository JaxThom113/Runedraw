// using System.Collections;
using UnityEngine;
// using UnityEngine.VFX;
// using DG.Tweening;

public class FogSystem : Singleton<FogSystem>
{
    /*
    const string FogHideDistanceProperty = "HideDistance";
    const string PlayerPositionProperty = "PlayerPosition";

    [Header("Debug")]
    public bool DisableFog = true;

    [Header("Fog VFX (one active by area type)")]
    public VisualEffect[] Fog;
    // 0: Normal
    // 1: Fire
    // 2: Wind
    // 3: Water
    // 4: Earth
    // 5: Final Boss
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
        if (DisableFog)
        {
            foreach (VisualEffect fog in Fog)
            {
                if (fog == null)
                    continue;
                fog.gameObject.SetActive(false);
            }
            return;
        }
        else
        {
            foreach (VisualEffect fog in Fog)
            {
                if (fog == null)
                    continue;
                fog.gameObject.SetActive(true);
            }
        }
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
        selected = Fog[areaType];
        selected.gameObject.SetActive(true);

        if (selected == null)
            return;

        VisualEffect[] all = Fog;
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
    */
}
