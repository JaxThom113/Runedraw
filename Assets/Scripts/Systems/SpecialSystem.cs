using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class SpecialSystem : Singleton<SpecialSystem>
{
    // Takes reference to the DomainExpansion and SpecialSprite references in the current OverworldEnemy and tweens the speed and fade/scale
    // Performer tweens in the SpecialSprite with exact same logic as in OverworldEnemy.FadeIn().
    // DomainExpansion tweens from scale 0 to scale 1 (instead of fading).
    // Tweens the speed of the DomainExpansion and SpecialSprite.
    // On KillEnemyGA the tweens are reversed.

    [Header("SpecialSprite Fade (copied from OverworldEnemy.FadeIn / FadeOut)")]
    public string fadeInPropertyName = "_FadeIn";
    public float fadeInStartValue = 0.4f;
    public float fadeInEndValue = -0.2f;
    public float specialSpriteFadeInDuration = 3f;
    public float specialSpriteFadeOutDuration = 3f;

    [Header("DomainExpansion Scale")]
    public float domainExpansionScaleInDuration = 3f;
    public float domainExpansionScaleOutDuration = 3f;
    public float environmentDeactivateDelay = 0.5f;

    [Header("Speed")]
    public string speedPropertyName = "_Speed";
    public float specialSpriteTargetSpeed = 1f;
    public float domainExpansionTargetSpeed = 1f;
    public float specialSpriteSpeedTweenDuration = 3f;
    public float domainExpansionSpeedTweenDuration = 3f;

    private Material specialSpriteRuntimeMaterial;
    private Material domainExpansionRuntimeMaterial;
    private Tween domainExpansionScaleTween;

    // Remember who cast the last special so the reverse on KillEnemyGA skips the sprite work
    // when the player cast it (the enemy's own sprite, if any, must stay untouched).
    private bool lastSpecialCastByPlayer;

    private GameObject cachedGridContainer;
    private GameObject cachedSkybox;
    private GameObject cachedGround;
    private GameObject cachedWallsContainer;
    private GameObject cachedTorchContainer;

    void OnEnable()
    {
        ActionSystem.AttachPerformer<SpecialGA>(SpecialEffectPerformer);
        ActionSystem.SubscribeReaction<KillEnemyGA>(ReverseSpecialOnKillPreReaction, ReactionTiming.PRE);
    }

    void OnDisable()
    {
        ActionSystem.DetachPerformer<SpecialGA>();
        ActionSystem.UnsubscribeReaction<KillEnemyGA>(ReverseSpecialOnKillPreReaction, ReactionTiming.PRE);
    }

    IEnumerator SpecialEffectPerformer(SpecialGA specialGA)
    {
        OverworldEnemy overworldEnemy = EnemySystem.Instance != null ? EnemySystem.Instance.overworldEnemy : null;
        if (overworldEnemy == null)
            yield break;

        lastSpecialCastByPlayer = specialGA.isPlayer;

        GameObject domainExpansion = overworldEnemy.DomainExpansion;
        if (domainExpansion != null)
        {
            domainExpansion.transform.localScale = Vector3.zero;
            domainExpansion.SetActive(true);
            domainExpansionRuntimeMaterial = ApplyMaterialInstance(domainExpansion, specialGA.domainExpansionMaterial, domainExpansionRuntimeMaterial);
            ScaleDomainExpansion(domainExpansion, Vector3.one, domainExpansionScaleInDuration, deactivateOnComplete: false);
            TweenSpeed(domainExpansionRuntimeMaterial, domainExpansionTargetSpeed, domainExpansionSpeedTweenDuration);
        }

        // Enemy specials always tween the SpecialSprite in. Player specials deliberately leave
        // the enemy's SpecialSprite alone (its existing material/state must stay intact).
        if (!specialGA.isPlayer)
        {
            GameObject specialSprite = overworldEnemy.SpecialSprite;
            if (specialSprite != null)
            {
                specialSprite.SetActive(true);
                specialSpriteRuntimeMaterial = ApplyMaterialInstance(specialSprite, specialGA.specialSpriteMaterial, specialSpriteRuntimeMaterial);
                FadeInMaterial(specialSpriteRuntimeMaterial, specialSpriteFadeInDuration);
                TweenSpeed(specialSpriteRuntimeMaterial, specialSpriteTargetSpeed, specialSpriteSpeedTweenDuration);
            }

            // Boss specials additionally fade the regular enemy sprite out and fully restore health.
            if (specialGA.isBoss)
            {
                overworldEnemy.FadeOut();
                RestoreEnemyHealthToMax();
            }
        }

        CreateLevel activeCreateLevel = FindFirstObjectByType<CreateLevel>(FindObjectsInactive.Exclude);
        cachedGridContainer = activeCreateLevel != null ? activeCreateLevel.gridContainer : null;
        cachedSkybox = activeCreateLevel != null ? activeCreateLevel.skybox : null;
        cachedGround = activeCreateLevel != null ? activeCreateLevel.ground : null;
        cachedWallsContainer = activeCreateLevel != null ? activeCreateLevel.wallsContainer : null;
        cachedTorchContainer = activeCreateLevel != null ? activeCreateLevel.torchContainer : null;

        yield return new WaitForSeconds(environmentDeactivateDelay);

        if (cachedGridContainer != null) cachedGridContainer.SetActive(false);
        if (cachedSkybox != null) cachedSkybox.SetActive(false);
        if (cachedGround != null) cachedGround.SetActive(false);
        if (cachedWallsContainer != null) cachedWallsContainer.SetActive(false);
        if (cachedTorchContainer != null) cachedTorchContainer.SetActive(false);

        // Block subsequent effects (damage, status, etc.) until the special's tweens have settled.
        float totalSpecialDuration = Mathf.Max(domainExpansionScaleInDuration, specialSpriteFadeInDuration);
        float remainingWait = totalSpecialDuration - environmentDeactivateDelay;
        if (remainingWait > 0f)
            yield return new WaitForSeconds(remainingWait);
    }

    public void FadeOutSpecial()
    {
        if (cachedGridContainer != null) cachedGridContainer.SetActive(true);
        if (cachedSkybox != null) cachedSkybox.SetActive(true);
        if (cachedGround != null) cachedGround.SetActive(true);
        if (cachedWallsContainer != null) cachedWallsContainer.SetActive(true);
        if (cachedTorchContainer != null) cachedTorchContainer.SetActive(true);

        OverworldEnemy overworldEnemy = EnemySystem.Instance != null ? EnemySystem.Instance.overworldEnemy : null;
        if (overworldEnemy == null)
            return;

        // Mirror the performer: skip sprite teardown when the player was the caster so the
        // enemy's own sprite state is not disturbed.
        if (!lastSpecialCastByPlayer && specialSpriteRuntimeMaterial != null)
        {
            FadeOutMaterial(specialSpriteRuntimeMaterial, specialSpriteFadeOutDuration, overworldEnemy.SpecialSprite);
            TweenSpeed(specialSpriteRuntimeMaterial, 0f, specialSpriteSpeedTweenDuration);
        }

        if (overworldEnemy.DomainExpansion != null)
        {
            ScaleDomainExpansion(overworldEnemy.DomainExpansion, Vector3.zero, domainExpansionScaleOutDuration, deactivateOnComplete: true);
            TweenSpeed(domainExpansionRuntimeMaterial, 0f, domainExpansionSpeedTweenDuration);
        }
    }

    private void ReverseSpecialOnKillPreReaction(KillEnemyGA killEnemyGA)
    {
        FadeOutSpecial();
    }

    private Material ApplyMaterialInstance(GameObject target, Material template, Material existingRuntime)
    {
        if (template == null || target == null)
            return existingRuntime;

        Renderer renderer = target.GetComponentInChildren<Renderer>();
        if (renderer == null)
            return existingRuntime;

        if (existingRuntime != null)
            Destroy(existingRuntime);

        Material instance = new Material(template);
        renderer.sharedMaterial = instance;
        return instance;
    }

    // Copy-paste of OverworldEnemy.FadeIn, scoped to the provided material with a tweakable duration.
    private void FadeInMaterial(Material material, float duration)
    {
        if (material == null || !material.HasProperty(fadeInPropertyName))
            return;

        material.SetFloat(fadeInPropertyName, fadeInStartValue);

        DOTween.To(
            () => material.GetFloat(fadeInPropertyName),
            (float x) => material.SetFloat(fadeInPropertyName, x),
            fadeInEndValue,
            duration
        ).SetEase(Ease.InOutSine);
    }

    // Copy-paste of OverworldEnemy.FadeOut, scoped to the provided material with a tweakable duration.
    private Tween FadeOutMaterial(Material material, float duration, GameObject target)
    {
        if (material == null || !material.HasProperty(fadeInPropertyName))
            return null;

        material.SetFloat(fadeInPropertyName, fadeInEndValue);
        Tween t = DOTween.To(
            () => material.GetFloat(fadeInPropertyName),
            (float x) => material.SetFloat(fadeInPropertyName, x),
            fadeInStartValue,
            duration
        ).SetEase(Ease.InSine);

        if (target != null)
            t.OnComplete(() => target.SetActive(false));
        return t;
    }

    private void ScaleDomainExpansion(GameObject target, Vector3 targetScale, float duration, bool deactivateOnComplete)
    {
        if (target == null)
            return;

        domainExpansionScaleTween?.Kill();
        domainExpansionScaleTween = target.transform.DOScale(targetScale, duration).SetEase(Ease.InOutSine);
        if (deactivateOnComplete)
            domainExpansionScaleTween.OnComplete(() => target.SetActive(false));
    }

    private void RestoreEnemyHealthToMax()
    {
        EnemyView enemyView = DamageSystem.Instance != null ? DamageSystem.Instance.enemyView : null;
        if (enemyView == null || enemyView.maxHealth <= 0)
            return;

        enemyView.currentHealth = enemyView.maxHealth;
        enemyView.UpdateHealthDisplay();
    }

    private void TweenSpeed(Material material, float targetSpeed, float duration)
    {
        if (material == null || string.IsNullOrWhiteSpace(speedPropertyName) || !material.HasProperty(speedPropertyName))
            return;

        DOTween.To(
            () => material.GetFloat(speedPropertyName),
            (float x) => material.SetFloat(speedPropertyName, x),
            targetSpeed,
            duration
        ).SetEase(Ease.InOutSine);
    }

    private void OnDestroy()
    {
        domainExpansionScaleTween?.Kill();
        if (specialSpriteRuntimeMaterial != null)
            Destroy(specialSpriteRuntimeMaterial);
        if (domainExpansionRuntimeMaterial != null)
            Destroy(domainExpansionRuntimeMaterial);
    }
}
