using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class UI_ThrowableTomato : MonoBehaviour
{
    [Header("References")]
    //[SerializeField] private RectTransform targetHeight;
    [SerializeField] private Image tomatoImage;
    [SerializeField] private GameObject Splash;

    [Header("Throw")]
    [SerializeField] private float distancePerSpeed = .3f;
    [SerializeField] private float minThrowDistance = 100f;
    [SerializeField] private float maxThrowDistance = 800f;
    [SerializeField] private bool moveable;
    [SerializeField] private float minimumThrowVelocity = 1000;
    [SerializeField] private float preparedHeight = 50;
    [SerializeField] private float tomatoSpeed;
    [SerializeField] private float upwardsVelocity;
    [SerializeField] private float gravity;
    [SerializeField] private float contactMargin = 1f;
    [SerializeField] private float scaleOnLanding = 0.3f;

    [Header("Spin / Respawn")]
    [SerializeField] private float rotationPerFrame;
    [SerializeField] private float frameRate = 12;
    [SerializeField] private float respawnTime = 1;

    private RectTransform rect;
    private RectTransform parentRect;
    private Camera uiCamera;

    private Vector2 mouseVelocity;
    private Vector2 lastMousePosition;
    private Vector2 initialPosition;
    private Vector2 targetPosition;
    private Vector2 targetPositionWithoutLob;
    private Vector2 lobOffset;
    private Vector3 initialScale;
    private Vector3 targetAbsoluteScale;
    private float initialUpwardsVelocity;
    private float rotation;
    private float spinCounter;
    private float respawnCounter;
    private bool isThrown;
    private bool hasLanded;
    private bool isPrepared;

    private Vector2 Pos
    {
        get => rect.localPosition;
        set => rect.localPosition = new Vector3(value.x, value.y, rect.localPosition.z);
    }

    void Awake()
    {
        rect = (RectTransform)transform;
        parentRect = (RectTransform)transform.parent;
        Canvas canvas = GetComponentInParent<Canvas>().rootCanvas;
        uiCamera = canvas.renderMode == RenderMode.ScreenSpaceOverlay ? null : canvas.worldCamera;
    }

    void Start()
    {
        SetInitialValues();
    }

    void Update()
    {
        if (isThrown)
        {
            SendTomatoToTarget();
        }
        else
        {
            CalculateMouseVelocity();
            PrepareTomatoForThrow();
            if (moveable && !isPrepared) MoveTomatoTowardsMouse();
            if (isPrepared && HasThrowVelocity()) ThrowTomato();
        }

        if (hasLanded)
        {
            respawnCounter += Time.deltaTime;
            if (respawnCounter >= respawnTime) ResetTomato();
        }

        SpinTomato();
    }

    Vector2 GetMouseLocalPosition()
    {
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            parentRect, Mouse.current.position.ReadValue(), uiCamera, out Vector2 local);
        return local;
    }

    void MoveTomatoTowardsMouse()
    {
        Vector2 p = Pos;
        p.x = GetMouseLocalPosition().x;
        Pos = p;
    }

    public bool HasThrowVelocity()
    {
        return mouseVelocity.magnitude > minimumThrowVelocity && mouseVelocity.y > 0;
    }

    public void ThrowTomato()
    {
        isThrown = true;

        Vector2 p = Pos;
        p.y = initialPosition.y;
        Pos = p;

        targetPositionWithoutLob = p;
        float distance = Mathf.Clamp(mouseVelocity.magnitude * distancePerSpeed, minThrowDistance, maxThrowDistance);
        targetPosition = p + mouseVelocity.normalized * distance;
    }

    void SendTomatoToTarget()
    {
        if (hasLanded) return;

        CalculateLobOffset();
        targetPositionWithoutLob = Vector2.Lerp(targetPositionWithoutLob, targetPosition, tomatoSpeed * Time.deltaTime);
        rect.localScale = Vector3.Lerp(rect.localScale, targetAbsoluteScale, tomatoSpeed * Time.deltaTime);
        Pos = targetPositionWithoutLob + lobOffset;

        if (Vector2.Distance(targetPositionWithoutLob, targetPosition) < contactMargin)
        {
            Land();
        }
    }

    void Land()
    {
        Pos = targetPosition;
        rect.localScale = targetAbsoluteScale;
        hasLanded = true;
        ShowSplash(true);

        Vector2 screenPoint = RectTransformUtility.WorldToScreenPoint(uiCamera, rect.position);
        foreach (UI_Target target in FindObjectsByType<UI_Target>(FindObjectsSortMode.None))
        {
            if (target.ContainsScreenPoint(screenPoint, uiCamera))
            {
                target.Hit();
                break;
            }
        }
    }

    void CalculateMouseVelocity()
    {
        Vector2 mousePosition = GetMouseLocalPosition();
        if (Time.deltaTime > 0f)
            mouseVelocity = (mousePosition - lastMousePosition) / Time.deltaTime;
        lastMousePosition = mousePosition;
    }

    public bool checkIfThrown()
    {
        return isThrown;
    }

    void CalculateLobOffset()
    {
        lobOffset.y += upwardsVelocity * Time.deltaTime;
        upwardsVelocity -= gravity * Time.deltaTime;
    }

    void ShowSplash(bool shouldShow)
    {
        Splash.SetActive(shouldShow);
        tomatoImage.enabled = !shouldShow;
    }

    void SetInitialValues()
    {
        initialPosition = Pos;
        initialScale = rect.localScale;
        initialUpwardsVelocity = upwardsVelocity;
        targetPositionWithoutLob = Pos;
        targetAbsoluteScale = rect.localScale * scaleOnLanding;
        lastMousePosition = GetMouseLocalPosition();
        Splash.SetActive(false);
    }

    void ResetTomato()
    {
        Pos = initialPosition;
        rect.localScale = initialScale;
        tomatoImage.rectTransform.localRotation = Quaternion.identity;
        rotation = 0f;
        targetPositionWithoutLob = initialPosition;
        upwardsVelocity = initialUpwardsVelocity;
        lobOffset = Vector2.zero;
        respawnCounter = 0f;
        ShowSplash(false);
        isThrown = false;
        hasLanded = false;
        isPrepared = false;
    }

    void SpinTomato()
    {
        if (!isThrown) return;

        spinCounter += Time.deltaTime;
        if (spinCounter >= 1f / frameRate)
        {
            rotation += rotationPerFrame;
            tomatoImage.rectTransform.localRotation = Quaternion.Euler(0f, 0f, rotation);
            spinCounter = 0f;
        }
    }

    void PrepareTomatoForThrow()
    {
        isPrepared = Mouse.current.leftButton.isPressed && !isThrown;

        Vector2 p = Pos;
        float goalY = initialPosition.y + (isPrepared ? preparedHeight : 0f);
        p.y = Mathf.Lerp(p.y, goalY, 0.5f);
        Pos = p;
    }
}