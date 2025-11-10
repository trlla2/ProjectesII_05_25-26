using System.Collections;
using System.Security.Cryptography;
using System.Xml;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using static UnityEngine.Rendering.DebugUI;

public class LevelUIBehaviour : MonoBehaviour
{
    [Header("Exp Bar")]
    [SerializeField]
    private Slider expSlider;
    [SerializeField]
    private TMP_Text lvlText;
    [SerializeField]
    private float interpolationDuration = 1.0f;
    private float interpolationTime = 0.0f;
    [SerializeField]
    private AnimationCurve interpolationCurve;

    [Header("LevelUp Bar Animation")]
    [SerializeField]
    private AnimationCurve breathingCurve;
    [SerializeField]
    private float breathingSpeed = 1.5f;
    private float breathingTime = 0.0f;
    private bool breathingBackwards = false;
    private Color originalColor;
    private bool animationLevelUp = false;

    [Header("LevelUp Setup")]
    [SerializeField]
    private GameObject lvlUpPanel;
    [SerializeField]
    private TranslatorUI lvlUpPanelTranslator;


    private void Start()
    {
        PlayerStats.Instance.OnLevelUp += StartLevelUpPanel;
        PlayerStats.Instance.OnEXPChange += UpdateEXPBar;

        UpdateEXPBar();
    }

    private void StartLevelUpPanel()
    {
        lvlUpPanel.SetActive(true);
        lvlUpPanelTranslator.ToTarget();

        StartCoroutine(LevelUpExpBarAnimation());
    }

    private void UpdateEXPBar()
    {
        lvlText.text = PlayerStats.Instance.GetLevel() + "lvl";
        StartCoroutine(InterpolateEXPBar());
    }

    private IEnumerator InterpolateEXPBar()
    {
        while (interpolationTime < interpolationDuration)
        {
            interpolationTime += Time.deltaTime;

            float interpolateValue = interpolationCurve.Evaluate(interpolationTime / interpolationDuration);

            expSlider.value = (PlayerStats.Instance.GetEXP() * interpolateValue) / PlayerStats.Instance.GetEXPForNextLevel();
            
            yield return null;
        }
        interpolationTime = 0.0f;

        expSlider.value = PlayerStats.Instance.GetEXP() / PlayerStats.Instance.GetEXPForNextLevel();
    }

    private IEnumerator LevelUpExpBarAnimation()
    {
        Image fillImage = expSlider.fillRect.GetComponent<Image>();
        originalColor = fillImage.color;

        animationLevelUp = true;
        breathingTime = 0.0f;
        breathingBackwards = false;

        while (animationLevelUp)
        {
            breathingTime += Time.deltaTime;

            if(!breathingBackwards)
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


            yield return null;
        }

        fillImage.color = originalColor;
    }

    public void OnUpgradeElement(int i)
    {
        PlayerStats.Instance.UpgradeElement((ElementType)i);
        UpdateEXPBar();
        animationLevelUp = false;
    }

    private void OnDestroy()
    {
        PlayerStats.Instance.OnLevelUp -= StartLevelUpPanel;
        PlayerStats.Instance.OnEXPChange -= UpdateEXPBar;

    }
}
