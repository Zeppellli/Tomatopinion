using UnityEngine;

public class Bobbing : MonoBehaviour
{
    [SerializeField] private SoundManager.Voices linkedToVoice;
    private bool isTalking = false;
    [SerializeField] private bool alwaysBop = false;
    [SerializeField] private float bopHeight = 4f;
    [SerializeField] private float bopPerSec = 3f;
    [SerializeField] private float easeSpeed = 6f;

    private RectTransform rect;
    private float weight;       
    private float phase;        
    private float lastOffsetY;

    private void Awake()
    {
        rect = GetComponent<RectTransform>();
        SoundManager.OnVoiceStart += EnableBobbing;
    }
    private void OnDestroy()
    {
        SoundManager.OnVoiceStart -= EnableBobbing;
    }

    private void OnDisable()
    {
        RemoveOffset();
        weight = 0f;
        phase = 0f;
    }

    private void EnableBobbing(SoundManager.Voices voice)
    {
        isTalking = voice == linkedToVoice;
    }

    private void LateUpdate()
    {
        float dt = Time.deltaTime;

        bool shouldBop = alwaysBop || isTalking;
        weight = Mathf.MoveTowards(weight, shouldBop ? 1f : 0f, easeSpeed * dt);

        if (shouldBop || weight > 0f)
            phase += bopPerSec * dt * Mathf.PI * 2f;

        float newOffsetY = Mathf.Sin(phase) * bopHeight * weight;

        Vector2 pos = rect.anchoredPosition;
        pos.y += newOffsetY - lastOffsetY;
        rect.anchoredPosition = pos;

        lastOffsetY = newOffsetY;
    }

    private void RemoveOffset()
    {
        if (rect == null) return;

        Vector2 pos = rect.anchoredPosition;
        pos.y -= lastOffsetY;
        rect.anchoredPosition = pos;
        lastOffsetY = 0f;
    }
}