using UnityEngine;

public class PlaySoundOnEnable : MonoBehaviour
{
    [SerializeField] private AudioClip clip;

    private void OnEnable()
    {
        SoundManager.Instance.StartAmbiance(clip);
    }
}
