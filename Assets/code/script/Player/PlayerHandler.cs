using System.Runtime.CompilerServices;
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
        PlayerStats.Instance.OnHitRecived += HandlePlayerHit;
    }

    private void HandlePlayerHit(float health)
    {
        if(health <= 0)
        {
            killParticles.Play();
            spriteRenderer.enabled = false;
        }
    }

    private void OnDestroy()
    {
        PlayerStats.Instance.OnHitRecived -= HandlePlayerHit;
    }
}
