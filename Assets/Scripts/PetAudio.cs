using UnityEngine;

public class PetAudio : MonoBehaviour
{
    [Header("Volume")]
    [Range(0f, 1f)] public float masterVolume = 0.6f;

    private AudioSource source;
    private AudioClip feedClip;
    private AudioClip playClip;
    private AudioClip sleepClip;
    private AudioClip wakeClip;
    private AudioClip blockedClip;
    private AudioClip sadClip;
    private AudioClip gameOverClip;
    private AudioClip victoryClip;

    private void Awake()
    {
        source = gameObject.AddComponent<AudioSource>();
        source.playOnAwake = false;
        source.loop = false;
        source.volume = masterVolume;

        feedClip = MakeTone(660f, 0.14f, 0.5f, 990f);
        playClip = MakeTone(520f, 0.18f, 0.5f, 780f);
        sleepClip = MakeTone(320f, 0.5f, 0.4f, 200f);
        wakeClip = MakeTone(500f, 0.18f, 0.45f, 740f);
        blockedClip = MakeTone(160f, 0.12f, 0.5f, 120f);
        sadClip = MakeTone(420f, 0.3f, 0.35f, 240f);
        gameOverClip = MakeTone(380f, 1.1f, 0.55f, 90f);
        victoryClip = MakeTone(600f, 0.55f, 0.5f, 1100f);

        Pet pet = GetComponent<Pet>();
        if (pet != null)
        {
            pet.ActionPerformed += OnAction;
            pet.ActionBlocked += OnBlocked;
            pet.StateChanged += OnStateChanged;
            pet.GameOver += () => Play(gameOverClip);
            pet.Victory += () => Play(victoryClip);
        }
    }

    private void OnAction(PetAction action)
    {
        switch (action)
        {
            case PetAction.Feed: Play(feedClip); break;
            case PetAction.Play: Play(playClip); break;
            case PetAction.Sleep:
                Pet pet = GetComponent<Pet>();
                if (pet != null && pet.IsSleeping) Play(sleepClip);
                else Play(wakeClip);
                break;
        }
    }

    private void OnBlocked(PetAction action)
    {
        Play(blockedClip);
    }

    private void OnStateChanged(PetState previous, PetState next)
    {
        if (next == PetState.Sad || next == PetState.Hungry || next == PetState.Tired || next == PetState.Sick)
            Play(sadClip);
    }

    private void Play(AudioClip clip)
    {
        if (clip == null) return;
        source.PlayOneShot(clip, masterVolume);
    }

    private static AudioClip MakeTone(float freq, float duration, float volume, float freqEnd = -1f)
    {
        int sampleRate = 44100;
        int samples = Mathf.Max(1, (int)(sampleRate * duration));
        float[] data = new float[samples];
        float end = freqEnd > 0f ? freqEnd : freq;

        for (int i = 0; i < samples; i++)
        {
            float t = i / (float)sampleRate;
            float f = Mathf.Lerp(freq, end, t / duration);
            float attack = Mathf.Min(1f, t / 0.015f);
            float release = Mathf.Clamp01(1f - t / duration);
            data[i] = Mathf.Sin(2f * Mathf.PI * f * t) * attack * release * volume;
        }

        AudioClip clip = AudioClip.Create("tone_" + freq, samples, 1, sampleRate, false);
        clip.SetData(data, 0);
        return clip;
    }
}
