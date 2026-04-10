using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RTManager : MonoBehaviour
{
    [Header("Shader")]
    [SerializeField] private Shader sunShader;

    [Header("Render Textures")]
    [SerializeField] private RenderTexture sunRT_B;

    [Header("Blend")]
    [Range(0f, 1f)]
    [SerializeField] private float blend = 0f;

    private readonly List<Renderer> sunRenderers = new();
    private MaterialPropertyBlock block;
    private Coroutine transitionCoroutine;

    void Awake()
    {
        block = new MaterialPropertyBlock();

        Renderer[] allRenderers = FindObjectsOfType<Renderer>();
        foreach (Renderer r in allRenderers)
        {
            if (r.sharedMaterial != null && r.sharedMaterial.shader == sunShader)
            {
                sunRenderers.Add(r);
            }
        }
    }

    void LateUpdate()
    {
        foreach (Renderer r in sunRenderers)
        {
            r.GetPropertyBlock(block);
            // _SunRT_A comes from the material itself, we only override _SunRT_B and _Blend
            block.SetTexture("_TransitionTexture", sunRT_B);
            block.SetFloat("_Blend", blend);
            r.SetPropertyBlock(block);
        }
    }

    // Public API
    public void SetBlend(float value)
    {
        blend = Mathf.Clamp01(value);
    }

    public void SwapSecondRT(RenderTexture newRT)
    {
        sunRT_B = newRT;
    } 
    public void StartTransition(RenderTexture newRT)
    {
        SwapSecondRT(newRT);
        
        // Stop any existing transition
      
        
        // Start new transition
        transitionCoroutine = StartCoroutine(TransitionBlend(3f));
    }
    
    private IEnumerator TransitionBlend(float duration)
    {
        float startBlend = 0f;
        float endBlend = 1f;
        float elapsed = 0f;
        
        blend = startBlend;
        
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            blend = Mathf.Lerp(startBlend, endBlend, elapsed / duration);
            yield return null;
        }
        
        blend = endBlend;
        transitionCoroutine = null;
    }
}
