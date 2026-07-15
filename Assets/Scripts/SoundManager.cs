using UnityEngine;

/// <summary>
/// Central audio manager. Handles gameplay music and sound effects.
/// Attach to any persistent GameObject in your scene (e.g. GameManager).
/// </summary>
public class SoundManager : MonoBehaviour
{
    public static SoundManager Instance { get; private set; }

    [Header("Audio Sources")]
    [Tooltip("AudioSource used for looping gameplay music.")]
    public AudioSource musicSource;
    [Tooltip("AudioSource used for one-shot sound effects.")]
    public AudioSource sfxSource;

    [Header("Music Clips")]
    [Tooltip("The background music that plays during gameplay.")]
    public AudioClip gameplayMusic;
    [Tooltip("The background music that plays on the Main Menu.")]
    public AudioClip mainMenuMusic;
    [Tooltip("The background music that plays in the Shop.")]
    public AudioClip shopMusic;
    [Tooltip("The sound that plays when the player dies.")]
    public AudioClip deathSound;
    
    [Header("UI Sounds")]
    public AudioClip hoverSound;
    public AudioClip clickSound;
    public AudioClip purchaseSound;
    public AudioClip equipSound;

    [Header("Settings")]
    [Range(0f, 1f)] public float musicVolume = 0.75f;
    [Range(0f, 1f)] public float sfxVolume = 1f;

    void Awake()
    {
        // Singleton pattern - only one SoundManager ever exists
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject); // Keep this object alive across all scenes!

        // Auto-create AudioSources if not manually assigned
        if (musicSource == null)
        {
            musicSource = gameObject.AddComponent<AudioSource>();
            musicSource.loop = true;
            musicSource.playOnAwake = false;
        }

        if (sfxSource == null)
        {
            sfxSource = gameObject.AddComponent<AudioSource>();
            sfxSource.loop = false;
            sfxSource.playOnAwake = false;
        }
    }

    void Start()
    {
        // We now let the GameManager (or other scenes) decide when to play music!
    }

    /// <summary>
    /// Starts (or restarts) the gameplay background music.
    /// </summary>
    public void PlayGameplayMusic()
    {
        if (gameplayMusic == null) return;

        musicSource.clip = gameplayMusic;
        musicSource.volume = musicVolume;
        musicSource.Play();
    }

    /// <summary>
    /// Starts the Main Menu background music.
    /// </summary>
    public void PlayMainMenuMusic()
    {
        if (mainMenuMusic == null) return;

        musicSource.clip = mainMenuMusic;
        musicSource.volume = musicVolume;
        musicSource.Play();
    }

    /// <summary>
    /// Starts the Shop background music.
    /// </summary>
    public void PlayShopMusic()
    {
        if (shopMusic == null) return;

        musicSource.clip = shopMusic;
        musicSource.volume = musicVolume;
        musicSource.Play();
    }

    public void PauseMusic()
    {
        if (musicSource != null && musicSource.isPlaying)
        {
            musicSource.Pause();
        }
    }

    public void ResumeMusic()
    {
        if (musicSource != null)
        {
            musicSource.UnPause();
        }
    }

    public void StopMusic()
    {
        if (musicSource != null)
        {
            musicSource.Stop();
        }
    }

    /// <summary>
    /// Stops the music and plays the death sound effect.
    /// Called by GameManager on death.
    /// </summary>
    public void PlayDeathSound()
    {
        // Stop music immediately
        musicSource.Stop();

        // Play death sfx
        if (deathSound != null)
            sfxSource.PlayOneShot(deathSound, sfxVolume);
    }
    
    // --- UI SOUND HELPERS ---
    
    public void PlayHoverSound()
    {
        if (hoverSound != null) sfxSource.PlayOneShot(hoverSound, sfxVolume * 0.5f); // Hover sounds should be quieter
    }

    public void PlayClickSound()
    {
        if (clickSound != null) sfxSource.PlayOneShot(clickSound, sfxVolume);
    }

    public void PlayPurchaseSound()
    {
        if (purchaseSound != null) sfxSource.PlayOneShot(purchaseSound, sfxVolume);
    }

    public void PlayEquipSound()
    {
        if (equipSound != null) sfxSource.PlayOneShot(equipSound, sfxVolume);
    }

    /// <summary>
    /// Play any one-shot sound effect clip.
    /// </summary>
    public void PlaySFX(AudioClip clip, float volumeScale = 1f)
    {
        if (clip == null) return;
        sfxSource.PlayOneShot(clip, sfxVolume * volumeScale);
    }

    /// <summary>
    /// Set music volume at runtime (e.g. from a settings menu).
    /// </summary>
    public void SetMusicVolume(float volume)
    {
        musicVolume = Mathf.Clamp01(volume);
        musicSource.volume = musicVolume;
    }

    public void SetSFXVolume(float volume)
    {
        sfxVolume = Mathf.Clamp01(volume);
    }

    private bool isMuted = false;
    
    /// <summary>
    /// Toggles all game audio on/off. Perfect for a Pause Menu button!
    /// </summary>
    public void ToggleMute()
    {
        isMuted = !isMuted;
        
        musicSource.mute = isMuted;
        sfxSource.mute = isMuted;
    }
}
