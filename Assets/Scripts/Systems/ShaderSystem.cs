using UnityEngine;
using UnityEngine.Rendering.Universal;

public class ShaderSystem : MonoBehaviour
{
    [Tooltip("Forward Renderer Data asset that contains your Full Screen Pass Renderer Feature (e.g. Ultra_PipelineAsset_ForwardRenderer).")]
    [SerializeField] UniversalRendererData forwardRendererData;

    private FullScreenPassRendererFeature fullScreenPassFeature;

    [SerializeField] Material[] screenSpaceMaterials = new Material[7];
    /*
            0 = Tutorial level
            1 = neutral
            2 = fire
            3 = wind
            4 = water
            5 = earth
            6 = FinalBoss level
    */

    int lastSyncedAreaType = int.MinValue;

    void Awake()
    {
        if (fullScreenPassFeature == null && forwardRendererData != null)
        {
            foreach (ScriptableRendererFeature feature in forwardRendererData.rendererFeatures)
            {
                if (feature is FullScreenPassRendererFeature fs)
                {
                    fullScreenPassFeature = fs;
                    break;
                }
            }
        }

        if (fullScreenPassFeature == null)
            Debug.LogWarning($"{nameof(ShaderSystem)}: No {nameof(FullScreenPassRendererFeature)} found. Assign {nameof(forwardRendererData)} or {nameof(fullScreenPassFeature)}.", this);
    }

    void Update()
    {
        SyncPassMaterialToAreaType();
    }

    void SyncPassMaterialToAreaType()
    {
       

        int areaType = LevelSystem.Instance.CurrentAreaType;
      

        Material selected = screenSpaceMaterials[areaType];
       

        lastSyncedAreaType = areaType;
        fullScreenPassFeature.passMaterial = selected;
    }
}
