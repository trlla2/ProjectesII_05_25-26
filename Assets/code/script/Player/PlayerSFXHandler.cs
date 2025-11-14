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
        PlayerStats.Instance.OnHitRecived += HandlePlayerHit;
    }

    private void HandlePlayerHit(float health)
    {

        hitSource.Play();

        if (health <= 0)
            deathSource.Play();
    }
        
    private void OnDestroy()
    {
        PlayerStats.Instance.OnHitRecived -= HandlePlayerHit;
    }
}
