using UnityEditor;
using UnityEngine;

public class SlotHolder : MonoBehaviour
{
    public Slot LinkedSlot;
    public Highlighter highlighter;
    public Highlighter Highlighter() => highlighter;
}
