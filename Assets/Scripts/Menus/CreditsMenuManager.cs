using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using UnityEngine.SceneManagement;

public class CreditsMenuManager : MonoBehaviour
{
    public void OnBackClicked()
    {
        AudioSystem.Instance.PlaySFX("click");
        SceneManager.LoadScene("MainMenu");
    }
}
