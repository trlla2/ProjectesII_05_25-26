using System.Collections;
using UnityEngine;

public class QTEVisualController : MonoBehaviour
{
    [Header("Component References")]
    [SerializeField] private QTENumberPopup numberPopupManager;
    [SerializeField] private QTECircleMarker circleMarkerManager;
    [SerializeField] private QTETrailRenderer trailRendererManager;

    [Header("Popup Settings")]
    [SerializeField] private GameObject numberPopupPrefab;
    [SerializeField] private float popupScaleInDuration = 0.3f;
    [SerializeField] private AnimationCurve popupScaleCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);
    [SerializeField] private Color[] numberColors = new Color[] { Color.cyan, Color.magenta, Color.yellow, Color.green, Color.red, Color.white };
    [SerializeField] private float numberGlowIntensity = 2f;

    [Header("Circle Settings")]
    [SerializeField] private GameObject circlePrefab;
    [SerializeField] private float circlePulseSpeed = 2f;
    [SerializeField] private float circleMinScale = 0.9f;
    [SerializeField] private float circleMaxScale = 1.1f;

    [Header("Trail Settings")]
    [SerializeField] private LineRenderer trailRenderer;
    [SerializeField] private float trailWidth = 0.15f;
    [SerializeField] private Color trailColor = new Color(0.6f, 0.3f, 1f, 1f);
    [SerializeField] private Material trailMaterial;
    [SerializeField] private GameObject trailParticlesPrefab;

    [Header("Audio")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip[] numberRevealSounds;
    [SerializeField] private AudioClip numberTouchSound;
    [SerializeField] private AudioClip trailCompleteSound;

    [Header("References")]
    [SerializeField] private Grid grid;
    [SerializeField] private Canvas qteCanvas;

    private bool isActive = false;

    private void Awake()
    {
        InitializeComponents();
    }

    private void InitializeComponents()
    {
        // Auto-create components if not assigned
        if (numberPopupManager == null)
        {
            GameObject popupObj = new GameObject("NumberPopupManager");
            popupObj.transform.SetParent(transform);
            numberPopupManager = popupObj.AddComponent<QTENumberPopup>();
        }

        if (circleMarkerManager == null)
        {
            GameObject circleObj = new GameObject("CircleMarkerManager");
            circleObj.transform.SetParent(transform);
            circleMarkerManager = circleObj.AddComponent<QTECircleMarker>();
        }

        if (trailRendererManager == null)
        {
            GameObject trailObj = new GameObject("TrailRendererManager");
            trailObj.transform.SetParent(transform);
            trailRendererManager = trailObj.AddComponent<QTETrailRenderer>();
        }

        // Initialize each component with settings
        numberPopupManager.Initialize(qteCanvas, numberPopupPrefab, popupScaleInDuration, popupScaleCurve, numberColors, numberGlowIntensity);
        circleMarkerManager.Initialize(circlePrefab, circlePulseSpeed, circleMinScale, circleMaxScale, popupScaleInDuration, popupScaleCurve);
        trailRendererManager.Initialize(trailRenderer, trailWidth, trailColor, trailMaterial, trailParticlesPrefab);
    }

    public void Initialize(Grid gridController)
    {
        grid = gridController;
        if (audioSource == null)
            audioSource = GetComponent<AudioSource>();
    }

    public void StartQTE()
    {
        isActive = true;
        circleMarkerManager.SetActive(true);
        ClearAll();
    }

    public void EndQTE()
    {
        isActive = false;
        circleMarkerManager.SetActive(false);
    }

    public void RevealNumber(int number, Vector2Int gridPosition, float delay = 0f)
    {
        if (!isActive || grid == null) return;
        StartCoroutine(RevealNumberRoutine(number, gridPosition, delay));
    }

    private IEnumerator RevealNumberRoutine(int number, Vector2Int gridPosition, float delay)
    {
        yield return new WaitForSeconds(delay);

        Vector3 worldPos = grid.VisualGrid.GridToWorldPosition(gridPosition);

        // Create glowing circle
        circleMarkerManager.CreateCircle(worldPos, trailColor);

        // Create number popup
        numberPopupManager.CreatePopup(number, worldPos);

        // Play audio
        PlayNumberRevealAudio(number);

        // Haptic feedback
        if (HapticsManager.Instance != null)
        {
            HapticsManager.Instance.GenerateHapticFeedback(HapticsManager.HapticTypes.Selection);
        }
    }

    public void OnNumberTouched(int number, Vector2Int gridPosition)
    {
        if (!isActive) return;

        Vector3 worldPos = grid.VisualGrid.GridToWorldPosition(gridPosition);

        // Add point to trail
        trailRendererManager.AddPoint(worldPos);

        // Explode number popup
        numberPopupManager.ExplodePopup(number - 1);

        // Remove circle
        circleMarkerManager.RemoveCircle(number - 1);

        // Play audio
        PlayNumberTouchAudio(number);

        // Haptic feedback
        if (HapticsManager.Instance != null)
        {
            HapticsManager.Instance.GenerateHapticFeedback(HapticsManager.HapticTypes.LightImpact);
        }
    }

    public void OnTraceComplete(bool success)
    {
        if (!isActive) return;

        if (success)
        {
            StartCoroutine(trailRendererManager.SolidifyTrail());
            PlayTrailCompleteAudio();
            
            if (HapticsManager.Instance != null)
            {
                HapticsManager.Instance.GenerateHapticFeedback(HapticsManager.HapticTypes.Success);
            }
        }
        else
        {
            StartCoroutine(trailRendererManager.ShatterTrail());
            
            if (HapticsManager.Instance != null)
            {
                HapticsManager.Instance.GenerateHapticFeedback(HapticsManager.HapticTypes.Failure);
            }
        }
    }

    public void UpdateTrailToPosition(Vector3 worldPosition)
    {
        if (!isActive || trailRendererManager.GetPointCount() == 0) return;
        trailRendererManager.UpdatePreview(worldPosition);
    }

    public void ClearAll()
    {
        numberPopupManager.ClearAll();
        circleMarkerManager.ClearAll();
        trailRendererManager.Clear();
    }

    private void PlayNumberRevealAudio(int number)
    {
        if (audioSource != null && numberRevealSounds != null && number - 1 < numberRevealSounds.Length)
        {
            audioSource.pitch = 1f + (number - 1) * 0.1f;
            audioSource.PlayOneShot(numberRevealSounds[number - 1]);
        }
    }

    private void PlayNumberTouchAudio(int number)
    {
        if (audioSource != null && numberTouchSound != null)
        {
            audioSource.pitch = 1f + (number - 1) * 0.15f;
            audioSource.PlayOneShot(numberTouchSound);
        }
    }

    private void PlayTrailCompleteAudio()
    {
        if (audioSource != null && trailCompleteSound != null)
        {
            audioSource.PlayOneShot(trailCompleteSound);
        }
    }
}