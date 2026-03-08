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
        GameManager.Instance.CombatHandler.OnPlayerHitRecived += HandlePlayerHit;
    }

    private void HandlePlayerHit()
    {
        killParticles.Play();

        if (GameManager.Instance.PlayerStats.Hp <= 0)
            spriteRenderer.enabled = false;
    }

    private void OnDestroy()
    {
        GameManager.Instance.CombatHandler.OnPlayerHitRecived -= HandlePlayerHit;
    }
}
