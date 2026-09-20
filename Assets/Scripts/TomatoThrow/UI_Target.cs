using UnityEngine;

public class UI_Target : MonoBehaviour
{
    [SerializeField] private string targetScene;
    [SerializeField] private RectTransform hitArea;

    [Header("Case by Case bullshit")]
    [SerializeField] private bool activatesHats;
    [SerializeField] private bool activatesBowties;

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
        AdditionalEffects();

        GameSceneManager.Instance.GoToScene(targetScene);
    }
    
    private void AdditionalEffects()
    {
        LoadableObject.loadHats = activatesHats;
        LoadableObject.loadBowties = activatesBowties;
    }
}