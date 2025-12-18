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

    private void HandlePlayerHit()
    {

        hitSource.Play();

        if (GameManager.Instance.GetPlayerHP() <= 0)
            deathSource.Play();
    }
        
    private void OnDestroy()
    {
        GameManager.Instance.OnPlayerHitRecived -= HandlePlayerHit;
    }
}
