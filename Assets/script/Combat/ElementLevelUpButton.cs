using NUnit.Framework;
using System;
using System.Runtime.CompilerServices;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.U2D;
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

    private float interpolationTime = 0.0f;
    private bool isInterpolating = false;

    private float initialScaleX;
    private float targetScaleX;

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
        lvl.text = $"Lvl {GameManager.Instance.PlayerStats.GetElementLevel(currentElement.Type)}";
        upgradesBar.GetComponent<Image>().color = currentElement.Color;

        float upgradesRequired = (float)Math.Pow(2, GameManager.Instance.PlayerStats.GetElementLevel(currentElement.Type) - 1);
        float currentUpgrades = GameManager.Instance.PlayerStats.GetElementUpgradeLevel(currentElement.Type);

        targetScaleX = (upgradesRequired > 0) ? (currentUpgrades / upgradesRequired) : 0f;

        if (instant)
        {
            upgradesBar.transform.localScale = new Vector3(targetScaleX, 1, 1);
            isInterpolating = false;
        }
        else
        {
            initialScaleX = upgradesBar.transform.localScale.x;
            interpolationTime = 0.0f;
            isInterpolating = true;
        }
    }

    private void UpdateBar()
    {
        if (!isInterpolating) return;

        interpolationTime += Time.deltaTime;

        float timeRatio = Mathf.Clamp01(interpolationTime / interpolationDuration);

        float curveValue = interpolationCurve.Evaluate(timeRatio);

        float currentScaleX = Mathf.LerpUnclamped(initialScaleX, targetScaleX, curveValue);

        upgradesBar.transform.localScale = new Vector3(currentScaleX, 1, 1);

        if (interpolationTime >= interpolationDuration)
        {
            isInterpolating = false;
            upgradesBar.transform.localScale = new Vector3(targetScaleX, 1, 1);
            OnBarReach.Invoke();

            if (oldLevel != GameManager.Instance.PlayerStats.GetElementLevel(currentElement.Type))
            {
                SpawnLevelUpParticles(lastClickWorldPosition);
                OnElementLevelUp.Invoke();
            }
        }
    }
}