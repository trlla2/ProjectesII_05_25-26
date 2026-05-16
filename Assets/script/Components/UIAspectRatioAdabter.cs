using UnityEngine;

public class UIAspectRatioAdabter : MonoBehaviour
{
    private RectTransform rectTransform;    
    public Vector2 targetPosition;

    private float tabletThreshold = 0.7f;


    private void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
    }

    void Start()
    {
        float screenAspect = (float)Screen.width / Screen.height;
        Debug.Log($"Screen Aspect Ratio: {screenAspect}");
        if (screenAspect >= tabletThreshold)
        {
            rectTransform.position = targetPosition;
        }
        
    }
}
