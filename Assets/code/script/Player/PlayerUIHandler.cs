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
        GameManager.Instance.CombatHandler.OnPlayerHitRecived += UpdateHealBar;
        GameManager.Instance.CombatHandler.OnPlayerHealed += UpdateHealBar;
        GameManager.Instance.CombatHandler.OnPlayerHitEvaded += HitEvaded;

        UpdateHealBar();
    }

    private void UpdateHealBar()
    {
        healthBar.value = GameManager.Instance.PlayerStats.GetHPPercentage();
    }

    private void HitEvaded()
    {
        evadedTextTranslator.ToTarget();
    }

    private void OnDestroy()
    {
        GameManager.Instance.CombatHandler.OnPlayerHitRecived -= UpdateHealBar;
        GameManager.Instance.CombatHandler.OnPlayerHealed -= UpdateHealBar;
        GameManager.Instance.CombatHandler.OnPlayerHitEvaded -= HitEvaded;

    }
}
