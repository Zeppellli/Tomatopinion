using System;
using System.Collections;
using Mali.Utils;
using UnityEngine;

public class SoundManager : Singleton<SoundManager>
{
    public enum Voices
    {
        None,
        Caveman_A,
        Caveman_B,
        Young_Philosopher,
        Old_Philosopher,
        Executioner,
        Dame,
        Choir,
        Clergyman,
        Actor,
        Laughing_Audience,
        Revolutionary,
        ONU_Representative_A,
        ONU_Representative_B,
        ONU_Representative_C,
        Cavewoman,
    }

    [Header("SetUp")]
    [SerializeField] private VoiceLinker[] all_voices;
    [SerializeField] private AudioSource musicSource;
    [SerializeField] private AudioSource voiceSource;
    [SerializeField] private AudioSource ambianceSource;
    [SerializeField] private bool randomizeStartTime;

    [Header("Voice Smoothing")]
    [SerializeField] private float fadeInDuration;
    [SerializeField] private float fadeOutDuration;
    [SerializeField, Range(0f, 1f)] private float maxVoiceVolume;
    [SerializeField, Range(0f, 1f)] private float maxAmbianceVolume;

    public static Action<Voices> OnVoiceStart;

    private void Start()
    {
        musicSource.Play();
    }

    public IEnumerator PlayForDuration(Voices voice, float duration)
    {
        AudioClip clip = GetClipFromVoice(voice);
        voiceSource.clip = clip;
        voiceSource.volume = 0;

        if (randomizeStartTime)
        {
            float maxStart = Mathf.Max(0f, clip.length - duration);
            voiceSource.time = UnityEngine.Random.Range(0f, maxStart);
        }

        voiceSource.volume = 0; //force silence before fade in
        voiceSource.Play();
        OnVoiceStart?.Invoke(voice);
        Debug.Log($"PLAYING {voice} from {voiceSource.time:F2}s");

        float fadeIn = Mathf.Min(fadeInDuration, duration / 2f);
        float fadeOut = Mathf.Min(fadeOutDuration, duration / 2f);

        yield return StartCoroutine(FadeVolume(voiceSource, 0f, maxVoiceVolume, fadeIn));

        yield return new WaitForSeconds(duration - fadeIn - fadeOut);

        yield return StartCoroutine(FadeVolume(voiceSource, maxVoiceVolume, 0f, fadeOut));

        voiceSource.Stop();
    }

    public void StartAmbiance(AudioClip clip)
    {
        Debug.Log($"AMBIANCE - {clip.name}");
        StartCoroutine(FadeToNewAmbiance(clip));
    }
    private IEnumerator FadeToNewAmbiance(AudioClip clip)
    {
        yield return StartCoroutine(FadeVolume(voiceSource, maxAmbianceVolume, 0f, 1f)); //out

        ambianceSource.clip = clip;
        ambianceSource.Play();

        yield return StartCoroutine(FadeVolume(voiceSource, 0f, maxAmbianceVolume, 1f)); //in
    }
    

    private IEnumerator FadeVolume(AudioSource source, float from, float to, float time)
    {
        float elapsed = 0f;

        while (elapsed < time)
        {
            elapsed += Time.deltaTime;
            source.volume = Mathf.Lerp(from, to, elapsed / time);
            yield return null;
        }

        source.volume = to;
    }

    private AudioClip GetClipFromVoice(Voices voice)
    {
        foreach (VoiceLinker link in all_voices)
        {
            if (link.voiceName == voice) { return link.audioClip; }
        }

        Debug.LogError($"Couldn't find any clip with the given Voice! --> {voice}");
        return null;
    }

}

[Serializable]
public class VoiceLinker
{
    public SoundManager.Voices voiceName;
    public AudioClip audioClip;
}