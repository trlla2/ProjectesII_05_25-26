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

        if (behaviour == null) return;

        behaviour.OnHitRecived += HitAnimation;
        behaviour.OnEnemyDeath += () => animator.SetBool("Dead", true);

        if (behaviour.ActiveSpell != null)
            behaviour.ActiveSpell.OnExecute += () => animator.SetTrigger("Attack");
    }

    private void HitAnimation(float hp)
    {
        animator.SetTrigger("Hit");
    }

    private void OnDestroy()
    {
        if (behaviour == null) return;

        behaviour.OnHitRecived -= HitAnimation;
        behaviour.OnEnemyDeath -= () => animator.SetBool("Dead", true);

        if (behaviour.ActiveSpell != null)
            behaviour.ActiveSpell.OnExecute -= () => animator.SetTrigger("Attack");
    }
}
