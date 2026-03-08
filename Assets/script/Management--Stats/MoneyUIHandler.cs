using TMPro;
using UnityEngine;

[RequireComponent(typeof(TMP_Text))]
public class MoneyUIHandler : MonoBehaviour
{
    private TMP_Text moneyText;


    private void Start()
    {
        moneyText = GetComponent<TMP_Text>();
        GameManager.Instance.PlayerStats.OnMoneyChange += UpdateMoneyUI; 
        UpdateMoneyUI();
    }


    private void UpdateMoneyUI()
    {
        moneyText.text = GameManager.Instance.PlayerStats.Money.ToString() + "$";
    }

    private void OnDestroy()
    {
        GameManager.Instance.PlayerStats.OnMoneyChange -= UpdateMoneyUI;
    }
}
