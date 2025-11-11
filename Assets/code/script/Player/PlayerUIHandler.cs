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
        PlayerStats.Instance.OnHitRecived += HitRecived;
        PlayerStats.Instance.OnHitEvaded += HitEvaded;
    }

    private void HitRecived(float health)
    {
        healthBar.value = health / PlayerStats.Instance.GetMaxHP();
    }

    private void HitEvaded()
    {
        evadedTextTranslator.ToTarget();
    }

    private void OnDestroy()
    {
        PlayerStats.Instance.OnHitRecived -= HitRecived;
    }
}
