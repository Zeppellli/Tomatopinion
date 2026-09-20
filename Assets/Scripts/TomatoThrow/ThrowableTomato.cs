using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UIElements;
using UnityEngine.WSA;

public class ThrowableTomato : MonoBehaviour
{
    private Vector2 mouseVelocity;
    private Vector2 lastMousePosition;
    private Vector3 initialPosition;
    private Vector3 initialScale;
    private Vector3 lastPosition;
    private Vector3 targetPositionWithoutLob;
    private Vector3 lobOffset;
    [SerializeField] private float upwardsVelocity;
    private float initialUpwardsVelocity;
    [SerializeField] private float gravity;
    private Vector2 targetPosition;
    [SerializeField] private float contactMargin = 0.01f;
    [SerializeField] private float scaleOnLanding = 0.3f;
    private Vector3 targetAbsoluteScale;
    [SerializeField] private GameObject Splash;
    [SerializeField] private bool moveable;
    private bool isThrown = false;
    private bool hasLanded = false;
    private bool isPrepared = false;
    [SerializeField] private float minimumThrowVelocity = 100;
    [SerializeField] private Transform targetHeight;
    [SerializeField] private SpriteRenderer tomatoSprite;
    [SerializeField] private float tomatoSpeed;
    [SerializeField] private float rotationPerFrame;
    private float rotation;
    [SerializeField] private float frameRate = 12;
    private float spinCounter = 0;
    [SerializeField] private float respawnTime = 1;
    [SerializeField] private float preparedHeight = 5;
    private float respawnCounter = 0;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        setInitialValues();
    }

    // Update is called once per frame
    void Update()
    {
        if (isThrown)
        {
            sendTomatoToTarget();
        }
        else {
            CalculateMouseVelocity();
            PrepareTomatoForThrow();
            if (moveable && !isPrepared) MoveTomatoTowardsMouse();
            if (HasThrowVelocity() && isPrepared && !checkIfThrown())
            {
                ThrowTomato();
            }

        }

        if (transform.position.y == targetPosition.y && lastPosition == transform.position)

        {
            showSplash(true);
            hasLanded = true;

        }
        if (hasLanded) {
            respawnCounter += Time.deltaTime;
            if (respawnCounter >= respawnTime) ResetTomato();
        }

        SpinTomato();
        lastPosition = transform.position;
    }

    void MoveTomatoTowardsMouse()
    {
        Vector2 newPosition = transform.position;
        Vector2 mousePosition = Mouse.current.position.ReadValue();
        mousePosition = Camera.main.ScreenToWorldPoint(mousePosition);
        newPosition.x = mousePosition.x;
        transform.position = newPosition;
    }

    public bool HasThrowVelocity()
    {
        return (mouseVelocity.magnitude > minimumThrowVelocity && mouseVelocity.y > 0);
    }

    public void ThrowTomato()
    {
        isThrown = true;

        Vector3 newPosition = transform.position;
        newPosition.y = initialPosition.y;
        transform.position = newPosition;

        targetPositionWithoutLob = transform.position;
        float yValueRaycast = targetHeight.position.y - transform.position.y;
        float xValueRaycast = (yValueRaycast / mouseVelocity.y) * mouseVelocity.x;
        Vector2 raycast = new Vector2(xValueRaycast, yValueRaycast);
        targetPosition = new Vector2(transform.position.x + xValueRaycast, transform.position.y + yValueRaycast);

    }
    void sendTomatoToTarget()
    {
        CalculateLobOffset();
        targetPositionWithoutLob = Vector2.Lerp(targetPositionWithoutLob, targetPosition, tomatoSpeed * Time.deltaTime);
        transform.localScale = Vector2.Lerp(transform.localScale, targetAbsoluteScale, tomatoSpeed * Time.deltaTime);

        transform.position = targetPositionWithoutLob + lobOffset;

        if (targetPosition.y - targetPositionWithoutLob.y < contactMargin) {
            transform.position = targetPosition;
            transform.localScale = targetAbsoluteScale;
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

    void showSplash(bool shouldShow) { 
        Splash.SetActive(shouldShow);
        tomatoSprite.enabled = !shouldShow;
    }

    void setInitialValues()
    { 
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
        if (isThrown) { 
        
            spinCounter += Time.deltaTime;
            if (spinCounter >= 1 / frameRate) {
                rotation += rotationPerFrame;
                tomatoSprite.transform.rotation = Quaternion.Euler(0f,0f,rotation);
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
        else {
            preparedPosition.y = initialPosition.y;
        }

        transform.position = Vector3.MoveTowards(transform.position, preparedPosition, 0.5f * Vector3.Distance(transform.position, preparedPosition));

    }
}
