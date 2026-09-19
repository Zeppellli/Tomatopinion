using UnityEngine;
using UnityEngine.InputSystem;

public class TargetSelector : MonoBehaviour
{
    private Target[] targets;
    private bool isLockedIn = false;
    private bool shouldAim = true;
    private Target currentTarget;
    public Transform currentTargetTransform;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        targets = GetComponentsInChildren<Target>();
        
    }

    // Update is called once per frame
    void Update()
    {
        if (targets.Length > 0)
        {
            print(shouldAim);
            print(isLockedIn);
            if (!isLockedIn && shouldAim)
            {
                GetNearestTargetToCursor();
            }
        }
        else 
        {
            Debug.Log("No targets in target list");
        }
              
        CheckForClickAndLockIn();       


    }

    void GetNearestTargetToCursor()
    {
        Target nearestTarget = targets[0];
        nearestTarget.ChangeColor(Color.orange);

        Vector2 mousePosition = Mouse.current.position.ReadValue();
        mousePosition = Camera.main.ScreenToWorldPoint(mousePosition);

        foreach (Target target in targets)
        {
            Vector2 targetPosition = target.transform.position;
            float targetDistanceFromCursor = Vector2.Distance(targetPosition, mousePosition);

            Vector2 nearestTargetPosition = nearestTarget.transform.position;
            float nearestTargetDistanceFromCursor = Vector2.Distance(nearestTargetPosition, mousePosition);

            if (targetDistanceFromCursor < nearestTargetDistanceFromCursor)
            {
                nearestTarget = target;
            }
        }
        currentTarget = nearestTarget;
        currentTargetTransform.position = currentTarget.transform.position;
        changeTargetColors();
    }

    void changeTargetColors()
    {
        foreach (Target target in targets)
        {
            target.ChangeColor(Color.white);
        }
        currentTarget.ChangeColor(Color.orange);
    }

    void CheckForClickAndLockIn()
    {
        if (isLockedIn = Mouse.current.leftButton.isPressed)
        {
            currentTarget.ChangeColor(Color.blue);
        }
        else {
            changeTargetColors();
        }
    }

    public Target GetCurrentTarget()
    {
        return currentTarget;
    }

    public void StopAiming()
    {
        shouldAim = false;
    }
}
