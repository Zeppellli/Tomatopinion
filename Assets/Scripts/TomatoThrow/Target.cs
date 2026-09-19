using UnityEngine;
using UnityEngine.SceneManagement;

public class Target : MonoBehaviour
{
    private SpriteRenderer sprite;
    [SerializeField] private string targetScene;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        sprite = GetComponentInChildren<SpriteRenderer>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void OnTriggerEnter2D(Collider2D collider)
    {
        print("yo");
        SceneManager.LoadScene(targetScene);
    }
}
