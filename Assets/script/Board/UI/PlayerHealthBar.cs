using UnityEngine;
using UnityEngine.UI;

public class PlayerHealthBar : MonoBehaviour
{
    private Slider healthBar;

    private void Awake() { healthBar = GetComponent<Slider>(); }

    private void OnEnable()
    {
        UpdateHealthBar();
    }
    public void UpdateHealthBar()
    {
        healthBar.value = GameManager.Instance.PlayerStats.GetHPPercentage();
    }
}
