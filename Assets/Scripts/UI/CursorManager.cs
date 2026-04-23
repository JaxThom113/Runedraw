using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class CursorManager : Singleton<CursorManager>
{
    [Header("Mouse Cursor")]
    [SerializeField] private Texture2D cursorImage;

    void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        SetCursor();
    }

    public void SetCursor()
    {
        // assign the custom mouse cursor in every scene
        Cursor.SetCursor(cursorImage, Vector2.zero, CursorMode.Auto);
    }
}