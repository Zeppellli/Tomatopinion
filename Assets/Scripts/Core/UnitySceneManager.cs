using Mali.Utils;
using UnityEngine;
using UnityEngine.SceneManagement;

public class UnitySceneManager : Singleton<UnitySceneManager>
{
    public enum UnityScenes
    {
        Start,
        Game,
        End
    }
    private UnityScenes currentUnityScene = UnityScenes.Start;

    private void Awake()
    {
        base.Awake();
        LoadableObject.loadHats = false;
        LoadableObject.loadBowties = false;
    }

    public void LoadScene(UnityScenes sceneToLoad, bool forceLoad = false)
    {
        if (!forceLoad && sceneToLoad == currentUnityScene) { return; }

        string unitySceneName = "";

        switch (sceneToLoad)
        {
            case UnityScenes.Start:
                unitySceneName = "StartMenu";
                break;

            case UnityScenes.Game:
                unitySceneName = "Game";
                break;

            case UnityScenes.End:
                unitySceneName = "EndingScreen";
                break;
        }

        SceneManager.LoadScene(unitySceneName);
        currentUnityScene = sceneToLoad;
    }

    //DEBUG CONTROLS
#if UNITY_EDITOR
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.RightShift))
        {
            LoadScene(UnityScenes.Game);
        }
    }
#endif
}
