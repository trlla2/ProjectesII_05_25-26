using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.UI;

public class PlayerUIHandler : MonoBehaviour
{
    [Header("HealthBar")]
    [SerializeField]
    private Slider healthBar;
    [Header("EvadedText")]
    [SerializeField]
    private TranslatorUI evadedTextTranslator;

    private void Start()
    {
        G.GM.CombatHandler.OnPlayerHitRecived += UpdateHealBar;
        G.GM.CombatHandler.OnPlayerHealed += UpdateHealBar;
        G.GM.CombatHandler.OnPlayerHitEvaded += HitEvaded;

        UpdateHealBar();
    }

    private void UpdateHealBar()
    {
        healthBar.value = G.GM.PlayerStats.GetHPPercentage();
    }

    private void HitEvaded()
    {
        evadedTextTranslator.ToTarget();
    }

    private void OnDestroy()
    {
        G.GM.CombatHandler.OnPlayerHitRecived -= UpdateHealBar;
        G.GM.CombatHandler.OnPlayerHealed -= UpdateHealBar;
        G.GM.CombatHandler.OnPlayerHitEvaded -= HitEvaded;

    }
}
