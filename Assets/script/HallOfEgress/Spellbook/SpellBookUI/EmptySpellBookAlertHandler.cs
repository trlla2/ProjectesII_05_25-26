using UnityEngine;
using UnityEngine.Events;

[RequireComponent(typeof(TranslatorUI))]
public class EmptySpellBookAlertHandler : MonoBehaviour
{
    public UnityEvent OnShow;
    public void Show()
    {
        OnShow.Invoke();
    }
}
