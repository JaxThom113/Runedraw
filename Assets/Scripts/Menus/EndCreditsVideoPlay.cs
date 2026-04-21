using UnityEngine;
using UnityEngine.Video;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

[RequireComponent(typeof(VideoPlayer))]
public class EndCreditsVideoPlay : MonoBehaviour
{
    [Header("Character Credit Videos")]
    public VideoClip drawthurClip;
    public VideoClip decklanClip;
    public VideoClip shufflynnClip;

    [Header("Render Targets")]
    public RawImage displayImage;
    public RenderTexture renderTexture;

    private VideoPlayer videoPlayer;

    void OnEnable()
    {
        if (AudioSystem.Instance != null)
            AudioSystem.Instance.SetSuppressed(true);
    }

    void OnDisable()
    {
        if (AudioSystem.Instance != null)
            AudioSystem.Instance.SetSuppressed(false);
    }

    void Start()
    {
        videoPlayer = GetComponent<VideoPlayer>();

        videoPlayer.renderMode = VideoRenderMode.RenderTexture;
        videoPlayer.targetTexture = renderTexture;

        if (displayImage != null)
        {
            displayImage.texture = renderTexture;
        }

        switch (GameData.SelectedPlayer)
        {
            case 0:
                videoPlayer.clip = drawthurClip;
                break;
            case 1:
                videoPlayer.clip = decklanClip;
                break;
            case 2:
                videoPlayer.clip = shufflynnClip;
                break;
        }

        videoPlayer.loopPointReached += OnVideoFinished;
        videoPlayer.Play();
    }

    private void OnVideoFinished(VideoPlayer vp)
    {
        SceneManager.LoadScene("MainMenu");
    }
}
