using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class QTEVisualController : MonoBehaviour
{
    [Header("Component References")]
    [SerializeField] private QTENumberPopup numberPopupManager;
    [SerializeField] private QTETrailRenderer trailRendererManager;

    [Header("Audio")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip[] numberRevealSounds;
    [SerializeField] private AudioClip numberTouchSound;
    [SerializeField] private AudioClip trailCompleteSound;
    [SerializeField] private AudioClip timingHitSound;
    [SerializeField] private AudioClip timingMissSound;
    [SerializeField] private AudioClip timingPerfectSound;

    [Header("Canvas Reference")]
    [SerializeField] private Canvas qteCanvas;

    [Header("References")]
    [SerializeField] private Grid grid;
    [SerializeField] private Camera mainCamera;

    [Header("Timing Phase")]
    [SerializeField] private TimingRingUI timingRingUI;

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

        if (timingRingUI != null)
        {
            timingRingUI.Initialize(qteCanvas, mainCamera);
        }
    }

    public void StartQTE()
    {
        isActive = true;
        ClearAll();
    }

    public void EndQTE()
    {
        isActive = false;
    }

    public void ResumeQTE()
    {
        isActive = true;
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

        // Only add committed point if this is the correct number in sequence
        if (trailRendererManager != null)
        {
            int expectedNumber = trailRendererManager.GetPointCount() + 1;
            
            if (number == expectedNumber)
            {
                Vector3 worldPos = grid.VisualGrid.GridToWorldPosition(gridPosition);
                trailRendererManager.AddPoint(worldPos);

                if (numberPopupManager != null)
                    numberPopupManager.ExplodePopup(number - 1);

                PlayNumberTouchAudio(number);

                if (HapticsManager.Instance != null)
                {
                    HapticsManager.Instance.GenerateHapticFeedback(HapticsManager.HapticTypes.LightImpact);
                }
            }
            else
            {
                Debug.LogWarning($"Attempted to add point {number} but expected {expectedNumber}");
            }
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
        if (!isActive || trailRendererManager == null) 
            return;
        
        trailRendererManager.UpdatePreview(worldPosition);
    }

    public void ClearTrailPreview()
    {
        if (trailRendererManager != null)
        {
            trailRendererManager.ClearPreview();
        }
    }

    public void ClearAll()
    {
        if (numberPopupManager != null)
            numberPopupManager.ClearAll();
     
        
        if (trailRendererManager != null)
            trailRendererManager.Clear();
    }

    public QTETrailRenderer GetTrailRenderer()
    {
        return trailRendererManager;
    }

    public void ShowTimingBeat(int number, Vector3 worldPos)
    {
        if (!isActive) return;

        if (numberPopupManager != null)
        {
            numberPopupManager.ClearAll();
            numberPopupManager.CreatePopup(number, worldPos);
        }

        PlayNumberRevealAudio(number);

        if (HapticsManager.Instance != null)
            HapticsManager.Instance.GenerateHapticFeedback(HapticsManager.HapticTypes.Selection);
    }
    public void OnTimingBeatHit(int number, Vector2Int gridPos, bool isPerfect)
    {
        if (!isActive) return;

        if (numberPopupManager != null)
            numberPopupManager.ExplodePopup(0);

        if (audioSource != null)
        {
            AudioClip clip = isPerfect && timingPerfectSound != null ? timingPerfectSound : timingHitSound;
            if (clip != null)
            {
                Time.timeScale = 1f;
                audioSource.PlayOneShot(clip);
                Time.timeScale = 0f;
            }
        }

        if (HapticsManager.Instance != null)
            HapticsManager.Instance.GenerateHapticFeedback(HapticsManager.HapticTypes.LightImpact);
    }

    public void OnTimingBeatMissed(int number)
    {
        if (audioSource != null && timingMissSound != null)
        {
            Time.timeScale = 1f;
            audioSource.PlayOneShot(timingMissSound);
            Time.timeScale = 0f;
        }

        if (HapticsManager.Instance != null)
            HapticsManager.Instance.GenerateHapticFeedback(HapticsManager.HapticTypes.Failure);

        if (numberPopupManager != null)
            numberPopupManager.ClearAll();
    }

    public TimingRingUI GetTimingRingUI() => timingRingUI;

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
            Time.timeScale = 1;
            audioSource.PlayOneShot(trailCompleteSound);
            Time.timeScale = 0;
        }
    }

    private void OnDestroy()
    {
        ClearAll();
    }
    public void BuildTrailFromPoints(List<Vector2Int> gridPositions)
    {
        if (trailRendererManager == null || grid == null) return;

        trailRendererManager.Clear();

        foreach (Vector2Int gridPos in gridPositions)
        {
            Vector3 worldPos = grid.VisualGrid.GridToWorldPosition(gridPos);
            worldPos.z = 0;
            trailRendererManager.AddPoint(worldPos);
        }

        trailRendererManager.PrepareForAnimation();
    }
}