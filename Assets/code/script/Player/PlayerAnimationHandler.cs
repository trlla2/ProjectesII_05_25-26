using UnityEditor.Animations;
using UnityEngine;

public class PlayerAnimationHandler : MonoBehaviour
{
    private Animator animator;

    private void Start()
    {
        animator = GetComponent<Animator>();

        GameManager.Instance.OnPlayerHitRecived += HitAnimation;
    }

    private void HitAnimation(float hp)
    {
        animator.SetTrigger("Hit");
    }

    private void OnDestroy()
    {
        GameManager.Instance.OnPlayerHitRecived -= HitAnimation;
    }
}
