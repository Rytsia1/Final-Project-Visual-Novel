using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Pemutar BGM global (bootstrap otomatis, DontDestroyOnLoad) dengan crossfade antar track.
/// Track diambil dari Resources/Music. Main menu -> Main_Menu, scene gameplay -> Prologue
/// selama sesi prolog atau Blooming selain itu; GameManager mengganti ke Blooming saat prolog selesai.
/// Volume mengikuti PlayerPrefs "Config_BGM" (slider BGM di Config).
/// </summary>
public class MusicPlayer : MonoBehaviour
{
    public const string TRACK_MAIN_MENU = "Main_Menu";
    public const string TRACK_PROLOGUE = "Prologue";
    public const string TRACK_GAMEPLAY = "Blooming";

    private const string MAIN_MENU_SCENE = "MainMenuScene";
    private const float CROSSFADE_DURATION = 1.5f;

    private static MusicPlayer _instance;

    private AudioSource _active, _idle;
    private string _currentTrack;
    private float _volume;
    private Coroutine _fade;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void Bootstrap()
    {
        GameObject go = new GameObject("MusicPlayer");
        DontDestroyOnLoad(go);
        _instance = go.AddComponent<MusicPlayer>();
    }

    void Awake()
    {
        _volume = PlayerPrefs.GetFloat("Config_BGM", 0.8f);
        _active = CreateSource();
        _idle = CreateSource();
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private AudioSource CreateSource()
    {
        AudioSource src = gameObject.AddComponent<AudioSource>();
        src.loop = true;
        src.playOnAwake = false;
        src.volume = 0f;
        return src;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (scene.name == MAIN_MENU_SCENE) Play(TRACK_MAIN_MENU);
        else Play(PlayerPrefs.GetInt("IsNewGamePrologue", 0) == 1 ? TRACK_PROLOGUE : TRACK_GAMEPLAY);
    }

    /// <summary>Crossfade ke track baru; tidak melakukan apa-apa jika track tersebut sudah diputar.</summary>
    public static void Play(string trackName)
    {
        if (_instance == null || _instance._currentTrack == trackName) return;

        AudioClip clip = Resources.Load<AudioClip>("Music/" + trackName);
        if (clip == null)
        {
            Debug.LogWarning($"<color=yellow>[MusicPlayer]</color> Track 'Resources/Music/{trackName}' tidak ditemukan.");
            return;
        }

        _instance._currentTrack = trackName;
        _instance.CrossfadeTo(clip);
    }

    public static void SetVolume(float volume)
    {
        if (_instance == null) return;
        _instance._volume = volume;
        if (_instance._fade == null) _instance._active.volume = volume;
    }

    private void CrossfadeTo(AudioClip clip)
    {
        // Tukar source: track lama memudar di _idle, track baru naik di _active
        (_active, _idle) = (_idle, _active);
        _active.clip = clip;
        _active.volume = 0f;
        _active.Play();

        if (_fade != null) StopCoroutine(_fade);
        _fade = StartCoroutine(CrossfadeRoutine());
    }

    private IEnumerator CrossfadeRoutine()
    {
        float startIdle = _idle.volume;
        for (float t = 0f; t < CROSSFADE_DURATION; t += Time.unscaledDeltaTime)
        {
            float k = t / CROSSFADE_DURATION;
            _active.volume = Mathf.Lerp(0f, _volume, k);
            _idle.volume = Mathf.Lerp(startIdle, 0f, k);
            yield return null;
        }
        _active.volume = _volume;
        _idle.volume = 0f;
        _idle.Stop();
        _idle.clip = null;
        _fade = null;
    }
}
