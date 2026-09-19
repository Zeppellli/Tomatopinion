using UnityEngine;
using Mali.Utils;
using TMPro;
using UnityEngine.UI;
using System.Collections;
using System;
using System.Text.RegularExpressions;
using System.Globalization;

public class DialogueManager : Singleton<DialogueManager>
{
    [Header("SetUp")]
    [SerializeField] private TextMeshProUGUI textHolder;
    [SerializeField] private Image textBG;
    [SerializeField] private DialogueLinker[] all_dialogues;
    public static DialogueLinker[] ALL_SCENE_DIALOGUES { get; private set; }
    [SerializeField] private const string DIALOGUEENDING_STRING_MARKER = "//END//";
    [SerializeField] private static readonly Regex DELAY_MARKER_REGEX = new Regex(@"<(\d+(?:\.\d+)?)>");

    [Header("Visual")]
    [SerializeField] private float fadeDuration;

    private void Awake()
    {
        base.Awake();
        ALL_SCENE_DIALOGUES = all_dialogues;

        GameSceneManager.OnSceneLoaded += StartDialogue;
        GameSceneManager.OnSceneUnloaded += StopDialogue;
    }
    private void OnDestroy()
    {
        GameSceneManager.OnSceneLoaded -= StartDialogue;
        GameSceneManager.OnSceneUnloaded -= StopDialogue;
    }

    private void StartDialogue(string id)
    {
        GetLinkerFromID(id).isStopped = false;

        StartCoroutine(SayLine(GetLinkerFromID(id).dialogue, 0, id));
    }
    private void StopDialogue(string id)
    {
        GetLinkerFromID(id).isStopped = true;

        StartCoroutine(FadeImage(textBG, 0, fadeDuration));
        textHolder.text = "";
    }

    private IEnumerator SayLine(FullSceneDialogue dialogue, int lineIndex, string sceneID)
    {
        if (GetLinkerFromID(sceneID).isStopped) { yield break; } //EARLY STOP IF SCENE WAS CUT SHORT

        if (lineIndex >= dialogue.dialogueLinesInOrder.Length) //RECURSIVE EARLY RETURN
        {
            StartCoroutine(FadeImage(textBG, 0, fadeDuration));
            yield break;
        }

        if (dialogue.dialogueLinesInOrder[lineIndex].line.Contains(DIALOGUEENDING_STRING_MARKER)) //GO TO NEXT SCENE 
        {
            StartCoroutine(FadeImage(textBG, 0, fadeDuration));

            yield return new WaitForSeconds(dialogue.dialogueLinesInOrder[lineIndex].duration);

            string foundID = Regex.Match(dialogue.dialogueLinesInOrder[lineIndex].line, @"\((.*?)\)").Groups[1].Value;
            GameSceneManager.Instance.GoToScene(foundID);

            yield break;
        }

        if (textBG.color.a != 1) //if already on, don't wait again
        {
            yield return StartCoroutine(FadeImage(textBG, 1, fadeDuration));
        }

        DialogueLine currentLine = dialogue.dialogueLinesInOrder[lineIndex];

        /*textHolder.text = currentLine.line;
        StartCoroutine(SoundManager.Instance.PlayForDuration(currentLine.voice, currentLine.duration));

        yield return new WaitForSeconds(currentLine.duration);*/
        yield return StartCoroutine(ShowLineInSequence(currentLine.line, currentLine.duration, sceneID));

        textHolder.text = "";

        if (currentLine.backgroundFadeAwayOnEnd)
        {
            StartCoroutine(FadeImage(textBG, 0, fadeDuration));
        }
        yield return new WaitForSeconds(currentLine.delayBeforeNextLine);

        lineIndex++;
        StartCoroutine(SayLine(dialogue, lineIndex, sceneID));
    }

    private IEnumerator ShowLineInSequence(string rawLine, float totalDuration, string sceneID)
    {
        string[] parts = DELAY_MARKER_REGEX.Split(rawLine);
        string shown = "";
        float elapsed = 0f;

        for (int i = 0; i < parts.Length; i++)
        {
            if (i % 2 == 0) // TEXT SEGMENT
            {
                string segment = parts[i].Trim();
                if (segment.Length == 0) { continue; }

                shown = shown.Length == 0 ? segment : shown + " " + segment;
                textHolder.text = shown;
            }
            else // DELAY VALUE
            {
                float delay = float.Parse(parts[i], CultureInfo.InvariantCulture);
                yield return new WaitForSeconds(delay);
                elapsed += delay;

                if (GetLinkerFromID(sceneID).isStopped) { yield break; } //SCENE CUT SHORT MID-LINE
            }
        }

        // Hold the fully revealed line for whatever time is left in the line's duration
        float remaining = totalDuration - elapsed;
        if (remaining > 0f)
        {
            yield return new WaitForSeconds(remaining);
        }
    }

    private DialogueLinker GetLinkerFromID(string id)
    {
        foreach (DialogueLinker link in ALL_SCENE_DIALOGUES)
        {
            if (link.id == id) { return link; }
        }

        Debug.LogError($"Couldn't find any linker with the given ID! --> {id}");
        return null;
    }

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

[Serializable]
public class DialogueLinker
{
    public string id;
    public FullSceneDialogue dialogue;
    [HideInInspector] public bool isStopped = false;
}