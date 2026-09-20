using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class UI_ThrowableTomato : MonoBehaviour
{
    private enum State { Idle, Held, Flying, Shrinking, Respawning }

    [Header("References")]
    [SerializeField] private Image tomatoImage;
    [SerializeField] private GameObject Splash;

    [Header("Grab")]
    [SerializeField] private bool requireGrab = true;
    [SerializeField] private float grabRadius = 150f;
    [SerializeField] private float returnSharpness = 12f;

    [Header("Swipe")]
    [SerializeField] private float swipeWindow = 0.1f;
    [SerializeField] private float minimumThrowVelocity = 800f;
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
    [SerializeField] private float respawnTime = 1f;

    [Header("Spin")]
    [SerializeField] private float rotationPerFrame;
    [SerializeField] private float frameRate = 12f;

    private RectTransform rect;
    private RectTransform parentRect;
    private Camera uiCamera;

    private State state = State.Idle;
    private readonly List<(Vector2 pos, float time)> samples = new List<(Vector2, float)>();
    private Vector2 grabOffset;
    private Vector2 initialPosition;
    private Vector3 initialScale;
    private Vector3 landingScale;
    private Vector2 flightStart;
    private Vector2 flightEnd;
    private float flightDuration;
    private float flightTimer;
    private float arcHeight;
    private float stateTimer;
    private float rotation;
    private float spinCounter;

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
        initialPosition = Pos;
        initialScale = rect.localScale;
        landingScale = initialScale * scaleOnLanding;
        Splash.SetActive(false);
    }

    void Update()
    {
        switch (state)
        {
            case State.Idle: UpdateIdle(); break;
            case State.Held: UpdateHeld(); break;
            case State.Flying: UpdateFlying(); break;
            case State.Shrinking: UpdateShrinking(); break;
            case State.Respawning: UpdateRespawning(); break;
        }

        SpinTomato();
    }

    Vector2 ScreenToLocal(Vector2 screenPosition)
    {
        RectTransformUtility.ScreenPointToLocalPointInRectangle(parentRect, screenPosition, uiCamera, out Vector2 local);
        return local;
    }

    void UpdateIdle()
    {
        Pos = Vector2.Lerp(Pos, initialPosition, 1f - Mathf.Exp(-returnSharpness * Time.deltaTime));

        Pointer pointer = Pointer.current;
        if (pointer == null || !pointer.press.wasPressedThisFrame) return;

        Vector2 screenPosition = pointer.position.ReadValue();

        if (requireGrab)
        {
            Vector2 tomatoScreen = RectTransformUtility.WorldToScreenPoint(uiCamera, rect.position);
            if (Vector2.Distance(screenPosition, tomatoScreen) > grabRadius) return;
        }

        Vector2 local = ScreenToLocal(screenPosition);
        grabOffset = requireGrab ? Pos - local : Vector2.zero;
        samples.Clear();
        AddSample(local);
        state = State.Held;
    }

    void UpdateHeld()
    {
        Pointer pointer = Pointer.current;
        if (pointer == null)
        {
            state = State.Idle;
            return;
        }

        Vector2 local = ScreenToLocal(pointer.position.ReadValue());
        AddSample(local);
        Pos = local + grabOffset;

        if (!pointer.press.isPressed) Release();
    }

    void AddSample(Vector2 local)
    {
        samples.Add((local, Time.time));
        while (samples.Count > 1 && Time.time - samples[0].time > swipeWindow)
        {
            samples.RemoveAt(0);
        }
    }

    Vector2 GetSwipeVelocity()
    {
        if (samples.Count < 2) return Vector2.zero;

        var first = samples[0];
        var last = samples[samples.Count - 1];
        float elapsed = last.time - first.time;
        return elapsed > 0f ? (last.pos - first.pos) / elapsed : Vector2.zero;
    }

    void Release()
    {
        Vector2 velocity = GetSwipeVelocity();
        float speed = velocity.magnitude;

        if (speed >= minimumThrowVelocity && velocity.y / speed >= minUpwardDirection)
        {
            Throw(velocity / speed, speed);
        }
        else
        {
            state = State.Idle;
        }
    }

    void Throw(Vector2 direction, float speed)
    {
        float distance = Mathf.Clamp(speed * distancePerSpeed, minThrowDistance, maxThrowDistance);

        flightStart = Pos;
        flightEnd = flightStart + direction * distance;
        flightDuration = Mathf.Max(0.05f, distance / flightSpeed);
        arcHeight = distance * arcHeightPerDistance;
        flightTimer = 0f;
        state = State.Flying;
    }

    void UpdateFlying()
    {
        flightTimer += Time.deltaTime;
        float t = Mathf.Clamp01(flightTimer / flightDuration);

        Vector2 position = Vector2.Lerp(flightStart, flightEnd, t);
        position.y += arcHeight * 4f * t * (1f - t);
        Pos = position;
        rect.localScale = Vector3.Lerp(initialScale, landingScale, t);

        if (t >= 1f) Land();
    }

    void Land()
    {
        stateTimer = 0f;
        UI_Target hit = GetTargetUnderTomato();

        if (hit != null)
        {
            ShowSplash(true);
            state = State.Respawning;
            hit.Hit();
        }
        else
        {
            state = State.Shrinking;
        }
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

    void UpdateShrinking()
    {
        stateTimer += Time.deltaTime;
        float t = Mathf.Clamp01(stateTimer / Mathf.Max(0.01f, shrinkDuration));
        rect.localScale = Vector3.Lerp(landingScale, Vector3.zero, t);

        if (t >= 1f)
        {
            tomatoImage.enabled = false;
            stateTimer = 0f;
            state = State.Respawning;
        }
    }

    void UpdateRespawning()
    {
        stateTimer += Time.deltaTime;
        if (stateTimer >= respawnTime) ResetTomato();
    }

    void ResetTomato()
    {
        Pos = initialPosition;
        rect.localScale = initialScale;
        tomatoImage.rectTransform.localRotation = Quaternion.identity;
        rotation = 0f;
        spinCounter = 0f;
        stateTimer = 0f;
        samples.Clear();
        ShowSplash(false);
        state = State.Idle;
    }

    void ShowSplash(bool shouldShow)
    {
        Splash.SetActive(shouldShow);
        tomatoImage.enabled = !shouldShow;
    }

    void SpinTomato()
    {
        if (state != State.Flying) return;

        spinCounter += Time.deltaTime;
        if (spinCounter >= 1f / frameRate)
        {
            rotation += rotationPerFrame;
            tomatoImage.rectTransform.localRotation = Quaternion.Euler(0f, 0f, rotation);
            spinCounter = 0f;
        }
    }
}