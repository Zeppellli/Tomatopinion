using UnityEngine;
using Mali.Utils;
using System.Collections;
using System;

public class GameSceneManager : Singleton<GameSceneManager>
{
    [SerializeField] private string[] scene_IDs;
    public static string[] ALL_SCENE_IDS { get; private set; }

    [SerializeField] private string startingScene = "0";
    private string currentScene;

    public static Action<string> OnSceneLoaded;
    public static Action<string> OnSceneUnloaded;

    private void Awake()
    {
        base.Awake();
        ALL_SCENE_IDS = scene_IDs;
    }

    private void Start()
    {
        Initialize();
    }

    private void Initialize()
    {
        currentScene = startingScene;
        LoadScene(currentScene);
    }

    public string GetCurrentScene()
    {
        return currentScene;
    }

    public void GoToScene(string id)
    {
        StartCoroutine(UnloadSceneNextFrame(currentScene));

        LoadScene(id);
    }

    private void LoadScene(string id)
    {
        currentScene = id;
        OnSceneLoaded?.Invoke(id);
    }
    
    private IEnumerator UnloadSceneNextFrame(string id)
    {
        yield return null;

        OnSceneUnloaded?.Invoke(id);
    }
}
