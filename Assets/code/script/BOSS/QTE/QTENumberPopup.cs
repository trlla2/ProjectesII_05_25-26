using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using DG.Tweening;
using UnityEngine.UIElements;

public class QTENumberPopup : MonoBehaviour
{
    [Header("Popup Settings")]
    [SerializeField] private GameObject numberPopupPrefab;
    [SerializeField] private float popupScaleInDuration = 0.3f;
    [SerializeField] private AnimationCurve popupScaleCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);
    [SerializeField] private Color[] numberColors = new Color[] { Color.cyan, Color.magenta, Color.yellow, Color.green, Color.red, Color.white };
    [SerializeField] private float numberGlowIntensity = 2f;

    [Header("References")]
    [SerializeField] private Canvas qteCanvas;

    private List<GameObject> activePopups = new List<GameObject>();

    public void Initialize(Canvas canvas, GameObject prefab, float scaleInDuration, AnimationCurve scaleCurve, Color[] colors, float glowIntensity)
    {
        qteCanvas = canvas;
        numberPopupPrefab = prefab;
        popupScaleInDuration = scaleInDuration;
        popupScaleCurve = scaleCurve;
        numberColors = colors;
        numberGlowIntensity = glowIntensity;
    }

    public GameObject CreatePopup(int number, Vector3 worldPosition)
    {
        GameObject popup = Instantiate(numberPopupPrefab, qteCanvas.transform);

        // Position in world space
        Vector3 screenPos = Camera.main.WorldToScreenPoint(worldPosition);
        RectTransform rt = popup.GetComponent<RectTransform>();
        if (rt == null)
            rt = popup.AddComponent<RectTransform>();
        
        rt.position = screenPos;

        // Setup text
        TMP_Text text = popup.GetComponentInChildren<TMP_Text>();
        if (text == null)
        {
            text = popup.AddComponent<TextMeshProUGUI>();
        }

        text.text = number.ToString();
        
        //Apply color
        Color numberColor = number - 1 < numberColors.Length ? numberColors[number - 1] : Color.white;
        text.color = numberColor * numberGlowIntensity;

        popup.GetComponent<Image>().tintColor = numberColor * Color.grey * numberGlowIntensity;

        //Scale animation
        popup.transform.localScale = Vector3.zero;
        popup.transform.DOScale(Vector3.one * 1.2f, popupScaleInDuration).SetEase(popupScaleCurve)
            .OnComplete(() => {
                popup.transform.DOScale(Vector3.one, 0.1f);
            });

        activePopups.Add(popup);
        return popup;
    }

    public void ExplodePopup(int index)
    {
        if (index < 0 || index >= activePopups.Count) return;

        GameObject popup = activePopups[index];
        if (popup == null) return;

        //Particle explode effect
        popup.transform.DOScale(Vector3.one * 1.5f, 0.1f).OnComplete(() => {
            popup.transform.DOScale(Vector3.zero, 0.2f).OnComplete(() => Destroy(popup));
        });

        TMP_Text text = popup.GetComponentInChildren<TMP_Text>();
        if (text != null)
        {
            text.DOFade(0f, 0.3f);
        }
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
}