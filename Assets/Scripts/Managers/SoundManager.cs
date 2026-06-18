using UnityEngine;
using UnityEngine.Audio;

public class SoundManager : MonoBehaviour
{
    private static SoundManager _instance;
    public static SoundManager Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = FindFirstObjectByType<SoundManager>();
                if (_instance == null)
                {
                    GameObject obj = new GameObject("SoundManager");
                    _instance = obj.AddComponent<SoundManager>();
                    DontDestroyOnLoad(obj);
                }
            }
            return _instance;
        }
    }

    public AudioMixer AudioMixer { get; private set; }
    public AudioMixerGroup SfxGroup { get; private set; }
    public AudioMixerGroup MusicGroup { get; private set; }

    public AudioClip menuMusic;
    public AudioClip gameplayMusic;

    private const string SFX_VOLUME_PREF = "SFXVolume";
    private const string MUSIC_VOLUME_PREF = "MusicVolume";
    private float _currentSFXVolume = 0.5f;
    private float _currentMusicVolume = 0.5f;

    private AudioSource _musicSource1;
    private AudioSource _musicSource2;
    private AudioSource _activeMusicSource;
    private Coroutine _fadeCoroutine;

    private void Awake()
    {
        if (_instance != null && _instance != this)
        {
            Destroy(gameObject);
            return;
        }
        _instance = this;
        DontDestroyOnLoad(gameObject);

        _musicSource1 = gameObject.AddComponent<AudioSource>();
        _musicSource2 = gameObject.AddComponent<AudioSource>();
        _musicSource1.loop = true;
        _musicSource2.loop = true;

        InitializeAudio();
    }

    private void Start()
    {
        StartCoroutine(ApplyInitialVolumes());
    }

    private System.Collections.IEnumerator ApplyInitialVolumes()
    {
        yield return null;
        ApplySFXVolume(_currentSFXVolume);
        ApplyMusicVolume(_currentMusicVolume);
    }

    private void OnEnable()
    {
        UnityEngine.SceneManagement.SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable()
    {
        UnityEngine.SceneManagement.SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void InitializeAudio()
    {
        AudioMixer = Resources.Load<AudioMixer>("AudioMixer");

        if (AudioMixer != null)
        {
            AudioMixerGroup[] groups = AudioMixer.FindMatchingGroups("SFX");
            if (groups != null && groups.Length > 0)
            {
                SfxGroup = groups[0];
            }
            AudioMixerGroup[] musicGroups = AudioMixer.FindMatchingGroups("Music");
            if (musicGroups != null && musicGroups.Length > 0)
            {
                MusicGroup = musicGroups[0];
                if (_musicSource1 != null) _musicSource1.outputAudioMixerGroup = MusicGroup;
                if (_musicSource2 != null) _musicSource2.outputAudioMixerGroup = MusicGroup;
            }
        }

        _currentSFXVolume = PlayerPrefs.GetFloat(SFX_VOLUME_PREF, 0.5f);
        _currentMusicVolume = PlayerPrefs.GetFloat(MUSIC_VOLUME_PREF, 0.5f);
    }

    private void OnSceneLoaded(UnityEngine.SceneManagement.Scene scene, UnityEngine.SceneManagement.LoadSceneMode mode)
    {
        if (scene.name == "MainMenu")
        {
            PlayMusic(menuMusic);
        }
        else if (scene.name == "LobbyScene" || scene.name == "SampleScene")
        {
            PlayMusic(gameplayMusic);
        }
    }

    public void PlayMusic(AudioClip clip, float fadeDuration = 3.0f)
    {
        if (clip == null) return;
        if (_activeMusicSource != null && _activeMusicSource.clip == clip) return;

        if (_fadeCoroutine != null)
        {
            StopCoroutine(_fadeCoroutine);
        }

        AudioSource nextSource = (_activeMusicSource == _musicSource1) ? _musicSource2 : _musicSource1;
        nextSource.clip = clip;
        nextSource.Play();

        _fadeCoroutine = StartCoroutine(FadeMusicCoroutine(_activeMusicSource, nextSource, fadeDuration));
        _activeMusicSource = nextSource;
    }

    private System.Collections.IEnumerator FadeMusicCoroutine(AudioSource fromSource, AudioSource toSource, float duration)
    {
        float elapsed = 0f;
        float targetVolume = 1f;

        if (fromSource == null)
        {
            toSource.volume = 0f;
            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                toSource.volume = Mathf.Lerp(0f, targetVolume, elapsed / duration);
                yield return null;
            }
            toSource.volume = targetVolume;
        }
        else
        {
            float startFromVolume = fromSource.volume;
            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / duration;
                fromSource.volume = Mathf.Lerp(startFromVolume, 0f, t);
                toSource.volume = Mathf.Lerp(0f, targetVolume, t);
                yield return null;
            }
            fromSource.volume = 0f;
            fromSource.Stop();
            toSource.volume = targetVolume;
        }
    }

    public float GetSFXVolume() => _currentSFXVolume;

    public void SetSFXVolume(float volume)
    {
        _currentSFXVolume = Mathf.Clamp01(volume);
        PlayerPrefs.SetFloat(SFX_VOLUME_PREF, _currentSFXVolume);
        PlayerPrefs.Save();
        ApplySFXVolume(_currentSFXVolume);
    }

    private void ApplySFXVolume(float volume)
    {
        if (AudioMixer == null) return;
        float db = (volume > 0.0001f) ? (volume - 1f) * 40f : -80f;
        AudioMixer.SetFloat("SFXVolume", db);
    }

    public float GetMusicVolume() => _currentMusicVolume;

    public void SetMusicVolume(float volume)
    {
        _currentMusicVolume = Mathf.Clamp01(volume);
        PlayerPrefs.SetFloat(MUSIC_VOLUME_PREF, _currentMusicVolume);
        PlayerPrefs.Save();
        ApplyMusicVolume(_currentMusicVolume);
    }

    private void ApplyMusicVolume(float volume)
    {
        if (AudioMixer == null) return;
        float db = (volume > 0.0001f) ? (volume - 1f) * 40f : -80f;
        AudioMixer.SetFloat("MusicVolume", db);
    }

    public void PlaySFXAtPoint(AudioClip clip, Vector3 position, float volume = 1f, float minPitch = 0.9f, float maxPitch = 1.1f)
    {
        if (clip == null) return;

        GameObject sfxObj = new GameObject("TempSFX_" + clip.name);
        sfxObj.transform.position = position;

        AudioSource source = sfxObj.AddComponent<AudioSource>();
        source.clip = clip;
        source.volume = volume;
        source.pitch = Random.Range(minPitch, maxPitch);
        source.spatialBlend = 1f;
        source.minDistance = 2f;
        source.maxDistance = 30f;
        source.rolloffMode = AudioRolloffMode.Logarithmic;

        if (SfxGroup != null) source.outputAudioMixerGroup = SfxGroup;

        source.Play();
        Destroy(sfxObj, clip.length + 0.1f);
    }

    public void PlaySFXOnSource(AudioSource source, AudioClip clip, float volume = 1f, float minPitch = 0.9f, float maxPitch = 1.1f)
    {
        if (source == null || clip == null) return;

        if (SfxGroup != null && source.outputAudioMixerGroup != SfxGroup)
        {
            source.outputAudioMixerGroup = SfxGroup;
        }

        source.pitch = Random.Range(minPitch, maxPitch);
        source.PlayOneShot(clip, volume);
    }
}
