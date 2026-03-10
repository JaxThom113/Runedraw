using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SoundEffectSystem : Singleton<SoundEffectSystem>
{
    [SerializeField] private AudioSource effectAudioSource;  
    [SerializeField] private AudioSource themeAudioSource; 

    [SerializeField] private AudioClip cardDrawSound; 
    [SerializeField] private AudioClip cardDiscardSound;  
    [SerializeField] private AudioClip ButtonClickSound; 
    [SerializeField] private AudioClip WalkSound; 
    [SerializeField] private AudioClip overworldTheme;   
    [SerializeField] private AudioClip battleTheme;   
    [SerializeField] private AudioClip victoryTheme;   
    [SerializeField] private AudioClip defeatTheme;   

    private bool actionHooksBound = false;

    private void OnEnable()
    {
        if (actionHooksBound) return;
        actionHooksBound = true;
        ActionSystem.AttachPerformer<SoundEffectGA>(SoundEffectPerformer);
    }

    private void OnDisable()
    {
        if (!actionHooksBound) return;
        actionHooksBound = false;
        ActionSystem.DetachPerformer<SoundEffectGA>();
    }

    private IEnumerator SoundEffectPerformer(SoundEffectGA soundEffectGA)
    {
        if (soundEffectGA.sound != null && effectAudioSource != null)
        {
            effectAudioSource.PlayOneShot(soundEffectGA.sound);
        }
        yield return null;
    } 
    public void PlayCardDrawSound()
    {
        if (cardDrawSound != null && effectAudioSource != null)
        {
            effectAudioSource.PlayOneShot(cardDrawSound);
        }
    }
    public void PlayCardDiscardSound()
    {
        if (cardDiscardSound != null && effectAudioSource != null)
        {
            effectAudioSource.PlayOneShot(cardDiscardSound);
        }
    }
    public void PlayButtonClickSound()
    {
        if (ButtonClickSound != null && effectAudioSource != null)
        {
            effectAudioSource.PlayOneShot(ButtonClickSound);
        }
    }
    public void PlayWalkSound()
    {
        if (WalkSound != null && effectAudioSource != null)
        {
            effectAudioSource.PlayOneShot(WalkSound);
        }
    }
    public void PlayOverworldTheme()
    {
        if (overworldTheme != null && themeAudioSource != null)
        {
            themeAudioSource.clip = overworldTheme;
            themeAudioSource.loop = true;
            themeAudioSource.Play();
        }
    }
    public void PlayBattleTheme()
    {
        if (battleTheme != null && themeAudioSource != null)
        {
            themeAudioSource.clip = battleTheme;
            themeAudioSource.loop = true;
            themeAudioSource.Play();
        }
    }
    public void PlayVictoryTheme()
    {
        if (victoryTheme != null && themeAudioSource != null)
        {
            themeAudioSource.clip = victoryTheme;
            themeAudioSource.loop = true;
            themeAudioSource.Play();
        }
    }
    public void PlayDefeatTheme()
    {
        if (defeatTheme != null && themeAudioSource != null)
        {
            themeAudioSource.clip = defeatTheme;
            themeAudioSource.loop = true;
            themeAudioSource.Play();
        }
    }
}
