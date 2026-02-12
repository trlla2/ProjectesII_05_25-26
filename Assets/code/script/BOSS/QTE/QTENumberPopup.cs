using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using DG.Tweening;
using UnityEngine.UI;

public class QTENumberPopup : MonoBehaviour
{
    [Header("Popup Settings")]
    [SerializeField] private GameObject numberPopupPrefab;
    [SerializeField] private float popupScaleInDuration = 0.3f;
    [SerializeField] private AnimationCurve popupScaleCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);
    [SerializeField] private Color[] numberColors = new Color[] 
    { 
        Color.cyan, Color.magenta, Color.yellow, 
        Color.green, Color.red, Color.white 
    };
    [SerializeField] private float numberGlowIntensity = 2f;
    [SerializeField] private float fontSize = 48f;

    [Header("References")]
    [SerializeField] private Canvas qteCanvas;
    [SerializeField] private Camera mainCamera;

    private List<GameObject> activePopups = new List<GameObject>();

    public void Initialize(Canvas canvas, Camera camera, GameObject prefab)
    {
        if (canvas != null)
        {
            qteCanvas = canvas;
        }

        if (camera != null)
        {
            mainCamera = camera;
        }
        else if (mainCamera == null)
        {
            mainCamera = Camera.main;
        }
        
        if (prefab != null)
        {
            numberPopupPrefab = prefab;
        }
        else
        {
            Debug.LogWarning("[QTENumberPopup] Null prefab passed to Initialize - using inspector value");
        }
    }

    public GameObject CreatePopup(int number, Vector3 worldPosition)
    {
        if (numberPopupPrefab == null || mainCamera == null || qteCanvas == null)
        {
            Debug.LogError("Null References!!");
            return null;
        }

        GameObject popup = Instantiate(numberPopupPrefab, qteCanvas.transform);

        //Convert world position to canvas position
        Vector2 canvasPosition = WorldToCanvasPosition(worldPosition);
        RectTransform rt = popup.GetComponent<RectTransform>();
        rt.anchoredPosition = canvasPosition;

        TMP_Text text = popup.GetComponentInChildren<TMP_Text>();
        text.text = number.ToString();
        
        Color numberColor = (number - 1 < numberColors.Length) ? numberColors[number - 1] : Color.white;
        text.color = numberColor * numberGlowIntensity;

        Image bgImage = popup.GetComponent<Image>();
        if (bgImage != null)
        {
            bgImage.color = new Color(numberColor.r, numberColor.g, numberColor.b, 0.3f);
        }

        //Scale animation with unscaled time (bc paused game)
        popup.transform.localScale = Vector3.zero;
        popup.transform.DOScale(Vector3.one * 1.2f, popupScaleInDuration)
            .SetEase(popupScaleCurve)
            .SetUpdate(true)
            .OnComplete(() => {
                popup.transform.DOScale(Vector3.one, 0.1f).SetUpdate(true);
            });

        activePopups.Add(popup);
        return popup;
    }

    private Vector2 WorldToCanvasPosition(Vector3 worldPosition)
    {
        RectTransform canvasRect = qteCanvas.GetComponent<RectTransform>();
        Vector2 viewportPosition = mainCamera.WorldToViewportPoint(worldPosition);

        // Convert viewport position to canvas position
        Vector2 canvasPosition = new Vector2(
            (viewportPosition.x * canvasRect.sizeDelta.x) - (canvasRect.sizeDelta.x * 0.5f),
            (viewportPosition.y * canvasRect.sizeDelta.y) - (canvasRect.sizeDelta.y * 0.5f)
        );

        return canvasPosition;
    }
    public void ExplodePopup(int index)
    {
        if (index < 0 || index >= activePopups.Count)
        {
            return;
        }

        GameObject popup = activePopups[index];
        if (popup == null) return;

        TMP_Text text = popup.GetComponentInChildren<TMP_Text>();
        if (text != null)
        {
            text.DOFade(0f, 0.3f).SetUpdate(true);
        }

        Image bgImage = popup.GetComponent<Image>();
        if (bgImage != null)
        {
            bgImage.DOFade(0f, 0.3f).SetUpdate(true);
        }

        popup.transform.DOScale(Vector3.one * 1.5f, 0.1f)
            .SetUpdate(true)
            .OnComplete(() => {
                popup.transform.DOScale(Vector3.zero, 0.2f)
                    .SetUpdate(true)
                    .OnComplete(() => Destroy(popup));
            });
    }

    public void ClearAll()
    {
        foreach (var popup in activePopups)
        {
            if (popup != null)
                Destroy(popup);
        }
        activePopups.Clear();
    }

    public int GetPopupCount()
    {
        return activePopups.Count;
    }

    private void OnDestroy()
    {
        ClearAll();
    }
}