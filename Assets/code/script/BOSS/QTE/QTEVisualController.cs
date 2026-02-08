using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class QTEVisualController : MonoBehaviour
{
    [Header("Component References")]
    [SerializeField] private QTENumberPopup numberPopupManager;
    [SerializeField] private QTECircleMarker circleMarkerManager;
    [SerializeField] private QTETrailRenderer trailRendererManager;

    [Header("Audio")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip[] numberRevealSounds;
    [SerializeField] private AudioClip numberTouchSound;
    [SerializeField] private AudioClip trailCompleteSound;

    [Header("Canvas Reference")]
    [SerializeField] private Canvas qteCanvas;

    [Header("References")]
    [SerializeField] private Grid grid;
    [SerializeField] private Camera mainCamera;

    private bool isActive = false;

    private void Awake()
    {        
        if (mainCamera == null)
            mainCamera = Camera.main;
    }
    public void Initialize(Grid gridController)
    {
        grid = gridController;
        
        if (mainCamera == null)
            mainCamera = Camera.main;
        
        //Initialize all sub-components
        if (numberPopupManager != null && qteCanvas != null)
        {
            numberPopupManager.Initialize(qteCanvas, mainCamera, null);
        }

        if (circleMarkerManager != null && qteCanvas != null)
        {
            circleMarkerManager.Initialize(qteCanvas, mainCamera, null);
        }

        if (trailRendererManager != null)
        {
            trailRendererManager.Initialize(null, 0, Color.white, null, null);
        }
    }

    public void StartQTE()
    {
        isActive = true;
        
        if (circleMarkerManager != null)
            circleMarkerManager.SetActive(true);
        
        ClearAll();
    }

    public void EndQTE()
    {
        isActive = false;
        
        if (circleMarkerManager != null)
            circleMarkerManager.SetActive(false);
    }

    public void RevealNumbersSequential(List<(int number, Vector2Int position)> numbersToReveal, float delayBetween = 0.5f)
    {
        if (!isActive || grid == null)
        {
            return;
        }
        
        StartCoroutine(RevealNumbersSequentialRoutine(numbersToReveal, delayBetween));
    }

    private IEnumerator RevealNumbersSequentialRoutine(List<(int number, Vector2Int position)> numbersToReveal, float delayBetween)
    {
        foreach (var item in numbersToReveal)
        {
            Vector3 worldPos = grid.VisualGrid.GridToWorldPosition(item.position);

            if (numberPopupManager != null)
            {
                GameObject popup = numberPopupManager.CreatePopup(item.number, worldPos);
            }
            
            PlayNumberRevealAudio(item.number);

            if (HapticsManager.Instance != null)
            {
                HapticsManager.Instance.GenerateHapticFeedback(HapticsManager.HapticTypes.Selection);
            }

            //Wait for specified delay BEFORE revealing next number
            yield return new WaitForSecondsRealtime(delayBetween);
        }
    }

    public void OnNumberTouched(int number, Vector2Int gridPosition)
    {
        if (!isActive) return;

        Vector3 worldPos = grid.VisualGrid.GridToWorldPosition(gridPosition);

        if (trailRendererManager != null)
            trailRendererManager.AddPoint(worldPos);

        if (numberPopupManager != null)
            numberPopupManager.ExplodePopup(number - 1);

        PlayNumberTouchAudio(number);

        if (HapticsManager.Instance != null)
        {
            HapticsManager.Instance.GenerateHapticFeedback(HapticsManager.HapticTypes.LightImpact);
        }
    }

    public void OnTraceComplete(bool success)
    {
        if (!isActive) return;

        if (trailRendererManager != null)
        {
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
    }

    public void UpdateTrailToPosition(Vector3 worldPosition)
    {
        if (!isActive || trailRendererManager == null || trailRendererManager.GetPointCount() == 0) 
            return;
        
        trailRendererManager.UpdatePreview(worldPosition);
    }

    public void ClearAll()
    {
        if (numberPopupManager != null)
            numberPopupManager.ClearAll();
        
        if (circleMarkerManager != null)
            circleMarkerManager.ClearAll();
        
        if (trailRendererManager != null)
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

    private void OnDestroy()
    {
        ClearAll();
    }
}