using UnityEngine;

public enum Sound { Pickup, Powerup, Jump, Flip, Damage, Goal, Click }

public class GameAudio : MonoBehaviour
{
    static GameAudio instance;
    AudioSource source;
    AudioClip[] clips;
    void Awake()
    {
        instance = this;
        source = gameObject.AddComponent<AudioSource>();
        source.spatialBlend = 0f;
        source.volume = .23f;
        clips = new AudioClip[7];
        clips[0] = Tone("Score", 880f, 1320f, .12f);
        clips[1] = Tone("Powerup", 440f, 1100f, .25f);
        clips[2] = Tone("Jump", 180f, 320f, .12f);
        clips[3] = Tone("Flip", 320f, 960f, .22f);
        clips[4] = Tone("Damage", 180f, 55f, .23f);
        clips[5] = Tone("Goal", 523f, 1046f, .55f);
        clips[6] = Tone("Click", 600f, 500f, .05f);
    }
    static AudioClip Tone(string name, float from, float to, float duration)
    {
        const int rate = 44100;
        float[] samples = new float[Mathf.CeilToInt(duration * rate)];
        float phase = 0f;
        for (int i = 0; i < samples.Length; i++)
        {
            float t = (float)i / samples.Length;
            phase += Mathf.Lerp(from, to, t) * 2f * Mathf.PI / rate;
            samples[i] = (Mathf.Sin(phase) + .16f * Mathf.Sin(phase * 2f)) * Mathf.Sin(t * Mathf.PI) * (1f - t) * .65f;
        }
        AudioClip clip = AudioClip.Create(name, samples.Length, 1, rate, false);
        clip.SetData(samples, 0);
        return clip;
    }
    public static void Play(Sound sound, Vector3 position)
    {
        if (instance != null) instance.source.PlayOneShot(instance.clips[(int)sound]);
    }
    void OnDestroy()
    {
        if (clips != null) foreach (AudioClip clip in clips) if (clip != null) Destroy(clip);
        if (instance == this) instance = null;
    }
}

