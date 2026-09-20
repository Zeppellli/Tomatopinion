using System.Linq;
using UnityEngine;

public class LoadableObject : MonoBehaviour
{
    [SerializeField] private string[] listeningToIDs;

    [SerializeField] private bool isHat;
    [SerializeField] private bool isBowtie;
    [HideInInspector] public static bool loadHats = false;
    [HideInInspector] public static bool loadBowties = false;

    private void Awake()
    {
        loadHats = false;
        loadBowties = false;
        GameSceneManager.OnSceneLoaded += LoadSelf;
        GameSceneManager.OnSceneUnloaded += UnloadSelf;
    }
    private void OnDestroy()
    {
        GameSceneManager.OnSceneLoaded -= LoadSelf;
        GameSceneManager.OnSceneUnloaded -= UnloadSelf;
    }

    private void Start()
    {
        if (isHat) { gameObject.SetActive(loadHats); }
        if (isBowtie) { gameObject.SetActive(loadBowties); }
    }
    void Update()
    {
        Debug.Log($"HAT: {loadHats}, BOWTIE: {loadBowties}");
    }

    private void LoadSelf(string id)
    {
        if (!isHat && !isBowtie)
        {
            if (!listeningToIDs.Contains(id)) { return; }
        }

        gameObject.SetActive(true);
        PlaySoundOnEnable soundComp;
        if (soundComp = gameObject.GetComponent<PlaySoundOnEnable>())
        {
            soundComp.PlaySound();
        }
    }
    
    private void UnloadSelf(string id)
    {
        gameObject.SetActive(false);
    }
}
