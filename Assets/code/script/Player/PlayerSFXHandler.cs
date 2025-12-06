using UnityEngine;

public class PlayerSFXHandler : MonoBehaviour
{
    [Header("Audio Sources")]
    [SerializeField]
    private AudioSource hitSource;
    [SerializeField]
    private AudioSource deathSource;


    private void Start()
    {
        GameManager.Instance.OnPlayerHitRecived += HandlePlayerHit;
    }

    private void HandlePlayerHit(float health)
    {

        hitSource.Play();

        if (health <= 0)
            deathSource.Play();
    }
        
    private void OnDestroy()
    {
        GameManager.Instance.OnPlayerHitRecived -= HandlePlayerHit;
    }
}
