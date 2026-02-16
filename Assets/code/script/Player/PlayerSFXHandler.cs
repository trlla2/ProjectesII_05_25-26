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
        G.GM.CombatHandler.OnPlayerHitRecived += HandlePlayerHit;
    }

    private void HandlePlayerHit()
    {

        hitSource.Play();

        if (G.GM.PlayerStats.Hp <= 0)
            deathSource.Play();
    }
        
    private void OnDestroy()
    {
        G.GM.CombatHandler.OnPlayerHitRecived -= HandlePlayerHit;
    }
}
