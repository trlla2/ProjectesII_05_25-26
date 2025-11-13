using TMPro;
using UnityEngine;

[RequireComponent(typeof(TMP_Text))]
public class MoneyUIHandler : MonoBehaviour
{
    private TMP_Text moneyText;


    private void Start()
    {
        moneyText = GetComponent<TMP_Text>();
        PlayerStats.Instance.OnMoneyChange += UpdateMoneyUI;
        UpdateMoneyUI();
    }


    private void UpdateMoneyUI()
    {
        moneyText.text = PlayerStats.Instance.GetMoney().ToString() + "$";
    }

    private void OnDestroy()
    {
        PlayerStats.Instance.OnMoneyChange -= UpdateMoneyUI;
    }
}
