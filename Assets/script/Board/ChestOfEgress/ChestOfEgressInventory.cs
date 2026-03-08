using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "ChestOfEgressInventory", menuName = "Game/Chest Of Egress Inventory")]
public class ChestOfEgressInventory : ScriptableObject
{
    [SerializeField] private int maxSlots = 9;
    public int MaxSlots => maxSlots;

    [SerializeField] private List<Item> _items = new List<Item>();
    public IReadOnlyList<Item> Items => _items;

    private void OnEnable()
    {
        if (_items == null)
            _items = new List<Item>();
    }

    public bool AddItem(Item item)
    {
        if (_items.Count >= maxSlots) return false;
        _items.Add(item);
        return true;
    }

    public bool RemoveItem(Item item)
    {
        return _items.Remove(item);
    }

    public bool IsFull() => _items.Count >= maxSlots;
}