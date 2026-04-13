using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.VFX;
using DG.Tweening;

public class OverworldEnemy : MonoBehaviour
{
    private static readonly int SpritePropertyId = Shader.PropertyToID("_Sprite");
    private static readonly int BaseColorPropertyId = Shader.PropertyToID("_BaseColor");
    private static readonly int ColorPropertyId = Shader.PropertyToID("_Color");
    private static readonly int IsDeadPropertyId = Shader.PropertyToID("_isDead");
    [SerializeField] private Renderer targetRenderer;
    [SerializeField] private Transform visualRoot;
    public GameObject bleedParticles;
    [SerializeField] private float bleedHitFlashDuration = 0.35f;
    private Coroutine bleedFlashCoroutine;
    public GameObject poisonParticles;
    [SerializeField] private float fadeInDuration = 3f;
    [SerializeField] private float fadeOutDuration = 3f;
    [Header("Status Visuals")]
    public string poisonMaterialProperty = "_PoisonAmount";
    [SerializeField] private float poisonTweenDuration = 0.35f;
    [SerializeField, Range(0f, 1f)] private float maxPoisonAmount = 0.3f;
    [SerializeField, Range(0.05f, 1f)] private float stunMinAlpha = 0.25f;
    [SerializeField] private float stunPulseDuration = 0.35f;
    [SerializeField] private float vulnerableScaleMultiplier = 0.5f;
    public Material material;  
    public EnemySO enemyData;    
    private Material runtimeMaterial;
    private Vector3 baseVisualScale = Vector3.one;
    private Color baseMaterialColor = Color.white;
    private bool stunActive;
    private bool vulnerableActive;
    private float currentStunAlpha = 1f;
    private float currentPoisonAmount;
    private int colorPropertyId = -1;
    private Tween stunTween;
    private Tween poisonTween;

    public void UpdateEnemy(EnemySO enemyData)
    { 
        this.enemyData = enemyData; 
        ApplyCurrentEnemyMaterial();
    } 

    private void Awake()
    {
        if (targetRenderer == null)
            targetRenderer = GetComponentInChildren<Renderer>();
        if (visualRoot == null)
            visualRoot = transform;

        baseVisualScale = visualRoot.localScale;
        CacheMaterialDefaults();

        if (bleedParticles != null)
            bleedParticles.SetActive(false);
    }

    public void SetBleedVisualEnabled(bool enabled)
    {
        if (bleedParticles != null)
            bleedParticles.SetActive(enabled);
    }

    public void ApplyCurrentEnemyMaterial()
    {
        if (enemyData == null || enemyData.enemyMaterial == null)
            return;

        ApplyMaterial(enemyData.enemyMaterial);
    }

    public void ApplyMaterial(Material materialTemplate)
    {
        if (materialTemplate == null || targetRenderer == null)
            return;

        if (runtimeMaterial != null)
            Destroy(runtimeMaterial);

        runtimeMaterial = new Material(materialTemplate);
        if (materialTemplate.HasProperty(SpritePropertyId) && runtimeMaterial.HasProperty(SpritePropertyId))
        {
            runtimeMaterial.SetTexture(SpritePropertyId, materialTemplate.GetTexture(SpritePropertyId));
        }
        material = runtimeMaterial;
        targetRenderer.sharedMaterial = runtimeMaterial;
        CacheMaterialDefaults();
        ApplyStatusVisualState();
    }

    /// <summary>
    /// Sets Shader Graph boolean <c>_isDead</c> on this enemy's <b>instance</b> material only
    /// (<see cref="runtimeMaterial"/> from <see cref="ApplyMaterial"/>), not the shared asset.
    /// </summary>
    public void SetIsDeadOnInstanceMaterial(bool isDead)
    {
        Material instance = runtimeMaterial != null ? runtimeMaterial : material;
        if (instance == null || !instance.HasProperty(IsDeadPropertyId))
            return;
        // Shader Graph Bool is a float 0/1 in the material property block.
        instance.SetFloat(IsDeadPropertyId, isDead ? 1f : 0f);
    }

    public void FadeIn() 
    {
        if (material == null)
            return;

        material.SetFloat("_FadeIn", 0.4f); 

        DOTween.To( 
            () => material.GetFloat("_FadeIn"), 
            (float x) => material.SetFloat("_FadeIn", x), 
            -0.2f, 
            fadeInDuration
        ).SetEase(Ease.InOutSine); 
        
    }

    public Tween FadeOut()
    {
        if (material == null)
            return null;
        material.SetFloat("_FadeIn", -0.2f); 
        return DOTween.To(
            () => material.GetFloat("_FadeIn"),
            (float x) => material.SetFloat("_FadeIn", x),
            0.4f,
            fadeOutDuration
        ).SetEase(Ease.InSine);
    }

    public void SetPoisonTurnsRemaining(int turnsRemaining, int maxTurns)
    {
        float targetPoisonAmount = 0f;
        if (turnsRemaining > 0 && maxTurns > 0)
        {
            float normalizedProgress = (float)(maxTurns - turnsRemaining + 1) / maxTurns;
            targetPoisonAmount = Mathf.Clamp01(normalizedProgress) * maxPoisonAmount;
        }

        if (!HasPoisonProperty())
        {
            currentPoisonAmount = targetPoisonAmount;
            return;
        }

        poisonTween?.Kill();
        currentPoisonAmount = material.GetFloat(poisonMaterialProperty);
        poisonTween = DOTween.To(
            () => material.GetFloat(poisonMaterialProperty),
            x =>
            {
                currentPoisonAmount = x;
                material.SetFloat(poisonMaterialProperty, currentPoisonAmount);
            },
            targetPoisonAmount,
            poisonTweenDuration
        ).SetEase(Ease.InOutSine);
    }

    public void PlayBleedHitFlash()
    {
        if (bleedParticles == null)
            return;

        if (bleedFlashCoroutine != null)
            StopCoroutine(bleedFlashCoroutine);

        bleedFlashCoroutine = StartCoroutine(BleedHitFlashRoutine());
    }

    private IEnumerator BleedHitFlashRoutine()
    {
        VisualEffect bloodVfx = bleedParticles.GetComponent<VisualEffect>();
        if (bloodVfx != null)
            bloodVfx.SendEvent("OnPlay");

        yield return new WaitForSeconds(bleedHitFlashDuration);
        bleedFlashCoroutine = null;
    }

    public void SetStunActive(bool active)
    {
        if (stunActive == active)
            return;

        stunActive = active;
        stunTween?.Kill();
        stunTween = null;
        currentStunAlpha = 1f;

        if (!stunActive)
        {
            ApplyStatusMaterialColor();
            return;
        }

        if (!HasUsableColorProperty())
            return;

        stunTween = DOTween.To(
            () => currentStunAlpha,
            x =>
            {
                currentStunAlpha = x;
                ApplyStatusMaterialColor();
            },
            stunMinAlpha,
            stunPulseDuration
        ).SetEase(Ease.InOutSine)
         .SetLoops(-1, LoopType.Yoyo);
    }

    public void SetVulnerableActive(bool active)
    {
        if (vulnerableActive == active)
            return;

        vulnerableActive = active;
        if (visualRoot == null)
            return;

        visualRoot.localScale = vulnerableActive
            ? baseVisualScale * vulnerableScaleMultiplier
            : baseVisualScale;
    }

    public void ClearStatusVisuals()
    {
        vulnerableActive = false;
        currentStunAlpha = 1f;
        currentPoisonAmount = 0f;
        stunActive = false;

        poisonTween?.Kill();
        stunTween?.Kill();
        poisonTween = null;
        stunTween = null;

        if (visualRoot != null)
            visualRoot.localScale = baseVisualScale;

        if (HasPoisonProperty())
            material.SetFloat(poisonMaterialProperty, 0f);

        ApplyStatusMaterialColor();
    }

    public void DisableSphereCollider()
    {
        SphereCollider sphereCollider = GetComponent<SphereCollider>();
        if (sphereCollider != null)
            sphereCollider.enabled = false;
    }

    private void CacheMaterialDefaults()
    {
        if (material == null)
            return;

        if (material.HasProperty(BaseColorPropertyId))
            colorPropertyId = BaseColorPropertyId;
        else if (material.HasProperty(ColorPropertyId))
            colorPropertyId = ColorPropertyId;
        else
            colorPropertyId = -1;

        if (colorPropertyId != -1)
            baseMaterialColor = material.GetColor(colorPropertyId);
    }

    private bool HasUsableColorProperty()
    {
        return material != null && colorPropertyId != -1;
    }

    private bool HasPoisonProperty()
    {
        return material != null
            && !string.IsNullOrWhiteSpace(poisonMaterialProperty)
            && material.HasProperty(poisonMaterialProperty);
    }

    private void ApplyStatusVisualState()
    {
        if (HasPoisonProperty())
            material.SetFloat(poisonMaterialProperty, currentPoisonAmount);

        ApplyStatusMaterialColor();
        SetVulnerableActive(vulnerableActive);
        if (stunActive)
        {
            bool shouldResumeStun = stunActive;
            stunActive = false;
            SetStunActive(shouldResumeStun);
        }
    }

    private void ApplyStatusMaterialColor()
    {
        if (!HasUsableColorProperty())
            return;

        Color targetColor = baseMaterialColor;
        targetColor.a *= currentStunAlpha;
        material.SetColor(colorPropertyId, targetColor);
    }

    private void OnDestroy()
    {
        if (bleedFlashCoroutine != null)
        {
            StopCoroutine(bleedFlashCoroutine);
            bleedFlashCoroutine = null;
        }

        poisonTween?.Kill();
        stunTween?.Kill();
        if (runtimeMaterial != null)
            Destroy(runtimeMaterial);
    }
}
