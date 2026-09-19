using UnityEngine;
using Mali.Utils;
using TMPro;
using UnityEngine.UI;
using System.Collections;

public class DialogueManager : Singleton<DialogueManager>
{
    [Header("SetUp")]
    [SerializeField] private TextMeshProUGUI textHolder;
    [SerializeField] private Image textBG;

    [Header("Visual")]
    [SerializeField] private float fadeDuration;


    private IEnumerator FadeImage(Image image, float targetAlpha, float duration)
    {
        Color color = image.color;
        float startAlpha = color.a;

        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;

            color.a = Mathf.Lerp(startAlpha, targetAlpha, elapsed / duration);
            image.color = color;

            yield return null;
        }

        color.a = targetAlpha;
        image.color = color;
    }

    //DEBUG CONTROLS
#if UNITY_EDITOR
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.UpArrow))
        {
            StartCoroutine(FadeImage(textBG, 1, fadeDuration));
        }
        if (Input.GetKeyDown(KeyCode.DownArrow))
        {
            StartCoroutine(FadeImage(textBG, 0, fadeDuration));
        }
    }
#endif
}
