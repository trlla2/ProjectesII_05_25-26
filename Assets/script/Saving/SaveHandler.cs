using UnityEngine;

public class SaveHandler : MonoBehaviour
{
    private void Start()
    {
        PlayerStats stats = GameManager.Instance.PlayerStats;

        stats.OnCurrencyChange += SaveCurrency;
        stats.OnBattlePassLevelUp += SaveBattlePassLevel;
        stats.OnSkinChange += SaveCurrentSkin;
        stats.OnUnlockedSkinsChange += SaveUnlockedSkins;
        stats.OnKnownSpellChange += SaveKnownSpells;
    }

    private void SaveCurrency(CurrencyType currencyType, int amount)
    {
        SaveManager.Instance.SaveCurrency(currencyType);
    }

    private void SaveCurrentSkin()
    {
        SaveManager.Instance.SaveCurrentSkin();
    }

    private void SaveUnlockedSkins()
    {
        SaveManager.Instance.SaveUnlockedSkins();
    }

    private void SaveKnownSpells()
    {
        SaveManager.Instance.SaveKnownSpells();
    }
    private void SaveBattlePassLevel()
    {
        SaveManager.Instance.SaveBattlePassLevel();
    }

    private void OnDestroy()
    {
        PlayerStats stats = GameManager.Instance.PlayerStats;

        stats.OnCurrencyChange -= SaveCurrency;
        stats.OnBattlePassLevelUp -= SaveBattlePassLevel;
        stats.OnSkinChange -= SaveCurrentSkin;
        stats.OnUnlockedSkinsChange -= SaveUnlockedSkins;
        stats.OnKnownSpellChange -= SaveKnownSpells;
    }
}
