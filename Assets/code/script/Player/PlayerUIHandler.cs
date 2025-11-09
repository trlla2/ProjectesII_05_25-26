using UnityEngine;
using UnityEngine.UI;

public class PlayerUIHandler : MonoBehaviour
{
    [Header("HealthBar")]
    [SerializeField]
    private Slider healthBar;

    private void Start()
    {
        PlayerStats.Instance.OnHitRecived += HitRecived;
    }

    private void HitRecived(float health)
    {
        healthBar.value = health / PlayerStats.Instance.GetMaxHP();
    }

    private void OnDestroy()
    {
        PlayerStats.Instance.OnHitRecived -= HitRecived;
    }
}
