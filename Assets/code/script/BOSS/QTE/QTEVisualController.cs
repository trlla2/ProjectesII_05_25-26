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

    [Header("References")]
    [SerializeField] private Grid grid;
    [SerializeField] private Canvas qteCanvas;

    private bool isActive = false;


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
        if (delay > 0f)
            yield return new WaitForSecondsRealtime(delay);

        Vector3 worldPos = grid.VisualGrid.GridToWorldPosition(gridPosition);

        // Create glowing circle
        circleMarkerManager.CreateCircle(worldPos, trailRendererManager.trailColor);

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

        //Add point to trail
        trailRendererManager.AddPoint(worldPos);

        //Explode number popup
        numberPopupManager.ExplodePopup(number - 1);

        //Remove circle
        circleMarkerManager.RemoveCircle(number - 1);

        //Play audio --- NOT IMPLEMENTED YET
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

    public void RevealNumbersSequential(List<(int number, Vector2Int position)> numbersToReveal, float delayBetween = 0.5f)
    {
        if (!isActive || grid == null) return;
        StartCoroutine(RevealNumbersSequentialRoutine(numbersToReveal, delayBetween));
    }

    private IEnumerator RevealNumbersSequentialRoutine(List<(int number, Vector2Int position)> numbersToReveal, float delayBetween)
    {
        foreach (var item in numbersToReveal)
        {
            Vector3 worldPos = grid.VisualGrid.GridToWorldPosition(item.position);

            // Create circle and number at the same time
            circleMarkerManager.CreateCircle(worldPos, trailRendererManager.trailColor);
            numberPopupManager.CreatePopup(item.number, worldPos);
            
            PlayNumberRevealAudio(item.number);

            if (HapticsManager.Instance != null)
            {
                HapticsManager.Instance.GenerateHapticFeedback(HapticsManager.HapticTypes.Selection);
            }

            // Wait for specified delay BEFORE revealing next number
            yield return new WaitForSecondsRealtime(delayBetween);
        }
    }
}