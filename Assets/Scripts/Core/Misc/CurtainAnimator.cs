using System.Collections;
using Mali.Utils;
using UnityEngine;

public class CurtainAnimator : Singleton<CurtainAnimator>
{
    [SerializeField] private float rightmostX;
    [SerializeField] private float midX;
    [SerializeField] private float leftmostX;
    [SerializeField] private float slideDuration;

    private RectTransform rectTransform;
    private Coroutine routine;

    private void Awake()
    {
        rectTransform = (RectTransform)transform;
    }

    private void Start()
    {
        SetX(rightmostX);
    }

    public void PlayCurtainAnim()
    {
        if (routine != null) { StopCoroutine(routine); }

        routine = StartCoroutine(AnimateRoutine());
    }

    private IEnumerator AnimateRoutine()
    {
        SetX(rightmostX);

        yield return SlideRoutine(rightmostX, midX);

        GameSceneManager.OnCurtainCover?.Invoke();

        yield return SlideRoutine(midX, leftmostX);

        routine = null;
    }

    private IEnumerator SlideRoutine(float from, float to)
    {
        float elapsed = 0f;

        while (elapsed < slideDuration)
        {
            elapsed += Time.unscaledDeltaTime;
            float t = Mathf.Clamp01(elapsed / slideDuration);
            t = Mathf.SmoothStep(0f, 1f, t);

            SetX(Mathf.Lerp(from, to, t));
            yield return null;
        }

        SetX(to);
    }

    private void SetX(float x)
    {
        Vector2 pos = rectTransform.anchoredPosition;
        pos.x = x;
        rectTransform.anchoredPosition = pos;
    }
}