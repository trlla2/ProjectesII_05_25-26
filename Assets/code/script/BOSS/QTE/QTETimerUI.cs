using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class QTETimerUI : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private TMP_Text timerText;
    [SerializeField] private Image timerFillImage;
    [SerializeField] private GameObject timerContainer;

    [Header("Colors")]
    [SerializeField] private Color normalColor = Color.white;
    [SerializeField] private Color warningColor = Color.red;
    [SerializeField] private float warningThreshold = 1f;

    [Header("Animation")]
    [SerializeField] private float pulseSpeed = 5f;
    [SerializeField] private float pulseAmount = 0.1f;

    private bool isActive = false;
    private float currentTime = 0f;
    private float maxTime = 0f;

    private void Awake()
    {
        if (timerContainer != null)
            timerContainer.SetActive(false);
    }

    public void StartTimer(float duration)
    {
        maxTime = duration;
        currentTime = duration;
        isActive = true;

        if (timerContainer != null)
            timerContainer.SetActive(true);

        UpdateDisplay();
    }

    public void UpdateTimer(float remainingTime, float maxDuration)
    {
        if (!isActive) return;

        currentTime = remainingTime;
        maxTime = maxDuration;

        UpdateDisplay();
    }

    public void HideTimer()
    {
        isActive = false;

        if (timerContainer != null)
            timerContainer.SetActive(false);
    }

    private void UpdateDisplay()
    {
        float fillAmount = currentTime / maxTime;

        if (timerText != null)
        {
            timerText.text = currentTime.ToString("F1") + "s";

            if (currentTime <= warningThreshold)
            {
                timerText.color = warningColor;
                float pulse = 1f + Mathf.Sin(Time.unscaledTime * pulseSpeed) * pulseAmount;
                timerText.transform.localScale = Vector3.one * pulse;
            }
            else
            {
                timerText.color = normalColor;
                timerText.transform.localScale = Vector3.one;
            }
        }

        // Update fill image
        if (timerFillImage != null)
        {
            timerFillImage.fillAmount = fillAmount;

            if (currentTime <= warningThreshold)
            {
                timerFillImage.color = warningColor;
            }
            else
            {
                timerFillImage.color = normalColor;
            }
        }
    }
}