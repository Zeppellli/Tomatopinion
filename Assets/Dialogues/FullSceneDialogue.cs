using System;
using UnityEngine;

[CreateAssetMenu(fileName = "FullSceneDialogue", menuName = "Scriptable Objects/FullSceneDialogue")]
public class FullSceneDialogue : ScriptableObject
{
    [SerializeField] public DialogueLine[] dialogueLinesInOrder;
}

[Serializable]
public class DialogueLine
{
    public string line;
    public float duration;
    public float delayBeforeNextLine;
    public bool backgroundFadeAwayOnEnd;
}