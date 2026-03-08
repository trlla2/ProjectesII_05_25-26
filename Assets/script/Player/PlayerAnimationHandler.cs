using UnityEngine;

public class PlayerAnimationHandler : MonoBehaviour
{
    private Animator animator;

    private void Start()
    {
        animator = GetComponent<Animator>();

        GameManager.Instance.CombatHandler.OnPlayerHitRecived += HitAnimation;
    }

    private void HitAnimation()
    {
        animator.SetTrigger("Hit");
    }

    private void OnDestroy()
    {
        GameManager.Instance.CombatHandler.OnPlayerHitRecived -= HitAnimation;
    }
}
