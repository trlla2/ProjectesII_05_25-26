using TMPro;
using UnityEngine;

[RequireComponent(typeof(TMP_Text))]
public class MoneyUIHandler : MonoBehaviour
{
    private TMP_Text moneyText;

    [Header("Setup")]
    public bool useGameManager = true;

    [Header("Fade")]
    [SerializeField]
    private bool showAlways = true;
    [SerializeField]
    private CanvasGroupFader parentFader;
    [SerializeField]
    private float duration = 3f;
    private float time = 0;
    private bool isFading = false;

    [Header("Money Added")]
    [SerializeField]
    private GameObject moneyAddedGO;
   


    private void Start()
    {
        moneyText = GetComponent<TMP_Text>();
        if(useGameManager)
            GameManager.Instance.PlayerStats.OnCurrencyChange += UpdateMoneyUI;

        UpdateMoneyUI(CurrencyType.GOLD, 0);


        if (!showAlways)
        {
            time = duration;
            isFading = false;
            parentFader.ResetToOrigin();
        }

    }

    public void StayVisible()
    {
        showAlways = true;
        isFading = false;
        time = 0;
        parentFader.ToTarget();
    }

    public void UnShow()
    {
        time = duration;
        isFading = false;
        parentFader.ToOrigin();
    }

    public void UpdateMoneyUI(CurrencyType type, int amount)
    {
        if(!showAlways && amount > 0)
        {
            time = 0;
            parentFader.ToTarget();
            isFading = true;
            if (SFXManager.Instance != null)
                SFXManager.Instance.PlaySFXByName("GetCoins");
        }

        if (moneyAddedGO != null && amount > 0)
        {
            moneyAddedGO.GetComponent<TMP_Text>().text = $"+{amount}";
            moneyAddedGO.GetComponent<TranslatorUI>().ToTarget();
            if (SFXManager.Instance != null)
                SFXManager.Instance.PlaySFXByName("GetCoins");
        }

        if (GameManager.Instance != null)
            moneyText.text = GameManager.Instance.PlayerStats.GetCurrency(CurrencyType.GOLD).ToString();
    }


    public void UpdateMoneyUI()
    {
        if (GameManager.Instance == null) return;

        if (!showAlways)
        {
            if (SFXManager.Instance != null)
                SFXManager.Instance.PlaySFXByName("GetCoins");
            time = 0;
            parentFader.ToTarget();
            isFading = true;

        }
        moneyText.text = GameManager.Instance.PlayerStats.GetCurrency(CurrencyType.GOLD).ToString();
    }

    private void Update()
    {
        if(!showAlways && isFading)
        {
            if (time < duration)
                time += Time.deltaTime;
            else
            {
                isFading = false;
                parentFader.ToOrigin();
            }
        }
    }

    private void OnDestroy()
    {
        if (useGameManager && GameManager.Instance != null)
            GameManager.Instance.PlayerStats.OnCurrencyChange -= UpdateMoneyUI;
    }
}
