using UnityEngine;

/* * This script bypasses the VisualEffect component to directly enforce 
 * the render queue on the specific mask material asset.
 */
public class VFXMaskQueueEnforcer : MonoBehaviour
{
    [Header("Mask Material Assignment")]
    [Tooltip("Drag the exact Material used in your 'Output Particle Mesh' block here. Do not put the URP rock material here.")]
    public Material maskMaterial;
    
    [Header("Render Settings")]
    public int targetQueue = 2000;

    void Awake()
    {
        if (maskMaterial != null)
        {
            // Forces the queue directly on the asset in memory during Play Mode
            if (maskMaterial.renderQueue != targetQueue)
            {
                maskMaterial.renderQueue = targetQueue;
                Debug.Log($"[VFX Tech Art] {maskMaterial.name} Render Queue forced to: {targetQueue}");
            }
        }
        else
        {
            Debug.LogWarning("[VFX Tech Art] Mask Material is not assigned to the Queue Enforcer script!");
        }
    }
}