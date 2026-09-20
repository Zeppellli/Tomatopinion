using UnityEngine;

public class UI_Target : MonoBehaviour
{
    [SerializeField] private string targetScene;
    [SerializeField] private RectTransform hitArea;

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
        GameSceneManager.Instance.GoToScene(targetScene);
    }
}