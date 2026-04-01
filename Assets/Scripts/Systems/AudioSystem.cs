using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioSystem : PersistentSingleton<AudioSystem>
{
    [Header("Audio Sources")]
    [SerializeField] private AudioSource musicSource;
    [SerializeField] private AudioSource sfxSource;

    [Header("Music")]
    [SerializeField] private AudioClip menuTheme;
    [SerializeField] private AudioClip overworldTheme;   
    [SerializeField] private AudioClip battleTheme;   
    [SerializeField] private AudioClip victoryTheme;   
    [SerializeField] private AudioClip defeatTheme;  

    [Header("Sound Effects")]
    [SerializeField] private AudioClip clickSound;
    [SerializeField] private AudioClip walkSound; 
    [SerializeField] private AudioClip cardDrawSound; 
    [SerializeField] private AudioClip cardDiscardSound;  

    // dictionaries for music/sfx so certain clips can be accessed outside AudioSystem
    private Dictionary<string, AudioClip> music;
    private Dictionary<string, AudioClip> sfx;

    private bool actionHooksBound = false;

    private void OnEnable()
    {
        music = new Dictionary<string, AudioClip>();
        sfx = new Dictionary<string, AudioClip>();

        // add all the music/sfx from serializefields into dictionaries
        music.Add("menu", menuTheme);
        music.Add("overworld", overworldTheme);
        music.Add("battle", battleTheme);
        music.Add("victory", victoryTheme);
        music.Add("defeat", defeatTheme);

        sfx.Add("click", clickSound);
        sfx.Add("walk", walkSound);
        sfx.Add("cardDraw", cardDrawSound);
        sfx.Add("cardDiscard", cardDiscardSound);

        // SoundEffectGA calls
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

    public void PlayMusic(string clipName)
    {
        if (!music.ContainsKey(clipName))
            return;
        
        AudioClip clip = music[clipName];

        // if the song is already playing, don't restart it
        if (musicSource.clip == clip) 
            return;

        musicSource.clip = clip;
        musicSource.loop = true;
        musicSource.Play();
    }

    public void StopMusic()
    {
        musicSource.Stop();
    }

    public void SetMusicVolume(float volume)
    {
        musicSource.volume = volume;
    }
    
    public void PlaySFX(string clipName)
    {
        if (!sfx.ContainsKey(clipName))
            return;

        AudioClip clip = sfx[clipName];
        sfxSource.PlayOneShot(clip);
    }

    public void SetSFXVolume(float volume)
    {
        sfxSource.volume = volume;
    }

    private IEnumerator SoundEffectPerformer(SoundEffectGA soundEffectGA)
    {
        if (soundEffectGA.sound != null && sfxSource != null)
            sfxSource.PlayOneShot(soundEffectGA.sound);

        yield return null;
    } 
}