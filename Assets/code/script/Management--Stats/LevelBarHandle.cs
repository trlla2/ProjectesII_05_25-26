using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class LevelBarHandle : MonoBehaviour
{
    [Header("Exp Bar")]
    [SerializeField]
    private Slider expSlider;
    [SerializeField]
    private TMP_Text lvlText;
    [SerializeField]
    private float interpolationDuration = 1.0f;
    [SerializeField]
    private AnimationCurve interpolationCurve;

    private float interpolationTime = 0.0f;
    private bool isInterpolating = false;

    [Header("LevelUp Bar Animation")]
    [SerializeField]
    private AnimationCurve breathingCurve;
    [SerializeField]
    private float breathingSpeed = 1.5f;

    private float breathingTime = 0.0f;
    private bool breathingBackwards = false;
    private Color originalColor;
    private bool isAnimatingLevelUp = false;
    private Image fillImage;

    private void Start()
    {
        if (expSlider.fillRect != null)
        {
            fillImage = expSlider.fillRect.GetComponent<Image>();
            if (fillImage != null)
            {
                originalColor = fillImage.color;
            }
        }

        GameManager.Instance.OnPlayerLevelUp += StartLevelUpAnimation;
        GameManager.Instance.OnPlayerEXPChange += UpdateEXPBar;

        UpdateEXPBar();
    }

    private void Update()
    {
        HandleInterpolation();
        HandleLevelUpAnimation();
    }

    private void OnEnable() {  StopLevelUpAnimation(); }

    private void HandleInterpolation()
    {
        if (!isInterpolating) return;

        interpolationTime += Time.deltaTime;

        float interpolateValue = interpolationCurve.Evaluate(interpolationTime / interpolationDuration);

        float currentExp = GameManager.Instance.GetPlayerEXP() - GameManager.Instance.GetPlayerEXPForLastLevel();
        float nextLevelExp = GameManager.Instance.GetPlayerEXPForNextLevel() - GameManager.Instance.GetPlayerEXPForLastLevel();

        if (nextLevelExp > 0)
        {
            expSlider.value = (currentExp * interpolateValue) / nextLevelExp;
        }

        if (interpolationTime >= interpolationDuration)
        {
            interpolationTime = 0.0f;
            isInterpolating = false;

            if (nextLevelExp > 0)
            {
                expSlider.value = (float)currentExp / (float)nextLevelExp;
            }
        }
    }

    private void HandleLevelUpAnimation()
    {
        if (!isAnimatingLevelUp || fillImage == null) return;

        if (!breathingBackwards)
        {
            breathingTime += Time.deltaTime;
            if (breathingTime >= breathingSpeed)
            {
                breathingTime = breathingSpeed;
                breathingBackwards = true;
            }
        }
        else
        {
            breathingTime -= Time.deltaTime;
            if (breathingTime <= 0)
            {
                breathingTime = 0;
                breathingBackwards = false;
            }
        }

        fillImage.color = Color.Lerp(originalColor, Color.white, breathingCurve.Evaluate(breathingTime / breathingSpeed));
    }

    private void StartLevelUpAnimation()
    {
        if (fillImage != null)
        {
            fillImage.color = originalColor;
            breathingTime = 0.0f;
            breathingBackwards = false;
            isAnimatingLevelUp = true;
        }
    }

    private void UpdateEXPBar()
    {
        lvlText.text = GameManager.Instance.GetPlayerLevel() + "lvl";

        interpolationTime = 0.0f;
        isInterpolating = true;
    }

    public void StopLevelUpAnimation()
    {
        isAnimatingLevelUp = false;

        if (fillImage != null)
        {
            fillImage.color = originalColor;
        }

        UpdateEXPBar();
    }

    private void OnDestroy()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnPlayerLevelUp -= StartLevelUpAnimation;
            GameManager.Instance.OnPlayerEXPChange -= UpdateEXPBar;
        }
    }
}