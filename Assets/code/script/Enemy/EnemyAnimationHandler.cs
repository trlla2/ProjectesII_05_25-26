using UnityEngine;

[RequireComponent(typeof(EnemyBehaviour))]
public class EnemyAnimationHandler : MonoBehaviour
{
    private Animator animator;
    private EnemyBehaviour behaviour;

    private void Awake()
    {
        behaviour = GetComponent<EnemyBehaviour>();
    }

    private void Start()
    {
        animator = GetComponent<Animator>();

        behaviour.OnHitRecived += HitAnimation;
    }

    private void HitAnimation(float hp)
    {
        animator.SetTrigger("Hit");
    }

    private void OnDestroy()
    {
        behaviour.OnHitRecived -= HitAnimation;
    }
}
