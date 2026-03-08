using UnityEngine;
using UnityEngine.UI;

public class PlayerHealthBar : MonoBehaviour
{
    private Slider healthBar;

    private void Awake() { healthBar = GetComponent<Slider>(); }

    private void OnEnable()
    {
        healthBar.value = GameManager.Instance.PlayerStats.GetHPPercentage();
    }
}
