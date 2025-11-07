using UnityEngine;
using System.Collections;


public class SlotHolder : MonoBehaviour
{
    public Slot LinkedSlot;
    public Highlighter highlighter;
    public Highlighter Highlighter() => highlighter;
}
