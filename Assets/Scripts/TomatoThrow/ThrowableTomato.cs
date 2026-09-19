using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;

public class ThrowableTomato : MonoBehaviour
{
    private Vector2 mouseVelocity;
    private Vector2 lastMousePosition;
    private bool isThrown = false;
    public float minimumThrowVelocity = 100;
    public Transform targetPositionTransform;
    public float tomatoSpeed;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (isThrown)
        {
            sendTomatoToTarget();
        }
        else { 
            if (HasThrowVelocity() && Mouse.current.leftButton.isPressed)
            {
                isThrown = true;
            }
            else { 
                CalculateMouseVelocity();
                MoveTomatoTowardsMouse();
            }
        }
        
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

    }
    void sendTomatoToTarget()
    {
       
        transform.position = Vector2.MoveTowards(transform.position, targetPositionTransform.position, tomatoSpeed * Time.deltaTime);
    }

    void CalculateMouseVelocity()
    {
        Vector2 mousePosition = Mouse.current.position.ReadValue();
        mousePosition = Camera.main.ScreenToWorldPoint(mousePosition);
        Vector2 distanceTraveled = mousePosition - lastMousePosition;
        mouseVelocity = distanceTraveled / Time.deltaTime;
        lastMousePosition = mousePosition;
        if (mouseVelocity.magnitude > 0) print(mouseVelocity);
    }

}
