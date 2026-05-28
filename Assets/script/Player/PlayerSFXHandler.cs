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
        GameManager.Instance.CombatHandler.OnPlayerHitRecived += HandlePlayerHit;
    }

    private void HandlePlayerHit()
    {

        hitSource.Play();

        if (GameManager.Instance.PlayerStats.Hp <= 0)
            deathSource.Play();
    }
        
    private void OnDestroy()
    {
        if (GameManager.Instance != null && GameManager.Instance.CombatHandler != null)
            GameManager.Instance.CombatHandler.OnPlayerHitRecived -= HandlePlayerHit;
    }
}
