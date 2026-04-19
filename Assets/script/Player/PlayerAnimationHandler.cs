using UnityEngine;

public class PlayerAnimationHandler : MonoBehaviour
{
    private Animator animator;
    private PlayerPathing pathing;

    private void Start()
    {
        animator = GetComponent<Animator>();
        pathing = GetComponent<PlayerPathing>();

        GameManager.Instance.CombatHandler.OnPlayerHitRecived += HitAnimation;
        GameManager.Instance.CombatHandler.OnPlayerDead += DeadAnimation;

        pathing.OnSpellCast += AttackAnimation;
    }

    private void HitAnimation()
    {
        animator.SetTrigger("Hit");
    }

    private void DeadAnimation() { animator.SetBool("Dead", true); }
    private void AttackAnimation() { animator.SetTrigger("Attack"); }

    private void OnDestroy()
    {
        GameManager.Instance.CombatHandler.OnPlayerHitRecived -= HitAnimation;
        GameManager.Instance.CombatHandler.OnPlayerDead -= DeadAnimation;

    }
}
