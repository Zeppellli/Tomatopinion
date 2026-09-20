using System.Data;
using UnityEngine;
using UnityEngine.UI;

public class SpeechAnimator : MonoBehaviour
{
    [SerializeField] private SoundManager.Voices linkedToVoice;
    [SerializeField] private Sprite[] allFrames;
    private int currentFrame;
    [SerializeField] float frameInterval;
    float intervalTimer;

    private void Awake()
    {
        SoundManager.OnVoiceStart += CheckEnable;
    }
    private void OnDestroy()
    {
        SoundManager.OnVoiceStart -= CheckEnable;
    }
    
    private void CheckEnable(SoundManager.Voices voice)
    {
        gameObject.SetActive(voice == linkedToVoice);
    }

    void OnEnable()
    {
        currentFrame = 0;
        intervalTimer = frameInterval;
    }

    void Update()
    {
        intervalTimer -= Time.deltaTime;
        if (intervalTimer <= 0)
        {
            currentFrame = (currentFrame + 1) % allFrames.Length;
            GetComponent<Image>().sprite = allFrames[currentFrame];
            intervalTimer = frameInterval;
        }
    }
}
