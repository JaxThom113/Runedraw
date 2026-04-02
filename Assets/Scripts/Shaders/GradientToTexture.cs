using System.Collections.Generic;
using UnityEngine; 
using System.IO;
#if UNITY_EDITOR
using UnityEditor;
#endif 

public class GradientBatchBaker : MonoBehaviour
{
    [Header("Gradient Setup")]
    public List<Gradient> gradientsToBake = new List<Gradient>();
    public int textureSize = 256;
    
    [Header("Bake Settings")]
    [Tooltip("If true, bakes when entered play mode")]
    public bool bakeOnStart = false;
    
    [Header("Output Location")]
    public string outputFolder = "Scripts/Shaders/BakedGradients";

    // Checks the boolean before deciding to run the batch process
    void Start()
    {
        if (bakeOnStart)
        {
            BakeAndSaveAll();
        }
        else
        {
            Debug.Log("Autobake disabled");
        }
    }

    [ContextMenu("Force Bake All Gradients Now")]
    public void BakeAndSaveAll()
    {
        if (gradientsToBake.Count == 0)
        {
            Debug.LogWarning("The gradient list is empty");
            return;
        }

        string dirPath = Application.dataPath + "/" + outputFolder + "/";
        if (!Directory.Exists(dirPath))
        {
            Directory.CreateDirectory(dirPath);
        }

        for (int i = 0; i < gradientsToBake.Count; i++)
        {
            Gradient currentGradient = gradientsToBake[i];
            
            if (currentGradient == null) continue;

            Texture2D tempTexture = new Texture2D(textureSize, 1);
            tempTexture.wrapMode = TextureWrapMode.Clamp;
            tempTexture.filterMode = FilterMode.Bilinear;

            Color[] colors = new Color[textureSize];
            for (int x = 0; x < textureSize; x++)
            {
                float timeValue = x / (float)(textureSize - 1);
                colors[x] = currentGradient.Evaluate(timeValue);
            }

            tempTexture.SetPixels(colors);
            tempTexture.Apply();

            string hash = JsonUtility.ToJson(currentGradient).GetHashCode().ToString();
            string fileName = "Gradient_" + i.ToString() + "_" + hash + ".png";
            string fullPath = dirPath + fileName;

            byte[] bytes = tempTexture.EncodeToPNG();
            File.WriteAllBytes(fullPath, bytes);
            
            DestroyImmediate(tempTexture);
        }

        #if UNITY_EDITOR
        AssetDatabase.Refresh();
        Debug.Log("Successfully baked and saved " + gradientsToBake.Count + " gradients to: " + dirPath);
        #endif
    }
}