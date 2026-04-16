using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using static UnityEditor.Progress;

[RequireComponent(typeof(EventTrigger), typeof(TranslatorUI))]
public class ItemSelectable : MonoBehaviour
{
    [Header("Setup")]
    [SerializeField] private Image itemSprite;
    [SerializeField] private GameObject selectImage;

    private Item currentItem;
    public Item Item => currentItem;

    private RemoveItemPanelHandler panelRef;
    private TranslatorUI translator;

    public bool CanBeSelected = true;
    
    public System.Action OnSelect;

    private void Awake()
    {
        translator = GetComponent<TranslatorUI>();

        selectImage.SetActive(false);
    }

    public void SetItem(RemoveItemPanelHandler reference, Item item)
    {

        currentItem = item;
        itemSprite.sprite = item.Sprite;
        panelRef = reference;
    }

    public void Select()
    {
        if (!CanBeSelected)
            return;

        selectImage.SetActive(true);
        translator.ToTarget();

        OnSelect?.Invoke();

        if (panelRef != null)
            panelRef.SelectItem(this);
        
        HapticsManager.Instance.GenerateHapticFeedback(HapticsManager.HapticTypes.RigidImpact);
    }

    public void UnSelect()
    {
        selectImage.SetActive(false);
    }
}
