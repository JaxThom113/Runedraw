using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class OverworldEnemy : MonoBehaviour
{
    private static readonly int SpritePropertyId = Shader.PropertyToID("_Sprite");
    private static readonly int BaseColorPropertyId = Shader.PropertyToID("_BaseColor");
    private static readonly int ColorPropertyId = Shader.PropertyToID("_Color");
    [SerializeField] private Renderer targetRenderer;
    [SerializeField] private Transform visualRoot;
    [SerializeField] private ParticleSystem bleedParticles;
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
    private bool bleedActive;
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

    public void SetBleedActive(bool active)
    {
        if (bleedActive == active)
            return;

        bleedActive = active;
        if (bleedParticles == null)
            return;

        if (bleedActive)
            bleedParticles.Play();
        else
            bleedParticles.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
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
        bleedActive = false;
        vulnerableActive = false;
        currentStunAlpha = 1f;
        currentPoisonAmount = 0f;
        stunActive = false;

        poisonTween?.Kill();
        stunTween?.Kill();
        poisonTween = null;
        stunTween = null;

        if (bleedParticles != null)
            bleedParticles.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);

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
        SetBleedActive(bleedActive);
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
        poisonTween?.Kill();
        stunTween?.Kill();
        if (runtimeMaterial != null)
            Destroy(runtimeMaterial);
    }
}
