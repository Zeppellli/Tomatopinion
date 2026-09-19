using UnityEngine;
using Mali.Utils;
using System.Collections;
using System;

public class GameSceneManager : Singleton<GameSceneManager>
{
    [SerializeField] private string[] scene_IDs;
    public static string[] ALL_SCENE_IDS { get; private set; }

    [SerializeField] private string startingSceneID = "0";
    private string currentSceneID;
    private int currentSceneIndex; //this one is mostly for debug

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
        currentSceneID = startingSceneID;
        currentSceneIndex = Array.IndexOf(ALL_SCENE_IDS, startingSceneID);

        UnloadScene(currentSceneID);
        LoadScene(currentSceneID);
    }

    public string GetCurrentScene()
    {
        return currentSceneID;
    }

    public void GoToScene(string id)
    {
        UnloadScene(currentSceneID);

        LoadScene(id);
    }

    private void LoadScene(string id)
    {
        currentSceneID = id;
        currentSceneIndex = Array.IndexOf(ALL_SCENE_IDS, id);
        OnSceneLoaded?.Invoke(id);

        Debug.Log($"LOAD - {id}");
    }

    private void UnloadScene(string id)
    {
        OnSceneUnloaded?.Invoke(id);
        Debug.Log($"UNLOAD - {id}");
    }

    //DEBUG CONTROLS
#if UNITY_EDITOR
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.RightArrow))
        {
            GoToScene(ALL_SCENE_IDS[Mathf.Clamp(currentSceneIndex + 1, 0, ALL_SCENE_IDS.Length)]);
        }
        if (Input.GetKeyDown(KeyCode.LeftArrow))
        {
            GoToScene(ALL_SCENE_IDS[Mathf.Clamp(currentSceneIndex - 1, 0, ALL_SCENE_IDS.Length)]);
        }
    }
#endif
}
