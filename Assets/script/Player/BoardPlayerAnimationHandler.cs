using UnityEngine;

public class BoardPlayerAnimationHandler : MonoBehaviour
{
    [SerializeField]
    private Animator animator;
    [SerializeField]
    private SpriteRenderer spriteRenderer;
    private PlayerMovement pMovemnt;

    private Skin skin;

    private bool oldMoving = false;


    private void Start()
    {
        UpdateSkin();

        pMovemnt = GetComponent<PlayerMovement>();

        oldMoving = pMovemnt.isMoving;
        GameManager.Instance.PlayerStats.OnSkinChange += UpdateSkin;
    }

    private void UpdateSkin()
    {
        SkinType currentSkin = GameManager.Instance.PlayerStats.CurrentSkin;

        skin = SkinManager.Instance.GetSkin(currentSkin);

        spriteRenderer.sprite = skin.SkinSprite;
        animator.runtimeAnimatorController = skin.AnimatorController;
    }

    private void Update()
    {
        if (oldMoving == pMovemnt.isMoving)
            return;
        
        animator.SetBool("Walking", pMovemnt.isMoving);
        oldMoving = pMovemnt.isMoving;
    }

    private void OnDestroy()
    {
        if (GameManager.Instance != null && GameManager.Instance.PlayerStats != null)
        {
            GameManager.Instance.PlayerStats.OnSkinChange -= UpdateSkin;
        }
    }

}
