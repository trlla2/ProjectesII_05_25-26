using UnityEngine;

public class PlayerHandler : MonoBehaviour
{
    [Header("Particles")]
    [SerializeField]
    private ParticleSystem killParticles;
    [Header("SpriteRender")]
    [SerializeField]
    private SpriteRenderer spriteRenderer;
    private void Start()
    {
        G.GM.CombatHandler.OnPlayerHitRecived += HandlePlayerHit;
    }

    private void HandlePlayerHit()
    {
        killParticles.Play();

        if (G.GM.PlayerStats.Hp <= 0)
            spriteRenderer.enabled = false;
    }

    private void OnDestroy()
    {
        G.GM.CombatHandler.OnPlayerHitRecived -= HandlePlayerHit;
    }
}
