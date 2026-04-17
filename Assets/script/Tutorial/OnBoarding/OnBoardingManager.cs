using NUnit.Framework;
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

        panelList.MarkPanelAsExplained(key);
        Time.timeScale = 0f;

        OnboardingPanel panelToOpen = GetPanelWithKey(key);
        panelToOpen.Show(() =>
        {
            Time.timeScale = 1f;
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

}
