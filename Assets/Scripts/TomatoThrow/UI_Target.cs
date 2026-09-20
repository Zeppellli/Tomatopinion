using UnityEngine;

public class UI_Target : MonoBehaviour
{
    [SerializeField] private string targetScene;
    [SerializeField] private RectTransform hitArea;

    [Header("Case by Case bullshit")]
    [SerializeField] private bool isStartButton;
    [SerializeField] private bool isRestartButton;
    [SerializeField] private bool activatesHats;
    [SerializeField] private bool activatesBowties;
    [SerializeField] private bool doesNothing;

    void Awake()
    {
        if (hitArea == null) hitArea = (RectTransform)transform;
    }

    public bool ContainsScreenPoint(Vector2 screenPoint, Camera uiCamera)
    {
        return RectTransformUtility.RectangleContainsScreenPoint(hitArea, screenPoint, uiCamera);
    }

    public void Hit()
    {
        if (doesNothing) { return; }

        if (isStartButton)
        {
            UnitySceneManager.Instance.LoadScene(UnitySceneManager.UnityScenes.Game);
            return;
        }
        if (isRestartButton)
        {
            UnitySceneManager.Instance.LoadScene(UnitySceneManager.UnityScenes.Start);
            return;
        }

        SoundManager.Instance.EarlyLineStop();
        AdditionalEffects();
        GameSceneManager.Instance.GoToScene(targetScene);
    }
    
    private void AdditionalEffects()
    {
        if (!LoadableObject.loadHats) { LoadableObject.loadHats = activatesHats; }
        if (!LoadableObject.loadBowties) { LoadableObject.loadBowties = activatesBowties; }        
    }
}