using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class UI_ThrowableTomato : MonoBehaviour
{
    private enum State { Idle, Held, Flying, Shrinking, Respawning }

    [Header("References")]
    [SerializeField] private Image tomatoImage;
    [SerializeField] private GameObject Splash;
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip throwClip;
    [SerializeField] private AudioClip[] allSplashClips;

    [Header("Grab")]
    [SerializeField] private bool requireGrab = true;
    [SerializeField] private float grabRadius = 150f;
    [SerializeField] private float returnSharpness = 12f;

    [Header("Swipe")]
    [SerializeField] private float swipeWindow = 0.1f;
    [SerializeField] private float minimumThrowVelocity = 60f;
    [SerializeField] private float minUpwardDirection = 0.2f;

    [Header("Flight")]
    [SerializeField] private float distancePerSpeed = 0.35f;
    [SerializeField] private float minThrowDistance = 150f;
    [SerializeField] private float maxThrowDistance = 900f;
    [SerializeField] private float flightSpeed = 1200f;
    [SerializeField] private float arcHeightPerDistance = 0.2f;
    [SerializeField] private float scaleOnLanding = 0.3f;

    [Header("Landing")]
    [SerializeField] private float shrinkDuration = 0.35f;
    [SerializeField] private float respawnTime = 2f;

    [Header("Spin")]
    [SerializeField] private float rotationPerFrame;
    [SerializeField] private float frameRate = 12f;

    private RectTransform rect;
    private RectTransform parentRect;
    private Camera uiCamera;

    private readonly List<(Vector2 pos, float time)> samples = new List<(Vector2, float)>();
    private Vector2 initialPosition;
    private Vector3 initialScale;
    private float rotation;
    private float spinCounter;


    private Vector2 mouseVelocity;
    private Vector2 lastMousePosition;
    private Vector3 lastPosition;
    private Vector3 targetPositionWithoutLob;
    private Vector3 lobOffset;
    [SerializeField] private float upwardsVelocity;
    private float initialUpwardsVelocity;
    [SerializeField] private float gravity;
    private Vector2 targetPosition;
    [SerializeField] private float contactMargin = 0.01f;
    private Vector3 targetAbsoluteScale;
    [SerializeField] private bool moveable;
    private bool isThrown = false;
    private bool hasLanded = false;
    private bool isPrepared = false;
    [SerializeField] private Transform targetHeight;
    private float targetHeightY;
    [SerializeField] private Image tomatoSprite;
    [SerializeField] private float tomatoSpeed;
    [SerializeField] private float preparedHeight = 5;
    private float respawnCounter = 0;
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
        setInitialValues();
    }

    void FixedUpdate()
    {
        if (isThrown)
        {
            sendTomatoToTarget();
        }
        else
        {
            CalculateMouseVelocity();
            PrepareTomatoForThrow();
            if (moveable && isPrepared) MoveTomatoTowardsMouse();
            if (HasThrowVelocity() && isPrepared && !checkIfThrown())
            {
                ThrowTomato();
            }

        }

        if (transform.position.y == targetPosition.y && lastPosition == transform.position)

        {
            Land();

        }
        if (isThrown)
        {
            respawnCounter += Time.deltaTime;
            if (respawnCounter >= respawnTime && !hasLanded) ResetTomato();
        }

        SpinTomato();
        lastPosition = transform.position;
    }

    void MoveTomatoTowardsMouse()
    {
        Vector2 newPosition = transform.position;
        Vector2 mousePosition = Mouse.current.position.ReadValue();
        mousePosition.y = Mathf.Clamp(mousePosition.y, initialPosition.y, targetHeightY * 0.75f);
        //mousePosition = Camera.main.ScreenToWorldPoint(mousePosition);
        newPosition = mousePosition;
        transform.position = Vector3.MoveTowards(transform.position, newPosition, 0.5f * Vector3.Distance(transform.position, newPosition));
    }

    public bool HasThrowVelocity()
    {
        return (mouseVelocity.magnitude > minimumThrowVelocity && mouseVelocity.y > 50f);
    }

    public void ThrowTomato()
    {
        isThrown = true;

        Vector3 newPosition = transform.position;
        //newPosition.y = initialPosition.y;
        transform.position = newPosition;

        targetPositionWithoutLob = transform.position;
        float yValueRaycast = targetHeightY - transform.position.y;
        float xValueRaycast = (yValueRaycast / mouseVelocity.y) * mouseVelocity.x;
        Vector2 raycast = new Vector2(xValueRaycast, yValueRaycast);
        targetPosition = new Vector2(transform.position.x + xValueRaycast, transform.position.y + yValueRaycast);

        audioSource.PlayOneShot(throwClip);
    }
    void sendTomatoToTarget()
    {
        if (hasLanded) return;
        CalculateLobOffset();
        targetPositionWithoutLob = Vector2.Lerp(targetPositionWithoutLob, targetPosition, tomatoSpeed * Time.deltaTime);
        transform.localScale = Vector2.Lerp(transform.localScale, Vector2.zero, (1 / shrinkDuration) * Time.deltaTime);

        transform.position = targetPositionWithoutLob + lobOffset;

        if (targetPosition.y - targetPositionWithoutLob.y < contactMargin)
        {
            Land();
        }
    }

    void CalculateMouseVelocity()
    {
        Vector2 mousePosition = Mouse.current.position.ReadValue();
        mousePosition = Camera.main.ScreenToWorldPoint(mousePosition);
        Vector2 distanceTraveled = mousePosition - lastMousePosition;
        mouseVelocity = distanceTraveled / Time.deltaTime;
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

    void showSplash(bool shouldShow)
    {
        Splash.SetActive(shouldShow);
        tomatoSprite.enabled = !shouldShow;
        if (shouldShow)
        {
            audioSource.PlayOneShot(allSplashClips[Random.Range(0, allSplashClips.Length)]);
        }
    }

    void setInitialValues()
    {
        targetHeightY = targetHeight.transform.position.y;
        initialPosition = transform.position;
        initialScale = transform.localScale;
        initialUpwardsVelocity = upwardsVelocity;
        targetPositionWithoutLob = transform.position;
        targetAbsoluteScale = transform.localScale * scaleOnLanding;
        Splash.SetActive(false);
    }
    void ResetTomato()
    {
        transform.position = initialPosition;
        tomatoSprite.transform.rotation = Quaternion.identity;
        transform.localScale = initialScale;
        targetPositionWithoutLob = transform.position;
        upwardsVelocity = initialUpwardsVelocity;
        lobOffset = Vector2.zero;
        respawnCounter = 0f;
        showSplash(false);
        isThrown = false;
        hasLanded = false;
        isPrepared = false;
    }

    void SpinTomato()
    {
        if (isThrown)
        {

            spinCounter += Time.deltaTime;
            if (spinCounter >= 1 / frameRate)
            {
                rotation += rotationPerFrame;
                tomatoSprite.transform.rotation = Quaternion.Euler(0f, 0f, rotation);
                spinCounter = 0f;
            }
        }
    }

    void PrepareTomatoForThrow()
    {
        Vector3 preparedPosition = transform.position;
        if (isPrepared = Mouse.current.leftButton.isPressed && !isThrown)
        {
            preparedPosition.y = initialPosition.y + preparedHeight;
        }
        else
        {
            preparedPosition.y = initialPosition.y;
        }

        transform.position = Vector3.MoveTowards(transform.position, preparedPosition, 0.5f * Vector3.Distance(transform.position, preparedPosition));

    }

    UI_Target GetTargetUnderTomato()
    {
        Vector2 screenPoint = RectTransformUtility.WorldToScreenPoint(uiCamera, rect.position);
        foreach (UI_Target target in FindObjectsByType<UI_Target>(FindObjectsSortMode.None))
        {
            if (target.ContainsScreenPoint(screenPoint, uiCamera)) return target;
        }
        return null;
    }

    void Land()
    {
        UI_Target hit = GetTargetUnderTomato();

        if (hit != null)
        {
            showSplash(true);
            hit.Hit();
            hasLanded = true;
        }
    }
}

