using UnityEngine;

public class PlaySoundOnEnable : MonoBehaviour
{
    [SerializeField] private AudioClip clip;

    public void PlaySound()
    {
        SoundManager.Instance.StartAmbiance(clip);
    }
}
