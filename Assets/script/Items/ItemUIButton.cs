using System;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

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

    private void Awake()
    {
        button = GetComponent<Button>();
        image = GetComponent<Image>();
        
        if (canvasGroup == null)
        {
            canvasGroup = gameObject.AddComponent<CanvasGroup>();
        }
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
            else if (canActivate)
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
        Debug.Log($"Attempting to activate {item.NameItem}...");
        if (item == null || !item.IsActivatable) return;
        ItemContext context = new ItemContext
        {
            Grid = GameManager.Instance.CombatHandler.GetEnemyRef().GetGrid(),
            Player = GameManager.Instance.PlayerStats,
            Enemy = GameManager.Instance.CombatHandler.GetEnemyRef(),
            SpellAnimator = GameManager.Instance.CombatHandler.GetEnemyRef().GetComponentInChildren<SpellPathAnimator>() ?? null
        };

        if(context.SpellAnimator == null)
        {
            Debug.LogWarning("No SpellAnimator found in enemy!");
        }   

        if (!item.CanActivate(context))
        {
            Debug.Log($"Cannot activate {item.NameItem} right now!");
            return;
        }

        if (item.ActivationType == ItemActivationType.GridTarget)
        {
            // TODO: Start grid target selection mode
            StartGridTargetSelection(context);
        }
        else
        {
            // Immediate activation
            ActivateItem(context);
        }
    }

    private void ActivateItem(ItemContext context)
    {
        if(item.Effect is FireRingEffect fireRingEffect)
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
        
        // For now, just activate immediately
        ActivateItem(context);
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
    }
}