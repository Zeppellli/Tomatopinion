using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public ThrowableTomato tomato;
    public TargetSelector targetSelector;
    private Target chosenTarget;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (tomato.HasThrowVelocity() && Mouse.current.leftButton.isPressed && !tomato.checkIfThrown())
        {
            tomato.ThrowTomato();
            targetSelector.StopAiming();
            chosenTarget = targetSelector.GetCurrentTarget();
        }
        if (Mouse.current.rightButton.isPressed)
        {
            SceneManager.LoadScene("TestLevel");
        }
    }
}
