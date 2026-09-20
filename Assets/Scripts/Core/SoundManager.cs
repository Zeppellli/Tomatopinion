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
    [SerializeField, Range(0f, 1f)] private float maxVoiceVolume = 1f;
    [SerializeField, Range(0f, 1f)] private float maxAmbianceVolume = 1f;

    [Header("Shock FX")]
    [SerializeField] private AudioClip[] allShocks;

    public static Action<Voices> OnVoiceStart;

    private int voiceToken;
    private Voices currentVoice = Voices.None;
    private Coroutine ambianceRoutine;

    private void Start()
    {
        musicSource.Play();
    }

    public IEnumerator PlayForDuration(Voices voice, float duration)
    {
        OnVoiceStart?.Invoke(voice);
        if (voice == Voices.None) { yield break; }

        int token = ++voiceToken;

        AudioClip clip = GetClipFromVoice(voice);
        if (clip == null) yield break;

        bool alreadyPlaying = voiceSource.isPlaying && currentVoice == voice;

        if (!alreadyPlaying)
        {
            voiceSource.Stop();
            voiceSource.clip = clip;
            voiceSource.volume = 0f;
            voiceSource.loop = true;

            if (randomizeStartTime)
            {
                float maxStart = Mathf.Max(0f, clip.length - duration);
                voiceSource.time = UnityEngine.Random.Range(0f, maxStart);
            }

            voiceSource.Play();
            currentVoice = voice;
            //OnVoiceStart?.Invoke(voice);
            Debug.Log($"PLAYING {voice} from {voiceSource.time:F2}s");
        }

        float fadeIn = Mathf.Min(fadeInDuration, duration / 2f);
        float fadeOut = Mathf.Min(fadeOutDuration, duration / 2f);

        yield return StartCoroutine(FadeVoice(token, maxVoiceVolume, fadeIn));
        if (token != voiceToken) yield break;

        yield return new WaitForSeconds(Mathf.Max(0f, duration - fadeIn - fadeOut));
        if (token != voiceToken) yield break;

        yield return StartCoroutine(FadeVoice(token, 0f, fadeOut));
        if (token != voiceToken) yield break;

        voiceSource.Stop();
        currentVoice = Voices.None;
    }

    public void EarlyLineStop(bool playShockSound = true)
    {
        voiceToken++;
        currentVoice = Voices.None;

        voiceSource.Stop();
        voiceSource.volume = maxVoiceVolume;

        if (playShockSound && allShocks != null && allShocks.Length > 0)
        {
            voiceSource.PlayOneShot(allShocks[UnityEngine.Random.Range(0, allShocks.Length)]);
        }
    }

    public void StartAmbiance(AudioClip clip)
    {
        Debug.Log($"AMBIANCE - {clip.name}");

        if (ambianceRoutine != null) StopCoroutine(ambianceRoutine);
        ambianceRoutine = StartCoroutine(FadeToNewAmbiance(clip));
    }

    private IEnumerator FadeToNewAmbiance(AudioClip clip)
    {
        if (ambianceSource.isPlaying)
        {
            yield return StartCoroutine(FadeVolume(ambianceSource, 0f, 1f));
        }

        ambianceSource.clip = clip;
        ambianceSource.volume = 0f;
        ambianceSource.Play();

        yield return StartCoroutine(FadeVolume(ambianceSource, maxAmbianceVolume, 1f));

        ambianceRoutine = null;
    }

    private IEnumerator FadeVoice(int token, float to, float time)
    {
        float from = voiceSource.volume;
        float elapsed = 0f;

        while (elapsed < time)
        {
            if (token != voiceToken) yield break;

            elapsed += Time.deltaTime;
            voiceSource.volume = Mathf.Lerp(from, to, elapsed / time);
            yield return null;
        }

        if (token == voiceToken)
        {
            voiceSource.volume = to;
        }
    }

    private IEnumerator FadeVolume(AudioSource source, float to, float time)
    {
        float from = source.volume;
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