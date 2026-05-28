using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;
using UnityEngine.Localization.Settings;

public class TutorialUIController : MonoBehaviour
{
    [Header("Prefabs")]
    [SerializeField] private GameObject dimOverlay;
    [SerializeField] private GameObject popupPanel;
    [SerializeField] private GameObject toastPanel;
    [SerializeField] private GameObject handIcon;
    [SerializeField] private GameObject highlightRingPrefab;

    [Header("References")]
    [SerializeField] private TextMeshProUGUI popupText;
    [SerializeField] private TextMeshProUGUI toastText;
    [SerializeField] private TextMeshProUGUI arrowLabelText;
    [SerializeField] private Button popupDismissButton;
    [SerializeField] private RectTransform playerHPBarAnchor;
    [SerializeField] private RectTransform enemyHPBarAnchor;
    [SerializeField] private RectTransform spellHUDAnchor;

    [Header("UI Pathing")]
    [SerializeField] private RectTransform handCanvasRoot; // Canvas/root RectTransform where handIcon lives
    [SerializeField] private Camera worldCamera;           // Camera that sees the grid/world

    private List<GameObject> activeHighlights = new List<GameObject>();
    private Tween handTween;
    private Coroutine toastCoroutine;
    private Action popupCallback;
    private RectTransform handRect;
    private RectTransform arrowRect;

    private void Start()
    {
        popupDismissButton.onClick.AddListener(OnPopupTap);
        handRect = handIcon != null ? handIcon.GetComponent<RectTransform>() : null;
        HideAll();
    }

    public void ShowPopup(string text, float autoDismiss, Action onDismiss)
    {
        popupCallback = onDismiss;
        popupText.text = text;
        dimOverlay.SetActive(true);
        popupPanel.SetActive(true);

        if (autoDismiss > 0)
            StartCoroutine(AutoDismissPopup(autoDismiss));
    }

    public void ShowToast(string text, float duration)
    {
        toastText.text = text;
        toastPanel.SetActive(true);
        toastText.gameObject.SetActive(true);
        if (toastCoroutine != null) StopCoroutine(toastCoroutine);
        toastCoroutine = StartCoroutine(HideAfter(toastPanel, duration));
    }

    public void ShowHighlights(List<Vector2Int> positions, Grid grid)
    {
        ClearHighlights();

        if (positions == null || positions.Count == 0 || grid == null || grid.VisualGrid == null)
            return;

        Transform parent = handCanvasRoot != null ? handCanvasRoot : transform;

        foreach (var pos in positions)
        {
            Vector3 worldPos = grid.VisualGrid.GridToWorldPosition(pos);
            GameObject ring = Instantiate(highlightRingPrefab, parent);

            RectTransform ringRect = ring.GetComponent<RectTransform>();
            if (ringRect != null)
            {
                ringRect.anchoredPosition = WorldToCanvasPoint(worldPos);
            }
            else
            {
                Debug.LogWarning("HighlightRingPrefab is not a UI element.");
                ring.transform.position = worldPos;
            }

            activeHighlights.Add(ring);
        }
    }

    public void ShowHandAnimation(List<Vector2Int> path, Grid grid, float duration)
    {
        if (path == null || path.Count == 0 || grid == null || grid.VisualGrid == null) return;
        if (handIcon == null || handRect == null || handCanvasRoot == null)
        {
            Debug.LogWarning("TutorialUIController: handIcon/handRect/handCanvasRoot is missing.");
            return;
        }

        KillHandTween();
        handIcon.SetActive(true);

        if (!handIcon.activeInHierarchy)
            Debug.LogWarning("TutorialUIController: handIcon parent is inactive, UI cannot be shown.");

        Vector2[] canvasPoints = new Vector2[path.Count];
        for (int i = 0; i < path.Count; i++)
        {
            Vector3 worldPos = grid.VisualGrid.GridToWorldPosition(path[i]);
            canvasPoints[i] = WorldToCanvasPoint(worldPos);
        }

        handRect.anchoredPosition = canvasPoints[0];

        if (canvasPoints.Length == 1)
        {
            handTween = DOVirtual.DelayedCall(duration, () => handIcon.SetActive(false));
            return;
        }

        Sequence seq = DOTween.Sequence();
        float segmentDuration = Mathf.Max(0.01f, duration / (canvasPoints.Length - 1));

        for (int i = 1; i < canvasPoints.Length; i++)
        {
            Vector2 next = canvasPoints[i];
            seq.Append(handRect.DOAnchorPos(next, segmentDuration).SetEase(Ease.InOutSine));
        }

        handTween = seq.OnComplete(() => handIcon.SetActive(false));
    }

    private Vector2 WorldToCanvasPoint(Vector3 worldPos)
    {
        Camera camForWorld = worldCamera != null ? worldCamera : Camera.main;
        Vector2 screenPoint = RectTransformUtility.WorldToScreenPoint(camForWorld, worldPos);

        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            handCanvasRoot,
            screenPoint,
            null,
            out Vector2 localPoint
        );

        return localPoint;
    }

   

    public void HideAll()
    {
        dimOverlay.SetActive(false);
        popupPanel.SetActive(false);
        toastPanel.SetActive(false); 
        toastText.gameObject.SetActive(true);
        handIcon.SetActive(false);
        arrowLabelText.gameObject.SetActive(false);
        KillHandTween();
        ClearHighlights();
    }

    private void KillHandTween()
    {
        if (handTween != null && handTween.IsActive())
            handTween.Kill();
        handTween = null;
    }

    private void ClearHighlights()
    {
        foreach (var h in activeHighlights)
        {
            if (h != null) Destroy(h);
        }
        activeHighlights.Clear();
    }

    private void OnPopupTap()
    {
        popupPanel.SetActive(false);
        dimOverlay.SetActive(false);
        popupCallback?.Invoke();
        popupCallback = null;
    }

    private IEnumerator AutoDismissPopup(float delay)
    {
        yield return new WaitForSeconds(delay);
        OnPopupTap();
    }

    private IEnumerator HideAfter(GameObject target, float delay)
    {
        yield return new WaitForSeconds(delay);
        target.SetActive(false);
    }
}