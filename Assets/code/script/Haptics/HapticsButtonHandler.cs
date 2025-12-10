 using UnityEngine;
using UnityEngine.UI;


[RequireComponent(typeof(Button))]
public class HapticsButtonHandler : MonoBehaviour
{
    private Button _button;
    [SerializeField]
    public HapticsManager.HapticTypes hapticType;

    private void Awake()
    {
        _button = GetComponent<Button>();
        _button.onClick.AddListener(OnButtonPress);
    }

    public void OnButtonPress()
    {
        HapticsManager.instance.GenerateHapticFeedback(hapticType);
    }
}
