using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UIElements;

public class ThrowableTomato : MonoBehaviour
{
    private Vector2 mouseVelocity;
    private Vector2 lastMousePosition;
    private Vector3 lastPosition;
    private Vector3 targetPositionWithoutLob;
    private Vector3 lobOffset;
    [SerializeField] private float upwardsVelocity;
    [SerializeField] private float gravity;
    private Vector2 targetPosition;
    [SerializeField] private float contactMargin = 0.01f;
    [SerializeField] private float scaleOnLanding = 0.3f;
    private Vector3 targetAbsoluteScale;
    [SerializeField] private GameObject Splash;
    [SerializeField] private bool moveable;
    private bool isThrown = false;
    [SerializeField] private float minimumThrowVelocity = 100;
    [SerializeField] private Transform targetHeight;
    [SerializeField] private float tomatoSpeed;
    [SerializeField] private float rotationPerFrame;
    [SerializeField] private float frameRate = 12;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        targetPositionWithoutLob = transform.position;
        targetAbsoluteScale = transform.localScale * scaleOnLanding;
        Splash.SetActive(false);
    }

    // Update is called once per frame
    void Update()
    {
        if (isThrown)
        {
            sendTomatoToTarget();
        }
        else{        
            CalculateMouseVelocity();
            if(moveable) MoveTomatoTowardsMouse();
            if (HasThrowVelocity() && Mouse.current.leftButton.isPressed && !checkIfThrown())
            {
                ThrowTomato();
            }

        }

        if (transform.position.y == targetPosition.y && lastPosition == transform.position)

        {
            Splash.SetActive(true);

        }

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
        return (mouseVelocity.magnitude > minimumThrowVelocity && mouseVelocity.y  > 0);
    }

    public void ThrowTomato()
    {
        isThrown = true;
        targetPositionWithoutLob = transform.position;
        float yValueRaycast = targetHeight.position.y - transform.position.y;
        float xValueRaycast = (yValueRaycast / mouseVelocity.y) * mouseVelocity.x;
        Vector2 raycast = new Vector2(xValueRaycast, yValueRaycast);
        targetPosition = new Vector2(transform.position.x + xValueRaycast, transform.position.y + yValueRaycast);
        print(targetPosition);

    }
    void sendTomatoToTarget()
    {
        CalculateLobOffset();
        targetPositionWithoutLob = Vector2.Lerp(targetPositionWithoutLob, targetPosition, tomatoSpeed * Time.deltaTime);
        transform.localScale = Vector2.Lerp(transform.localScale, targetAbsoluteScale, tomatoSpeed * Time.deltaTime);

        transform.position = targetPositionWithoutLob + lobOffset; 

        if (targetPosition.y - targetPositionWithoutLob.y  < contactMargin) {
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
}
