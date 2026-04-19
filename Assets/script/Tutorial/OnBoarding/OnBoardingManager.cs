using NUnit.Framework;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OnBoardingManager : MonoBehaviour
{
    public static OnBoardingManager Instance { get; private set; }

    [SerializeField] private OnBoardingPanelList panelList;
    [SerializeField] private List<OnboardingPanel> instancedPanels;

    private OnboardingPanel currentPanel;
    public void RegisterPanel(OnboardingPanel _panel) => currentPanel = _panel;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Awake()
    {
        if (Instance != null) { Destroy(this.gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(this.gameObject);

        ResetOnBoardingDataFromPlayerPrefs(); // DEBUG ONLY!!! // remove for build
        instancedPanels = panelList.LoadPrefsData(this.transform);
    }

    //private void Start()
    //{
    //    OnBoardingManager.Instance.TriggerIfNew(OnBoardingKey.USABLE_ITEMS);
    //}

    public void TriggerIfNew(OnBoardingKey key)
    {
        if (panelList.IsPanelExplained(key)) return;
        if (currentPanel == null) return;

        
        Time.timeScale = 0f;

        OnboardingPanel panelToOpen = GetPanelWithKey(key);
        panelToOpen.Show(() =>
        {
            Time.timeScale = 1f;
            panelList.MarkPanelAsExplained(key);
        });
    }

    private OnboardingPanel GetPanelWithKey(OnBoardingKey key)
    {
        return instancedPanels.Find(panel => panel._key == key);
    }

    // debug
    private void ResetOnBoardingDataFromPlayerPrefs()
    {
        panelList.ResetAllSavedData();
    }

    public void TriggerIfNewWithDelay(OnBoardingKey key)
    {
        StartCoroutine(ShowOnBoardingWithKey(key));
    }

    private IEnumerator ShowOnBoardingWithKey(OnBoardingKey key)
    {
        yield return new WaitForSecondsRealtime(0.5f);
        OnBoardingManager.Instance.TriggerIfNew(key);
    }

    public bool IsPanelExplainedWithKey(OnBoardingKey key)
    {
        return panelList.IsPanelExplained(key);
    }

    public void MarkPanelAsExplained(OnBoardingKey key)
    {
        panelList.MarkPanelAsExplained(key);
    }
}
