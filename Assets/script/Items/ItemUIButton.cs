using System;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using DG.Tweening;

[RequireComponent(typeof(Button))]
public class ItemUIButton : MonoBehaviour
{
    private Item item;
    private Button button;
    private CanvasGroup canvasGroup;
    private ParticleSystem glow;

    [Header("UI")]
    [SerializeField] private Color backgroundActiveColor = Color.white;
    [SerializeField] private Color backgroundCooldownColor = new Color(1f, 0.5f, 0.5f, 1f);
    [SerializeField] private Color backgroundInactiveColor = new Color(0.5f, 0.5f, 0.5f, 1f);
    [SerializeField] private Color inactiveColor = new Color(0.5f, 0.5f, 0.5f, 1f);

    [SerializeField] private Image bg;
    [SerializeField] private Image itemImage;

    [Header("Animation")]
    [SerializeField] private float upwardOffset = 100f;
    [SerializeField] private float scaleMultiplier = 10f;
    [SerializeField] private float shakeAngle = 15f;

    [Header("Animation Timing")]
    [SerializeField] private float moveDuration = 0.4f;
    [SerializeField] private float shakeStepDuration = 0.075f;
    [SerializeField] private float impactDuration = 0.2f;

    [Header("SFX")]
    [SerializeField]
    private AudioSource activateItem;
    PlayerPathing playerRef;

    private static Vector2 centerOffset = new Vector2(900, 2000); //we need it be the ItemUi is in the inventorycanvas  

    private Scene currentScene;

    private void Awake()
    {
        playerRef = FindFirstObjectByType<PlayerPathing>();
        button = GetComponent<Button>();
        canvasGroup = GetComponent<CanvasGroup>();
        glow = GetComponentInChildren<ParticleSystem>();
        currentScene = SceneManager.GetActiveScene();
    }

    private void OnEnable()
    {
        SceneManager.activeSceneChanged += OnActiveSceneChanged;
    }

    private void OnDisable()
    {
        SceneManager.activeSceneChanged -= OnActiveSceneChanged;
    }

    private void OnActiveSceneChanged(Scene previous, Scene next)
    {
        currentScene = next;
    }

    private bool IsInCombatScene()
    {
        return GameManager.Instance != null && GameManager.Instance.CombatHandler != null;
    }

    public void Initialize(Item itemData)
    {
        item = itemData;
        itemImage.sprite = item.Sprite;

        button.onClick.AddListener(OnItemClicked);

        UpdateVisualState();
    }

    private void Update()
    {
        if (item != null && item.IsActivatable)
        {
            UpdateVisualState();
        }
    }

    private void UpdateVisualState()
    {
        if (item == null) return;

        if (!item.IsActivatable || !IsInCombatScene())
        {
            button.interactable = false;
            canvasGroup.alpha = 0.7f;
            //itemImage.color = inactiveColor;
            bg.color = new Color(0,0,0,0);
            return;
        }

        ItemContext context = new ItemContext
        {
            Grid = GameManager.Instance.CombatHandler.GetEnemyRef()?.GetGrid() ?? null,
            Player = playerRef ?? null,
            Enemy = GameManager.Instance.CombatHandler.GetEnemyRef() ?? null
        };

        bool canActivate = item.CanActivate(context);
        button.interactable = canActivate;

        if (item.IsOnCooldown())
        {
            canvasGroup.alpha = 0.5f;
            bg.color = backgroundCooldownColor;
            glow.Stop();
        }
        if (canActivate)
        {
            canvasGroup.alpha = 1f;
            glow.Play();
            bg.color = backgroundActiveColor;
        }
        else
        {
            canvasGroup.alpha = 0.7f;
            glow.Stop();
            glow.Clear();
            bg.color = backgroundInactiveColor;
            itemImage.color = inactiveColor;
        }
    }

    public void OnItemClicked()
    {
        if (item == null || !item.IsActivatable || !IsInCombatScene()) return;

        ItemContext context = new ItemContext
        {
            Grid = GameManager.Instance.CombatHandler.GetEnemyRef().GetGrid(),
            Player = playerRef,
            Enemy = GameManager.Instance.CombatHandler.GetEnemyRef(),
            SpellAnimator = GameManager.Instance.CombatHandler.GetEnemyRef().GetComponentInChildren<SpellPathAnimator>() ?? null
        };

        if (!item.CanActivate(context))
        {
            Debug.Log($"Cannot activate {item.NameItem} right now!");
            return;
        }
        
        activateItem.Play();

        if (item.Effect is GridModifierEffectBase gridModifierEffect)
        {
            // for Grid targeting items, ActivateItem sets ItemProcessingState immediately,
            Vector2 playerPos = FindFirstObjectByType<PlayerHandler>().transform.position;
            gridModifierEffect.SetUIPosition(playerPos);
            gridModifierEffect.BeforeAnimation = (continueActivation) =>
                PlayItemUseAnimation(context, 0.5f, continueActivation);

            ActivateItem(context);
        }
        else
        {
            context.Grid.ChangeState(new ItemProcessingState(context.Grid));
            PlayItemUseAnimation(context, 1f);
        }
    }

    public void UnShowBG() { bg.gameObject.SetActive(false); }

    private void PlayItemUseAnimation(ItemContext context, float timeMultiplier = 1f, Action onComplete = null)
    {
        GameObject clone = Instantiate(gameObject, transform.parent);
        clone.GetComponent<CanvasGroup>().alpha = 1f;
        Destroy(clone.GetComponent<ItemUIButton>());
        Destroy(clone.GetComponent<Button>());
        clone.GetComponentInChildren<Image>().color = Color.white;
        clone.GetComponent<ItemUIButton>().UnShowBG();
        RectTransform cloneRect = clone.GetComponent<RectTransform>();

        

        Vector3 originalPosition = cloneRect.anchoredPosition;
        Vector3 originalScale = cloneRect.localScale;

        Vector2 centerWithOffset = centerOffset + new Vector2(0, upwardOffset);
        Vector2 screenCenter = centerOffset;

        float move   = moveDuration      * timeMultiplier;
        float half   = shakeStepDuration * timeMultiplier / 2f;
        float full   = shakeStepDuration * timeMultiplier;
        float impact = impactDuration    * timeMultiplier;

        Sequence animSequence = DOTween.Sequence();

        //Move to center
        animSequence.Append(cloneRect.DOAnchorPos(centerWithOffset, move).SetEase(Ease.OutCubic));
        animSequence.Join(cloneRect.DOScale(originalScale * scaleMultiplier, move).SetEase(Ease.OutCubic));

        //Shake
        animSequence.Append(cloneRect.DORotate(new Vector3(0, 0,  shakeAngle), half).SetEase(Ease.InOutSine));
        animSequence.Append(cloneRect.DORotate(new Vector3(0, 0, -shakeAngle), full).SetEase(Ease.InOutSine));
        animSequence.Append(cloneRect.DORotate(new Vector3(0, 0,  0),          half).SetEase(Ease.InOutSine));
        animSequence.Append(cloneRect.DORotate(new Vector3(0, 0,  shakeAngle), half).SetEase(Ease.InOutSine));
        animSequence.Append(cloneRect.DORotate(new Vector3(0, 0, -shakeAngle), full).SetEase(Ease.InOutSine));
        animSequence.Append(cloneRect.DORotate(new Vector3(0, 0,  0),          half).SetEase(Ease.InOutSine));

        //Final Impact
        animSequence.Append(cloneRect.DOScale(originalScale, impact).SetEase(Ease.InCubic));
        animSequence.Join(cloneRect.DOAnchorPos(screenCenter, impact).SetEase(Ease.OutCubic));

        animSequence.OnComplete(() =>
        {
            if (onComplete != null)
                onComplete.Invoke();
            else
                ActivateItem(context);

            Destroy(clone);
        });

        animSequence.Play();
    }

    private void ActivateItem(ItemContext context)
    {
        // Provide origin position for flying element animations on immediate effects
        if (item.Effect is ActivatableEffectBase baseEffect)
        {
            Vector2 playerPos = FindFirstObjectByType<PlayerHandler>().transform.position;
            baseEffect.SetUIPosition(playerPos);
        }

        item.Activate(context, () =>
        {
            if (item.ConsumeOnUse)
                GameManager.Instance.PlayerStats.RemoveItemFromInventory(item);

            context.Grid.ChangeState(new IdleState(context.Grid));

            if (!item.ConsumeOnUse)
                UpdateVisualState();
        });
    }

    private void OnDestroy()
    {
        if (button != null)
            button.onClick.RemoveListener(OnItemClicked);

        transform.DOKill();
    }
}