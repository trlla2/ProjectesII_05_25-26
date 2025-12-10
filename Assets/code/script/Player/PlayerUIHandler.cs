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
        GameManager.Instance.OnPlayerHitRecived += HitRecived;
        GameManager.Instance.OnPlayerHitEvaded += HitEvaded;
        HitRecived(GameManager.Instance.GetPlayerHP());
    }

    private void HitRecived(float health)
    {
        healthBar.value = health / GameManager.Instance.GetPlayerMaxHP();
    }

    private void HitEvaded()
    {
        evadedTextTranslator.ToTarget();
    }

    private void OnDestroy()
    {
        GameManager.Instance.OnPlayerHitRecived -= HitRecived;
        GameManager.Instance.OnPlayerHitEvaded -= HitEvaded;

    }
}
