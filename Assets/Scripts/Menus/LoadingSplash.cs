using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

public class LoadingSplash : MonoBehaviour
{
    public Slider progressBar;
    public TextMeshProUGUI progressText;

    void Start()
    {
        StartCoroutine(LoadSceneAsync("Overworld"));
    }

    private IEnumerator LoadSceneAsync(string sceneName)
    {
        // start loading Overworld scene in the background
        AsyncOperation operation = SceneManager.LoadSceneAsync(sceneName);

        // prevent it from activating immediately when done
        operation.allowSceneActivation = false;

        while (!operation.isDone)
        {
            // progress goes from 0 to 0.9 before allowSceneActivation activates
            float progress = Mathf.Clamp01(operation.progress / 0.9f);

            if (progressBar != null)
                progressBar.value = progress;

            if (progressText != null)
                progressText.text = $"{(progress * 100f):0}%";

            // once fully loaded, activate the scene
            if (operation.progress >= 0.9f)
            {
                // have a small delay so player sees 100%
                yield return new WaitForSeconds(0.5f);
                operation.allowSceneActivation = true;
            }

            yield return null;
        }
    }
}
