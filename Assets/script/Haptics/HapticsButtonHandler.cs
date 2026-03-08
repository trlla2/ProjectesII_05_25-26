 using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;


[RequireComponent(typeof(Button))]
[RequireComponent(typeof(EventTrigger))]
public class HapticsButtonHandler : MonoBehaviour, IPointerDownHandler
{
    public void OnPointerDown(PointerEventData eventData)
    {
        Feedback();
    }

    public void Feedback()
    {
        HapticsManager.Instance.GenerateHapticFeedback(HapticsManager.HapticTypes.RigidImpact);
    }
}
