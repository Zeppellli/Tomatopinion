using System.Linq;
using UnityEngine;

public class LoadableObject : MonoBehaviour
{
    [SerializeField] private string[] listeningToIDs;

    private void Awake()
    {
        GameSceneManager.OnSceneLoaded += LoadSelf;
        GameSceneManager.OnSceneUnloaded += UnloadSelf;
    }
    private void OnDestroy()
    {
        GameSceneManager.OnSceneLoaded -= LoadSelf;
        GameSceneManager.OnSceneUnloaded -= UnloadSelf;
    }

    private void LoadSelf(string id)
    {
        if (!listeningToIDs.Contains(id)) { return; }

        gameObject.SetActive(true);
    }
    
    private void UnloadSelf(string id)
    {
        gameObject.SetActive(false);
    }
}
