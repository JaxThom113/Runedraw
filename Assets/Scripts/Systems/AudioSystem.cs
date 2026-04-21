using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class AudioSystem : PersistentSingleton<AudioSystem>
{
    [Serializable]
    private class AreaThemeSet
    {
        public AudioClip overworld;
        [Range(0f, 1f)] public float overworldVolume = 1f;
        public AudioClip battle;
        [Range(0f, 1f)] public float battleVolume = 1f;
    }

    [Header("Audio Sources")]
    [SerializeField] private AudioSource musicSource;
    [SerializeField] private AudioSource sfxSource;

    [Header("Music")]
    [SerializeField] private AudioClip menuTheme;
    [SerializeField, Range(0f, 1f)] private float menuThemeVolume = 1f;
    [SerializeField] private AudioClip overworldTheme;
    [SerializeField, Range(0f, 1f)] private float overworldThemeVolume = 1f;
    [SerializeField] private AudioClip battleTheme;
    [SerializeField, Range(0f, 1f)] private float battleThemeVolume = 1f;
    [SerializeField] private AudioClip victoryTheme;
    [SerializeField, Range(0f, 1f)] private float victoryThemeVolume = 1f;
    [SerializeField] private AudioClip defeatTheme;
    [SerializeField, Range(0f, 1f)] private float defeatThemeVolume = 1f;
    [SerializeField] private AudioClip portalWhirlTheme;
    [SerializeField, Range(0f, 1f)] private float portalWhirlThemeVolume = 1f;

    [Header("Per-Area Themes (Overworld + Battle)")]
    [SerializeField] private AreaThemeSet tutorialThemes;
    [SerializeField] private AreaThemeSet neutralThemes;
    [SerializeField] private AreaThemeSet fireThemes;
    [SerializeField] private AreaThemeSet windThemes;
    [SerializeField] private AreaThemeSet waterThemes;
    [SerializeField] private AreaThemeSet earthThemes;
    [SerializeField] private AreaThemeSet finalBossThemes;

    [Header("Sound Effects")]
    [SerializeField] private AudioClip clickSound;
    [SerializeField, Range(0f, 1f)] private float clickVolume = 1f;
    [SerializeField] private AudioClip walkSound;
    [SerializeField, Range(0f, 1f)] private float walkVolume = 1f;
    [SerializeField] private AudioClip cardDrawSound;
    [SerializeField, Range(0f, 1f)] private float cardDrawVolume = 1f;
    [SerializeField] private AudioClip spellCastSound;
    [SerializeField, Range(0f, 1f)] private float spellCastVolume = 1f;

    private Dictionary<string, AudioClip> music;
    private Dictionary<string, AudioClip> sfx;

    private Dictionary<string, float> musicVolumes;
    private Dictionary<string, float> sfxVolumes;

    private float masterMusicVolume = 1f;
    private float masterSfxVolume = 1f;

    private Dictionary<string, float> musicPlaybackTimes = new Dictionary<string, float>();
    private string currentTrack = null;

    // When true, AudioSystem becomes a no-op (used while the end-credits video
    // is active so its baked-in audio isn't competing with music/SFX).
    public bool Suppressed { get; private set; }

    public void SetSuppressed(bool suppressed)
    {
        Suppressed = suppressed;
        if (suppressed && musicSource != null && musicSource.isPlaying)
            musicSource.Stop();
    }

    private void OnEnable()
    {
        music = new Dictionary<string, AudioClip>();
        sfx = new Dictionary<string, AudioClip>();
        musicVolumes = new Dictionary<string, float>();
        sfxVolumes = new Dictionary<string, float>();

        AddMusic("menu", menuTheme, menuThemeVolume);
        AddMusic("overworld", overworldTheme, overworldThemeVolume);
        AddMusic("battle", battleTheme, battleThemeVolume);
        AddMusic("victory", victoryTheme, victoryThemeVolume);
        AddMusic("defeat", defeatTheme, defeatThemeVolume);
        AddMusic("portalWhirl", portalWhirlTheme, portalWhirlThemeVolume);

        musicPlaybackTimes.Clear();
        foreach (var item in music)
            musicPlaybackTimes.Add(item.Key, 0f);

        AddSfx("click", clickSound, clickVolume);
        AddSfx("walk", walkSound, walkVolume);
        AddSfx("cardDraw", cardDrawSound, cardDrawVolume);
        AddSfx("spellCast", spellCastSound, spellCastVolume);

        masterMusicVolume = musicSource != null ? Mathf.Clamp01(musicSource.volume) : 1f;
        masterSfxVolume = sfxSource != null ? Mathf.Clamp01(sfxSource.volume) : 1f;

        BindActionHooks();
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
        UnbindActionHooks();
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        BindActionHooks();
    }

    private void BindActionHooks()
    {
        ActionSystem.AttachPerformer<SoundEffectGA>(SoundEffectPerformer);
        ActionSystem.SubscribeReaction<NextAreaGA>(NextAreaPostReaction, ReactionTiming.POST);
        ActionSystem.SubscribeReaction<SpellCastGA>(SpellCastPreReaction, ReactionTiming.PRE);
    }

    private void UnbindActionHooks()
    {
        ActionSystem.DetachPerformer<SoundEffectGA>();
        ActionSystem.UnsubscribeReaction<NextAreaGA>(NextAreaPostReaction, ReactionTiming.POST);
        ActionSystem.UnsubscribeReaction<SpellCastGA>(SpellCastPreReaction, ReactionTiming.PRE);
    }

    private static float SanitizeVolume(float volume, AudioClip clip)
    {
        if (clip != null && volume <= 0f) return 1f;
        return Mathf.Clamp01(volume);
    }

    private void AddMusic(string key, AudioClip clip, float volume)
    {
        music.Add(key, clip);
        musicVolumes[key] = SanitizeVolume(volume, clip);
    }

    private void AddSfx(string key, AudioClip clip, float volume)
    {
        sfx.Add(key, clip);
        sfxVolumes[key] = SanitizeVolume(volume, clip);
    }

    private float GetMusicVolume(string key)
    {
        if (musicVolumes != null && musicVolumes.TryGetValue(key, out float v)) return v;
        return 1f;
    }

    private float GetSfxVolume(string key)
    {
        if (sfxVolumes != null && sfxVolumes.TryGetValue(key, out float v)) return v;
        return 1f;
    }

    public void PlayMusic(string clipName, bool resume = false)
    {
        if (Suppressed) return;
        if (!music.ContainsKey(clipName) || musicSource == null)
            return;

        if (currentTrack == clipName)
            return;

        if (currentTrack != null && musicSource.isPlaying)
            musicPlaybackTimes[currentTrack] = musicSource.time;

        AudioClip clip = music[clipName];
        musicSource.clip = clip;
        musicSource.loop = true;
        musicSource.volume = masterMusicVolume * GetMusicVolume(clipName);
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
        masterMusicVolume = Mathf.Clamp01(volume);
        if (musicSource == null) return;

        if (currentTrack != null && musicVolumes != null && musicVolumes.ContainsKey(currentTrack))
            musicSource.volume = masterMusicVolume * musicVolumes[currentTrack];
        else
            musicSource.volume = masterMusicVolume;
    }

    public void PlaySFX(string clipName)
    {
        if (Suppressed) return;
        if (!sfx.ContainsKey(clipName) || sfxSource == null)
            return;

        AudioClip clip = sfx[clipName];
        if (clip == null) return;
        sfxSource.PlayOneShot(clip, GetSfxVolume(clipName));
    }

    public void SetSFXVolume(float volume)
    {
        masterSfxVolume = Mathf.Clamp01(volume);
        if (sfxSource != null)
            sfxSource.volume = masterSfxVolume;
    }

    private IEnumerator SoundEffectPerformer(SoundEffectGA soundEffectGA)
    {
        if (!Suppressed && soundEffectGA.sound != null && sfxSource != null)
            sfxSource.PlayOneShot(soundEffectGA.sound);

        yield return null;
    }

    private AreaThemeSet GetThemesForArea(int areaType)
    {
        switch (areaType)
        {
            case 0: return tutorialThemes;
            case 1: return neutralThemes;
            case 2: return fireThemes;
            case 3: return windThemes;
            case 4: return waterThemes;
            case 5: return earthThemes;
            case 6: return finalBossThemes;
            default: return null;
        }
    }

    public void SetAreaThemes(int areaType)
    {
        if (music == null) return;

        AreaThemeSet themes = GetThemesForArea(areaType);

        AudioClip newOverworld;
        float newOverworldVolume;
        if (themes != null && themes.overworld != null)
        {
            newOverworld = themes.overworld;
            newOverworldVolume = SanitizeVolume(themes.overworldVolume, themes.overworld);
        }
        else
        {
            newOverworld = overworldTheme;
            newOverworldVolume = SanitizeVolume(overworldThemeVolume, overworldTheme);
        }

        AudioClip newBattle;
        float newBattleVolume;
        if (themes != null && themes.battle != null)
        {
            newBattle = themes.battle;
            newBattleVolume = SanitizeVolume(themes.battleVolume, themes.battle);
        }
        else
        {
            newBattle = battleTheme;
            newBattleVolume = SanitizeVolume(battleThemeVolume, battleTheme);
        }

        bool overworldChanged = music.ContainsKey("overworld") && music["overworld"] != newOverworld;
        bool battleChanged = music.ContainsKey("battle") && music["battle"] != newBattle;

        music["overworld"] = newOverworld;
        music["battle"] = newBattle;
        musicVolumes["overworld"] = newOverworldVolume;
        musicVolumes["battle"] = newBattleVolume;

        if (overworldChanged && musicPlaybackTimes.ContainsKey("overworld"))
            musicPlaybackTimes["overworld"] = 0f;
        if (battleChanged && musicPlaybackTimes.ContainsKey("battle"))
            musicPlaybackTimes["battle"] = 0f;
    }

    public void SyncToArea(int areaType)
    {
        SetAreaThemes(areaType);

        if (Suppressed) return;
        if (music == null || !music.ContainsKey("overworld") || musicSource == null)
            return;

        AudioClip overworldClip = music["overworld"];
        if (overworldClip == null) return;

        if (currentTrack == "overworld" && musicSource.isPlaying && musicSource.clip == overworldClip)
            return;

        musicSource.Stop();

        float overworldVolume = GetMusicVolume("overworld");
        float whirlVolume = GetMusicVolume("portalWhirl");
        musicSource.volume = masterMusicVolume * overworldVolume;

        float whirlDelay = 0f;
        if (portalWhirlTheme != null)
        {
            float whirlScale = overworldVolume > 0.0001f ? (whirlVolume / overworldVolume) : whirlVolume;
            musicSource.PlayOneShot(portalWhirlTheme, whirlScale);
            whirlDelay = portalWhirlTheme.length;
        }

        musicSource.clip = overworldClip;
        musicSource.loop = true;
        musicSource.time = 0f;
        if (whirlDelay > 0f)
            musicSource.PlayDelayed(whirlDelay);
        else
            musicSource.Play();
        currentTrack = "overworld";
    }

    private void NextAreaPostReaction(NextAreaGA nextAreaGA)
    {
        if (LevelSystem.Instance == null) return;
        SyncToArea(LevelSystem.Instance.CurrentAreaType);
    }

    private void SpellCastPreReaction(SpellCastGA spellCastGA)
    {
        if (Suppressed) return;
        PlaySFX("spellCast");
    }
}
