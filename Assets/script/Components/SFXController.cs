using UnityEngine;

public class SFXController : MonoBehaviour
{
    public AudioClip[] clips;
    public float minPitch = 0.8f;
    public float maxPitch = 1.2f;

    private AudioSource audioSource;

    private void Awake()
    {
        audioSource = GetComponent<AudioSource>();
    }

    public void PlayRandomSFX()
    {
        if (clips.Length == 0) return;

        int randomIndex = Random.Range(0, clips.Length);
        audioSource.pitch = Random.Range(minPitch, maxPitch);
        audioSource.PlayOneShot(clips[randomIndex]);
    }
}
