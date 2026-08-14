using UnityEngine;

public class BackgroundMusic : MonoBehaviour
{
    private const string MuteKey = "CreatureCare.MusicMuted.v1";

    [Range(0f, 1f)] public float volume = 0.45f;
    public string clipPath = "Music/background_music";

    private AudioSource source;
    private bool muted;

    public bool IsMuted => muted;
    public static BackgroundMusic Instance { get; private set; }

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void ResetStatics()
    {
        Instance = null;
    }

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void AutoCreate()
    {
        if (Instance != null) return;
        var existing = Object.FindFirstObjectByType<BackgroundMusic>();
        if (existing != null)
        {
            Instance = existing;
            return;
        }
        var go = new GameObject("BackgroundMusic");
        go.AddComponent<BackgroundMusic>();
    }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        source = gameObject.AddComponent<AudioSource>();
        source.loop = true;
        source.playOnAwake = true;
        source.spatialBlend = 0f;
        source.volume = volume;

        AudioClip clip = Resources.Load<AudioClip>(clipPath);
        if (clip != null)
        {
            source.clip = clip;
            source.Play();
        }

        muted = PlayerPrefs.GetInt(MuteKey, 0) == 1;
        ApplyMute();
    }

    public void SetMuted(bool value)
    {
        muted = value;
        PlayerPrefs.SetInt(MuteKey, muted ? 1 : 0);
        PlayerPrefs.Save();
        ApplyMute();
    }

    public void Toggle()
    {
        SetMuted(!muted);
    }

    private void ApplyMute()
    {
        if (source != null) source.mute = muted;
    }
}
