using System;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using DG.Tweening;

[RequireComponent(typeof(Button))]
public class ItemUIButton : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    private Item item;
    private Button button;
    private Image image;
    private CanvasGroup canvasGroup;

    [Header("UI")]
    [SerializeField] private Color activeColor = Color.white;
    [SerializeField] private Color inactiveColor = new Color(0.5f, 0.5f, 0.5f, 1f);
    [SerializeField] private Color cooldownColor = new Color(1f, 0.5f, 0.5f, 1f);

    [Header("Animation")]
    [SerializeField] private float upwardOffset = 100f;
    [SerializeField] private float scaleMultiplier = 10f;
    [SerializeField] private float shakeAngle = 15f;

    private static Vector2 centerOffset = new Vector2(900, 2000); //we need it be the ItemUi is in the inventorycanvas  

    private void Awake()
    {
        button = GetComponent<Button>();
        image = GetComponent<Image>();
        canvasGroup = GetComponent<CanvasGroup>();
    }

    public void Initialize(Item itemData)
    {
        item = itemData;
        image.sprite = item.Sprite;

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

        if (!item.IsActivatable)
        {
            button.interactable = false;
            canvasGroup.alpha = 0.7f;
            image.color = inactiveColor;
        }
        else
        {
            ItemContext context = new ItemContext
            {
                Grid = GameManager.Instance.CombatHandler.GetEnemyRef().GetGrid(),
                Player = GameManager.Instance.PlayerStats,
                Enemy = GameManager.Instance.CombatHandler.GetEnemyRef()
            };

            bool canActivate = item.CanActivate(context);
            button.interactable = canActivate;

            if (item.IsOnCooldown())
            {
                canvasGroup.alpha = 0.5f;
                image.color = cooldownColor;
            }
            if (canActivate)
            {
                canvasGroup.alpha = 1f;
                image.color = activeColor;
            }
            else
            {
                canvasGroup.alpha = 0.7f;
                image.color = inactiveColor;
            }
        }
    }

    public void OnItemClicked()
    {
        if (item == null || !item.IsActivatable) return;
        ItemContext context = new ItemContext
        {
            Grid = GameManager.Instance.CombatHandler.GetEnemyRef().GetGrid(),
            Player = GameManager.Instance.PlayerStats,
            Enemy = GameManager.Instance.CombatHandler.GetEnemyRef(),
            SpellAnimator = GameManager.Instance.CombatHandler.GetEnemyRef().GetComponentInChildren<SpellPathAnimator>() ?? null
        };

        if (!item.CanActivate(context))
        {
            Debug.Log($"Cannot activate {item.NameItem} right now!");
            return;
        }

        if (item.ActivationType == ItemActivationType.GridTarget)
        {
            StartGridTargetSelection(context);
        }
        else
        {
            // Start animation sequence before activation
            PlayItemUseAnimation(context);
        }
    }

    private void PlayItemUseAnimation(ItemContext context)
    {
        // Create a clone of this item UI element
        GameObject clone = Instantiate(gameObject, transform.parent);
        clone.GetComponent<CanvasGroup>().alpha = 1f;
        RectTransform cloneRect = clone.GetComponent<RectTransform>();
        Image cloneImage = clone.GetComponent<Image>();

        // Disable the button on the clone so it can't be clicked
        Button cloneButton = clone.GetComponent<Button>();
        if (cloneButton != null)
        {
            cloneButton.interactable = false;
        }

        // Store original position and scale
        Vector3 originalPosition = cloneRect.anchoredPosition;
        Vector3 originalScale = cloneRect.localScale;

        // Screen center is at 0,0, add upward offset
        Vector2 centerWithOffset = centerOffset + new Vector2(0, upwardOffset);
        Vector2 screenCenter = centerOffset;

        // Create animation sequence
        Sequence animSequence = DOTween.Sequence();

        //Move to center
        animSequence.Append(cloneRect.DOAnchorPos(centerWithOffset, 0.4f).SetEase(Ease.OutCubic));
        animSequence.Join(cloneRect.DOScale(originalScale * scaleMultiplier, 0.4f).SetEase(Ease.OutCubic));

        //Shake
        animSequence.Append(cloneRect.DORotate(new Vector3(0, 0, shakeAngle), 0.075f/2.0f).SetEase(Ease.InOutSine));
        animSequence.Append(cloneRect.DORotate(new Vector3(0, 0, -shakeAngle), 0.15f / 2.0f).SetEase(Ease.InOutSine));
        animSequence.Append(cloneRect.DORotate(new Vector3(0, 0, 0), 0.075f / 2.0f).SetEase(Ease.InOutSine));
        animSequence.Append(cloneRect.DORotate(new Vector3(0, 0, shakeAngle), 0.075f/2.0f).SetEase(Ease.InOutSine));
        animSequence.Append(cloneRect.DORotate(new Vector3(0, 0, -shakeAngle), 0.15f / 2.0f).SetEase(Ease.InOutSine));
        animSequence.Append(cloneRect.DORotate(new Vector3(0, 0, 0), 0.075f / 2.0f).SetEase(Ease.InOutSine));

        //Final Impact
        animSequence.Append(cloneRect.DOScale(originalScale, 0.2f).SetEase(Ease.InCubic));
        animSequence.Join(cloneRect.DOAnchorPos(screenCenter, 0.2f).SetEase(Ease.OutCubic));

        animSequence.OnComplete(() =>
        {
            ActivateItem(context);
            Destroy(clone);
        });

        animSequence.Play();
    }

    private void ActivateItem(ItemContext context)
    {
        if (item.Effect is FireRingEffect fireRingEffect)
        {
            Vector2 PlayerPos = FindFirstObjectByType<PlayerHandler>().transform.position; //ineficient but i dont wanna go thoguh reference hell
            fireRingEffect.SetUIPosition(PlayerPos); //pas origin postion to te ring effect
        }
        item.Activate(context, () =>
        {

            // Remove from inventory 
            if (item.ConsumeOnUse)
            {
                GameManager.Instance.PlayerStats.RemoveItemFromInventory(item);
            }
            else
            {
                UpdateVisualState();
            }
        });
    }

    private void StartGridTargetSelection(ItemContext context)
    {
        // Placeholder for grid target selection
        //Need to implement a method to select the grid and stop the current game events
        //This should be a whole nother class
        Debug.LogWarning("Grid target selection not yet implemented!");

        // For now, just play animation and activate
        PlayItemUseAnimation(context);
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (item != null)
        {
            // TODO: Show tooltip with item info
            Debug.Log($"{item.NameItem}: {item.Description}");
            if (item.IsOnCooldown())
            {
                Debug.Log($"Cooldown: {item.GetCooldownRemaining():F1}s");
            }
        }
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        // TODO: Hide tooltip
    }

    private void OnDestroy()
    {
        if (button != null)
        {
            button.onClick.RemoveListener(OnItemClicked);
        }

        // Kill any tweens on this object to prevent errors
        transform.DOKill();
    }
}