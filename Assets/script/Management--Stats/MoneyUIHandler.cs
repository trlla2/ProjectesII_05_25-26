using TMPro;
using UnityEngine;

[RequireComponent(typeof(TMP_Text))]
public class MoneyUIHandler : MonoBehaviour
{
    private TMP_Text moneyText;
    
    public bool useGameManager = true;

    private void Start()
    {
        moneyText = GetComponent<TMP_Text>();
        if(useGameManager)
            GameManager.Instance.PlayerStats.OnMoneyChange += UpdateMoneyUI; 
        UpdateMoneyUI();
    }


    public void UpdateMoneyUI()
    {
        moneyText.text = GameManager.Instance.PlayerStats.Money.ToString();
    }

    private void OnDestroy()
    {
        if (useGameManager)
            GameManager.Instance.PlayerStats.OnMoneyChange -= UpdateMoneyUI;
    }
}
