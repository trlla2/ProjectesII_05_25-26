using UnityEngine;
using UnityEngine.UI;

public class PlayerUIHandler : MonoBehaviour
{
    [Header("HealthBar")]
    [SerializeField]
    private Slider healthBar;

    private PlayerStats playerStats;


    private void Start()
    {
        playerStats = GetComponentInParent<PlayerStats>();

        playerStats.OnHitRecived += HitRecived;
    }

    private void HitRecived(float health)
    {
        healthBar.value = health / playerStats.GetMaxHP();
    }

    private void OnDestroy()
    {
        playerStats.OnHitRecived -= HitRecived;
    }
}
