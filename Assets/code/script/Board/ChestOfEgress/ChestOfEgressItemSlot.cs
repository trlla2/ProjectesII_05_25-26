using UnityEngine;
using UnityEngine.EventSystems;

/// <summary>
/// Componente auxiliar que se añade dinámicamente sobre un ItemSelectable
/// para redirigir el click hacia el ChestOfEgressScreen.
/// </summary>
[RequireComponent(typeof(ItemSelectable))]
public class ChestOfEgressItemSlot : MonoBehaviour
{
    private Item _item;
    private bool _isChestPanel;
    private ChestOfEgressScreen _screen;

    public void Setup(Item item, bool isChestPanel, ChestOfEgressScreen screen)
    {
        _item = item;
        _isChestPanel = isChestPanel;
        _screen = screen;

        EventTrigger trigger = GetComponent<EventTrigger>();
        if (trigger == null) return;

        EventTrigger.Entry entry = new EventTrigger.Entry
        {
            eventID = EventTriggerType.PointerClick
        };
        entry.callback.AddListener(_ => OnClick());
        trigger.triggers.Add(entry);
    }

    private void OnClick()
    {
        if (_isChestPanel)
            _screen.MoveItemToPlayer(_item);
        else
            _screen.MoveItemToChest(_item);
    }
}