using DG.Tweening;
using System;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class ElementLevelUpButton : MonoBehaviour
{
    [Header("Bar")]
    [SerializeField]
    private GameObject upgradesBar;
    [SerializeField]
    private float interpolationDuration = 1.0f;
    [SerializeField]
    private AnimationCurve interpolationCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);

    [Header("Level Up Animation")]
    [SerializeField]
    private float punchStrength = 0.4f;
    [SerializeField]
    private float punchDuration = 0.4f;
    [SerializeField]
    private int punchVibrato = 5;

    private float interpolationTime = 0.0f;
    private float currentPhaseDuration;
    private bool isInterpolating = false;

    private float initialScaleX;
    private float targetScaleX;

    // when a level-up happens, we animate in two phases:
    // phase 1: bar goes to 1.0, phase 2: bar goes to new position
    private bool levelUpPhase1Active = false;
    private bool levelUpPhase2Active = false;
    private float levelUpFinalTargetScaleX;

    [Header("ElementSetup")]
    [SerializeField]
    private TMP_Text lvl;
    [SerializeField]
    private Image elementImage;
    [SerializeField]
    private Element currentElement;

    [Header("VFX")]
    [SerializeField]
    private ParticleSystem levelUpParticlePrefab;
    private Camera worldCamera;

    public UnityEvent OnBarReach;
    public UnityEvent OnElementLevelUp;
    public UnityEvent OnLevelUpAnimationComplete;

    private int oldLevel;
    private Vector3 lastClickWorldPosition;

    private void Start()
    {
        UpdateVisuals(true);

        if (worldCamera == null)
            worldCamera = Camera.main;
    }

    private void OnEnable()
    {
        oldLevel = GameManager.Instance.PlayerStats.GetElementLevel(currentElement.Type);

        UpdateVisuals();
    }

    private void Update()
    {
        UpdateBar();
    }

    public void RegisterClick()
    {
        if (worldCamera == null) return;

        Vector3 screenPos = Input.mousePosition;
        screenPos.z = worldCamera.nearClipPlane;
        lastClickWorldPosition = worldCamera.ScreenToWorldPoint(screenPos);
    }

    private void SpawnLevelUpParticles(Vector3 worldPosition)
    {
        if (levelUpParticlePrefab == null) return;

        ParticleSystem vfx = Instantiate(levelUpParticlePrefab, worldPosition, Quaternion.identity);
        vfx.Play();

        Destroy(vfx.gameObject, vfx.main.duration + vfx.main.startLifetime.constantMax);
    }

    public void UpdateVisuals(bool instant = false)
    {
        elementImage.sprite = currentElement.Sprite;
        elementImage.color = currentElement.Color;
        lvl.color = currentElement.Color;
        upgradesBar.GetComponent<Image>().color = currentElement.Color;

        bool leveledUp = oldLevel != GameManager.Instance.PlayerStats.GetElementLevel(currentElement.Type);

        float upgradesRequired = (float)Math.Pow(2, GameManager.Instance.PlayerStats.GetElementLevel(currentElement.Type) - 1);
        float currentUpgrades = GameManager.Instance.PlayerStats.GetElementUpgradeLevel(currentElement.Type);

        // +1 to both so the bar is never empty (min is 1/(max+1) instead of 0/max)
        float newTargetScaleX = (upgradesRequired > 0) ? ((currentUpgrades + 1) / (upgradesRequired + 1)) : 0f;

        if (instant)
        {
            lvl.text = $"Lvl {GameManager.Instance.PlayerStats.GetElementLevel(currentElement.Type)}";
            upgradesBar.transform.localScale = new Vector3(newTargetScaleX, 1, 1);
            isInterpolating = false;
            levelUpPhase1Active = false;
        }
        else if (leveledUp)
        {
            OnElementLevelUp.Invoke();

            // phase 1: fill bar to 1.0 first, then update text and go to new position
            levelUpPhase1Active = true;
            levelUpFinalTargetScaleX = newTargetScaleX;

            initialScaleX = upgradesBar.transform.localScale.x;
            targetScaleX = 1.0f;
            interpolationTime = 0.0f;
            currentPhaseDuration = interpolationDuration * 0.5f;
            isInterpolating = true;
        }
        else
        {
            lvl.text = $"Lvl {GameManager.Instance.PlayerStats.GetElementLevel(currentElement.Type)}";

            initialScaleX = upgradesBar.transform.localScale.x;
            targetScaleX = newTargetScaleX;
            interpolationTime = 0.0f;
            currentPhaseDuration = interpolationDuration;
            isInterpolating = true;
        }
    }

    private void UpdateBar()
    {
        if (!isInterpolating) return;

        interpolationTime += Time.deltaTime;

        float timeRatio = Mathf.Clamp01(interpolationTime / currentPhaseDuration);
        float curveValue = interpolationCurve.Evaluate(timeRatio);
        float currentScaleX = Mathf.LerpUnclamped(initialScaleX, targetScaleX, curveValue);

        upgradesBar.transform.localScale = new Vector3(currentScaleX, 1, 1);

        if (interpolationTime >= currentPhaseDuration)
        {
            isInterpolating = false;
            upgradesBar.transform.localScale = new Vector3(targetScaleX, 1, 1);

            if (levelUpPhase1Active)
            {
                FinishLevelUpPhase1();
            }
            else if (levelUpPhase2Active)
            {
                levelUpPhase2Active = false;
                OnBarReach.Invoke();
                OnLevelUpAnimationComplete.Invoke();
            }
            else
            {
                OnBarReach.Invoke();
            }
        }
    }

    private void FinishLevelUpPhase1()
    {
        levelUpPhase1Active = false;

        // update level text and punch scale it
        lvl.text = $"Lvl {GameManager.Instance.PlayerStats.GetElementLevel(currentElement.Type)}";
        lvl.transform.DOKill();
        lvl.transform.DOPunchScale(Vector3.one * punchStrength, punchDuration * 0.5f, punchVibrato);

        oldLevel = GameManager.Instance.PlayerStats.GetElementLevel(currentElement.Type);

        SpawnLevelUpParticles(lastClickWorldPosition);

        // phase 2: animate bar from 1.0 to new position
        levelUpPhase2Active = true;
        initialScaleX = 1.0f;
        targetScaleX = levelUpFinalTargetScaleX;
        interpolationTime = 0.0f;
        currentPhaseDuration = interpolationDuration * 0.5f;
        isInterpolating = true;
    }
}
