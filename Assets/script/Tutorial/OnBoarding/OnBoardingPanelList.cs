using System.Collections.Generic;
using UnityEngine;

public enum OnBoardingKey
{
    DEBUG,
    USABLE_ITEMS, //
    ITEMS_COMBINATION, // elementos dobles
    CHEST_OF_EGRESS, //
    SPELLBOOK, //
    SPELLBOOK_DEDICO,
    SHOP, // 
    BOMB_ENEMY, // 
    HIDDEN_ENEMY_SELECT, //
    HIDDEN_ENEMY_CAST,
    BOSS_STAGE1_START, //
    BOSS_STAGE1_ORBCHARGED, //
    BUFFS_N_DEBUFFS, //
    LVL_UP_ELEMENTS, // falta integrarlo
    HALL_OF_EGRESS_INTRO,
    HALL_OF_EGRESS_GOTOSPELLBOOK,
    HALL_OF_EGRESS_GOTOSPELLBOOK_DEDICO,
    BLOCKER_ENEMY,
    HALL_OF_EGRESS_GO_INTO_DUNGEON,
    HALL_OF_EGRESS_GO_INTO_DUNGEON_DEDICO,
    FIRST_ITEM_PICKUP,


}

[System.Serializable]
public struct OnBoardingPanelData
{
    public OnBoardingKey key;
    public GameObject panelPrefab;
    public bool isExplained;
}

[CreateAssetMenu(menuName = "Game/OnBoarding/OnBoardingPanelsList")]
public class OnBoardingPanelList : ScriptableObject
{
    [SerializeField] private List<OnBoardingPanelData> onBoardingFeatures;

    private const string PlayerPrefsOnboardingKey = "HasCompletedOnboarding_";

    public GameObject GetPanelPrefab(OnBoardingKey key)
    {
        foreach (var feature in onBoardingFeatures)
        {
            if (feature.key == key && !feature.isExplained)
                return feature.panelPrefab;
        }
        Debug.LogWarning($"OnBoardingPanelList: No panel found for key {key} or key isExplained");
        return null;
    }

    public void MarkPanelAsExplained(OnBoardingKey key)
    {
        for (int i = 0; i < onBoardingFeatures.Count; i++)
        {
            if (onBoardingFeatures[i].key == key)
            {
                onBoardingFeatures[i] = new OnBoardingPanelData
                {
                    key = onBoardingFeatures[i].key,
                    panelPrefab = onBoardingFeatures[i].panelPrefab,
                    isExplained = true
                };
                PlayerPrefs.SetInt(PlayerPrefsOnboardingKey + key, 1);
                PlayerPrefs.Save();
                return;
            }
        }
    }

    public bool IsPanelExplained(OnBoardingKey key)
    {
        foreach (var feature in onBoardingFeatures)
        {
            if (feature.key == key)
                return feature.isExplained /*|| PlayerPrefs.GetInt(PlayerPrefsOnboardingKey + key, 0) == 1*/;
        }
        Debug.LogWarning($"OnBoardingPanelList: No panel found for key {key}");
        return false;
    }

    public List<OnboardingPanel> LoadPrefsData(Transform panelsParent)
    {
        List<OnboardingPanel> notSeenPanels = new List<OnboardingPanel>();
        for (int i = 0; i < onBoardingFeatures.Count; i++)
        {
            bool isExplained = PlayerPrefs.GetInt(PlayerPrefsOnboardingKey + onBoardingFeatures[i].key, 0) == 1;
            
            onBoardingFeatures[i] = new OnBoardingPanelData
            {
                key = onBoardingFeatures[i].key,
                panelPrefab = onBoardingFeatures[i].panelPrefab,
                isExplained = isExplained
            };
            //if (!isExplained)
            {
                GameObject panelInstance = Instantiate(onBoardingFeatures[i].panelPrefab, panelsParent);
                panelInstance.SetActive(false);
                OnboardingPanel newPanel = panelInstance.GetComponent<OnboardingPanel>();
                newPanel.Initialize(onBoardingFeatures[i].key);
                notSeenPanels.Add(newPanel);
            }
        }
        return notSeenPanels;
    }

    public void ResetAllSavedData()
    {
        for (int i = 0; i < onBoardingFeatures.Count; i++)
        {
            onBoardingFeatures[i] = new OnBoardingPanelData
            {
                key = onBoardingFeatures[i].key,
                panelPrefab = onBoardingFeatures[i].panelPrefab,
                isExplained = false
            };
            PlayerPrefs.DeleteKey(PlayerPrefsOnboardingKey + onBoardingFeatures[i].key);
        }
        PlayerPrefs.Save();
    }
}
