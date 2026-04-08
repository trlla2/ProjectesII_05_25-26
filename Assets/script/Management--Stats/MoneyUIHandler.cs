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
   


    private void Start()
    {
        moneyText = GetComponent<TMP_Text>();
        if(useGameManager)
            GameManager.Instance.PlayerStats.OnMoneyChange += UpdateMoneyUI;

        if (showAlways)
            UpdateMoneyUI();
        else
            time = duration;
    }


    public void UpdateMoneyUI()
    {
        Debug.Log("Hola???");

        if(!showAlways)
        {
            time = 0;
            parentFader.ToTarget();
            isFading = true;
        }

        moneyText.text = GameManager.Instance.PlayerStats.Money.ToString();
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
        if (useGameManager)
            GameManager.Instance.PlayerStats.OnMoneyChange -= UpdateMoneyUI;
    }
}
