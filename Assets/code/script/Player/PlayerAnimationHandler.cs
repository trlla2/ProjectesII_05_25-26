using UnityEngine;

public class PlayerAnimationHandler : MonoBehaviour
{
    private Animator animator;

    private void Start()
    {
        animator = GetComponent<Animator>();

        G.GM.CombatHandler.OnPlayerHitRecived += HitAnimation;
    }

    private void HitAnimation()
    {
        animator.SetTrigger("Hit");
    }

    private void OnDestroy()
    {
        G.GM.CombatHandler.OnPlayerHitRecived -= HitAnimation;
    }
}
