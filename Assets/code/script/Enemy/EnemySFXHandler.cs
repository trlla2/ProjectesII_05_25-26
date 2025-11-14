using UnityEngine;

public class EnemySFXHandler : MonoBehaviour
{
    [Header("Setup Behaviour")]
    [SerializeField]
    private EnemyBehaviour enemyBehaviour;
    [Header("Audio Sources")]
    [SerializeField]
    private AudioSource hitSource;
    [SerializeField]
    private AudioSource deathSource;

    private void Start()
    {
        enemyBehaviour.OnHitRecived += HandleEnemyHit;
    }

    private void HandleEnemyHit(float health)
    {
        hitSource.Play();
        if (health <= 0)
            deathSource.Play();
    }

    private void OnDestroy()
    {
        enemyBehaviour.OnHitRecived -= HandleEnemyHit;
    }

}
