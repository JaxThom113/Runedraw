using UnityEngine;


public class TorchLightLinker : MonoBehaviour
{
    [Header("Settings")]
    public MeshRenderer meshRenderer;
    public Light torchLight;
    
    [Tooltip("If the light is too dim, multiply the color intensity here.")]
    public float intensityMultiplier = 1.0f;

    // The standard internal name for the Emission property in Shader Graph
    private const string EmissionPropertyName = "_EmissionColor";

    void Update()
    {
        if (meshRenderer == null || torchLight == null) return;

        // 1. Grab the HDR color from the material
        // Note: Using .sharedMaterial in EditMode prevents creating thousands of material instances
        Color emissionColor = meshRenderer.material.GetColor(EmissionPropertyName);

        // 2. Apply it to the Light component
        // We use the RGB values. If the emission is 'Super-White' (HDR), 
        // it will naturally make the light brighter.
        torchLight.color = emissionColor;
        torchLight.intensity = intensityMultiplier;
    }
}