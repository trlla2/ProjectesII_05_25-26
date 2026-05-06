using System.Collections.Generic;
using Unity.VisualScripting.Antlr3.Runtime.Misc;
using UnityEngine;

public class SaveManager : MonoBehaviour
{
    public static SaveManager Instance { get; private set; }

    private void Awake()
    {
        if (Instance != null)
        {
            Destroy(this.gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(this.gameObject);
    }


    public void SavePlayerStats()
    {
        foreach (CurrencyType currency in System.Enum.GetValues(typeof(CurrencyType)))
        {
            SaveCurrency(currency);
        }

        SaveBattlePassLevel();

        SaveKnownSpells();

        SaveUnlockedSkins();

        SaveCurrentSkin();
    }

    public void SaveCurrency(CurrencyType type)
    {
        PlayerStats stats = GameManager.Instance.PlayerStats;
        PlayerPrefs.SetInt("Currency_" + type.ToString(), stats.GetCurrency(type));
        PlayerPrefs.Save();
    }

    public void SaveBattlePassLevel()
    {
        PlayerStats stats = GameManager.Instance.PlayerStats;
        PlayerPrefs.SetInt("BattlePassLevel", stats.BattlePassLevel);
        PlayerPrefs.Save();
    }

    public void SaveKnownSpells()
    {
        PlayerStats stats = GameManager.Instance.PlayerStats;
        List<string> spellKeys = new List<string>();
        foreach (Spell spell in stats.KnownSpells)
        {
            spellKeys.Add(spell.id.ToString());
        }
        PlayerPrefs.SetString("KnownSpells", string.Join(",", spellKeys));
        PlayerPrefs.Save();
    }

    public void SaveUnlockedSkins()
    {
        PlayerStats stats = GameManager.Instance.PlayerStats;
        List<string> unlockedSkinsList = new List<string>();
        foreach (SkinType skin in stats.UnlockedSkins)
        {
            unlockedSkinsList.Add(skin.ToString());
        }
        PlayerPrefs.SetString("UnlockedSkins", string.Join(",", unlockedSkinsList));
        PlayerPrefs.Save();
    }

    public void SaveCurrentSkin()
    {
        PlayerStats stats = GameManager.Instance.PlayerStats;
        PlayerPrefs.SetString("CurrentSkin", stats.CurrentSkin.ToString());
        PlayerPrefs.Save();
    }

    public void LoadPlayerStats()
    {
        if (!PlayerPrefs.HasKey("CurrentSkin")) return;

        PlayerStats stats = GameManager.Instance.PlayerStats;

        foreach (CurrencyType currency in System.Enum.GetValues(typeof(CurrencyType)))
        {
            int currentAmount = stats.GetCurrency(currency);
            stats.UseCurrency(currency, currentAmount);
            stats.AddCurrency(currency, PlayerPrefs.GetInt("Currency_" + currency.ToString(), 0));
        }

        int savedBattlePassLevel = PlayerPrefs.GetInt("BattlePassLevel", 0);
        while (stats.BattlePassLevel < savedBattlePassLevel)
        {
            stats.AddBattlePassLevel();
        }

        string knownSpellsString = PlayerPrefs.GetString("KnownSpells", "");
        if (!string.IsNullOrEmpty(knownSpellsString))
        {
            foreach (string key in knownSpellsString.Split(','))
            {
                Spell spell = Resources.Load<Spell>(key);
                if (spell != null)
                {
                    stats.AddKnownSpell(spell);
                }
            }
        }

        string unlockedSkinsString = PlayerPrefs.GetString("UnlockedSkins", "");
        if (!string.IsNullOrEmpty(unlockedSkinsString))
        {
            foreach (string skinKey in unlockedSkinsString.Split(','))
            {
                stats.AddNewSkin((SkinType)System.Enum.Parse(typeof(SkinType), skinKey));
            }
        }

        string currentSkinString = PlayerPrefs.GetString("CurrentSkin", "Default");
        stats.ChangeSkin((SkinType)System.Enum.Parse(typeof(SkinType), currentSkinString));
    }

    public void ResetSavedStats()
    {
        foreach (CurrencyType currency in System.Enum.GetValues(typeof(CurrencyType)))
        {
            PlayerPrefs.DeleteKey("Currency_" + currency.ToString());
        }

        PlayerPrefs.DeleteKey("BattlePassLevel");
        PlayerPrefs.DeleteKey("KnownSpells");
        PlayerPrefs.DeleteKey("UnlockedSkins");
        PlayerPrefs.DeleteKey("CurrentSkin");
        PlayerPrefs.DeleteKey("TutorialCompleted");
        PlayerPrefs.DeleteKey("UsedSpells");

        PlayerPrefs.Save();
    }

    public bool TutorialCompleted
    {
        get 
        {
            if (!PlayerPrefs.HasKey("TutorialCompleted")) return false;
            return PlayerPrefs.GetInt("TutorialCompleted", 0) == 1; 
        }
        set { PlayerPrefs.SetInt("TutorialCompleted", value ? 1 : 0); PlayerPrefs.Save(); }
    }

    public bool HasUsedSpell(Spell s)
    {
        string UsedSpellsString = PlayerPrefs.GetString("UsedSpells", "");
        
        if(!string.IsNullOrEmpty(UsedSpellsString))
        {
            foreach (string key in UsedSpellsString.Split(','))
            {
                if (key == s.id.ToString())
                {
                    return true;
                }
            }
        }

        return false;
    }

    public void AddUsedSpell(Spell s)
    {
        string UsedSpellsString = PlayerPrefs.GetString("UsedSpells", "");
        List<string> usedSpellsList = new List<string>(UsedSpellsString.Split(','));
    
        if (!usedSpellsList.Contains(s.id.ToString()))
        {
            usedSpellsList.Add(s.id.ToString());
            PlayerPrefs.SetString("UsedSpells", string.Join(",", usedSpellsList));
            PlayerPrefs.Save();
        }
    }

}
