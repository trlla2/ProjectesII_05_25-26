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
        GameManager.Instance.OnPlayerHitRecived += HandlePlayerHit;
    }

    private void HandlePlayerHit()
    {
        killParticles.Play();

        if (GameManager.Instance.GetPlayerHP() <= 0)
            spriteRenderer.enabled = false;
    }

    private void OnDestroy()
    {
        GameManager.Instance.OnPlayerHitRecived -= HandlePlayerHit;
    }
}
