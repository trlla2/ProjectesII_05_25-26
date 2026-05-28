using UnityEngine;

public class PlayerAnimationHandler : MonoBehaviour
{
    private Animator animator;
    private SpriteRenderer spriteRenderer;
    private PlayerPathing pathing;

    private Skin skin;

    private void Start()
    {
        animator = GetComponent<Animator>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        pathing = GetComponent<PlayerPathing>();

        GameManager.Instance.CombatHandler.OnPlayerHitRecived += HitAnimation;
        GameManager.Instance.CombatHandler.OnPlayerDead += DeadAnimation;

        pathing.OnSpellCast += AttackAnimation;

        SkinType currentSkin = GameManager.Instance.PlayerStats.CurrentSkin;
        skin = SkinManager.Instance.GetSkin(currentSkin);

        spriteRenderer.sprite = skin.SkinSprite;
        animator.runtimeAnimatorController = skin.AnimatorController;
    }

    private void HitAnimation()
    {
        animator.SetTrigger("Hit");
    }

    private void DeadAnimation() { animator.SetBool("Dead", true); }
    private void AttackAnimation() { animator.SetTrigger("Attack"); }

    private void OnDestroy()
    {
        if (GameManager.Instance != null && GameManager.Instance.CombatHandler != null)
        {
            GameManager.Instance.CombatHandler.OnPlayerHitRecived -= HitAnimation;
            GameManager.Instance.CombatHandler.OnPlayerDead -= DeadAnimation;
        }
    }
}
