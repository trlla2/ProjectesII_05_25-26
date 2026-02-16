using TMPro;
using UnityEngine;

[RequireComponent(typeof(TMP_Text))]
public class MoneyUIHandler : MonoBehaviour
{
    private TMP_Text moneyText;


    private void Start()
    {
        moneyText = GetComponent<TMP_Text>();
        G.GM.PlayerStats.OnMoneyChange += UpdateMoneyUI; 
        UpdateMoneyUI();
    }


    private void UpdateMoneyUI()
    {
        moneyText.text = G.GM.PlayerStats.Money.ToString() + "$";
    }

    private void OnDestroy()
    {
        G.GM.PlayerStats.OnMoneyChange -= UpdateMoneyUI;
    }
}
