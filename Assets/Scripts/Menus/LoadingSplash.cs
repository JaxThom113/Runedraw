using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

public class LoadingSplash : MonoBehaviour
{
    // assign this variable before switching scenes to Splash
    public static string targetScene;

    [Header("UI Elements")]
    public Slider progressBar;
    public TextMeshProUGUI progressText;   
  

    [Header("Optimization")] 
    public ShaderVariantCollection shaderVariants;

    void Start()
    {
        StartCoroutine(LoadSceneAsync());
    }

    IEnumerator LoadSceneAsync()
    {
        // start loading Overworld scene in the background
        AsyncOperation operation = SceneManager.LoadSceneAsync(targetScene);

        // prevent it from activating immediately when done
        operation.allowSceneActivation = false;

        while (!operation.isDone)
        {
            // progress goes from 0 to 0.9 before allowSceneActivation activates
            float progress = Mathf.Clamp01(operation.progress / 0.9f);

            if (progressBar != null)
                progressBar.value = progress;

            if (progressText != null)
                progressText.text = $"Loading Scene... {(progress * 100f):0}%";

            // once fully loaded, activate the scene
            if (operation.progress >= 0.9f)
            { 
                if (progressText != null)
                    progressText.text = $"Optimizing Shaders... {(progress * 100f):0}%"; 
                if (shaderVariants != null)
                    shaderVariants.WarmUp();
                // have a small delay so player sees 100%
                yield return new WaitForSeconds(0.8f); 
                Camera.main.gameObject.SetActive(false); 
                operation.allowSceneActivation = true;
            }

            yield return null;
        }
    }
}
