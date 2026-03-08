using UnityEngine;
using UnityEngine.UI;

public class UIElement_Behaviour : MonoBehaviour
{
    [Header("SETUP")]
    [SerializeField]
    private Image elementImage;
    public void SetupElementUI(Element element)
    {
        elementImage.sprite = element.Sprite;
    }
}
