using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

[RequireComponent(typeof(EventTrigger),typeof(TranslatorUI))]
public class ItemSelectable : MonoBehaviour
{
    [Header("Setup")]
    [SerializeField]
    private Image itemSprite;
    [SerializeField]
    private GameObject selectImage;
    private Item currentItem;
    public Item Item => currentItem;
    
    

    private RemoveItemPanelHandler panelRef;
    private TranslatorUI translator;

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
        selectImage.SetActive(true);
        translator.ToTarget();
        panelRef.SelectItem(this);
        HapticsManager.Instance.GenerateHapticFeedback(HapticsManager.HapticTypes.RigidImpact);
    }

    public void UnSelect()
    {
        selectImage.SetActive(false);
    }


}
