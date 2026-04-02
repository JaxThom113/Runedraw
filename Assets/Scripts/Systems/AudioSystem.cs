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

    // keep track of the time played on each music track, so Pause/Resume work
    private Dictionary<string, float> musicPlaybackTimes = new Dictionary<string, float>();
    private string currentTrack = null;

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

        foreach (var item in music)
            musicPlaybackTimes.Add(item.Key, 0f);

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

    /*
        Music control
    */

    public void PlayMusic(string clipName, bool resume = false)
    {
        if (!music.ContainsKey(clipName) || musicSource == null)
            return;

        // if the song is already playing, don't restart it
        if (currentTrack == clipName) 
            return;
        
        // save current track time before switching
        if (currentTrack != null && musicSource.isPlaying)
            musicPlaybackTimes[currentTrack] = musicSource.time;

        AudioClip clip = music[clipName];
        musicSource.clip = clip;
        musicSource.loop = true;
        if (resume) musicSource.time = musicPlaybackTimes[clipName];
        musicSource.Play();

        currentTrack = clipName;
    }

    public void StopMusic()
    {
        musicSource.Stop();
    }

    public void SetMusicVolume(float volume)
    {
        musicSource.volume = volume;
    }

    /*
        SFX control
    */
    
    public void PlaySFX(string clipName)
    {
        if (!sfx.ContainsKey(clipName) || sfxSource == null)
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