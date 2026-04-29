using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class SkinShop : MonoBehaviour
{
    [Header("Setup")]
    [SerializeField]
    private Image imageSkin;
    [SerializeField]
    private GameObject moneyGO;
    [SerializeField]
    private TMP_Text textMoney;
    [SerializeField]
    private TranslatorUI translator;
    [SerializeField]
    private Button prevButton;
    [SerializeField]
    private Button nextButton;
    [SerializeField]
    private Button buyButton;
    [SerializeField]
    private TMP_Text buttonText;

    private int currentPage = 0;
    private int maxPage = 3;
    private int minPage = 0;
    


    private void Awake()
    {
        prevButton.onClick.AddListener(PreviousPage);
        nextButton.onClick.AddListener(NextPage);
        buyButton.onClick.AddListener(BuyButtonPressed);
    }

    private void Start()
    {
        UpdateShop();
    }

    public void PreviousPage()
    {
        if (currentPage > minPage)
        {
            currentPage--;
            UpdatePage();
        }
    }   
    
    public void NextPage() 
    {
        if (currentPage < maxPage)
        {
            currentPage++;
            UpdatePage();
        }
    }

    private void UpdatePage()
    {
        PlayerStats player = GameManager.Instance.PlayerStats;
        

        textMoney.text = $"{SkinManager.Instance.GetAllSkins()[currentPage].Cost}";

        if (!player.UnlockedSkins.Contains(SkinManager.Instance.GetAllSkins()[currentPage].SkinType)) // Is ownted by the player?
        {
            imageSkin.color = Color.black;
            moneyGO.SetActive(true);
            buttonText.text = "buy";
        }
        else
        {
            imageSkin.color = Color.white;
            moneyGO.SetActive(false);
            buttonText.text = "chose";
        }

        Debug.Log($"Current page {currentPage}");

        imageSkin.sprite = SkinManager.Instance.GetAllSkins()[currentPage].SkinSprite;
        imageSkin.GetComponent<Animator>().runtimeAnimatorController = SkinManager.Instance.GetAllSkins()[currentPage].AnimatorControllerUI;

        if (currentPage >= maxPage)
            nextButton.interactable = false;
        else
            nextButton.interactable = true;



        if (currentPage <= minPage)
            prevButton.interactable = false;
        else
            prevButton.interactable = true;
    }

    private void UpdateShop()
    {
        maxPage = SkinManager.Instance.GetAllSkins().Count;
        currentPage = 0;
    }

    public void Close()
    {
        translator.ToOrigin();
        Camera.main.GetComponent<TileClickHandler>().HandleBoardRaycast(true);
    }

    private void BuyButtonPressed()
    {
        PlayerStats player = GameManager.Instance.PlayerStats;
        Skin currentSkin = SkinManager.Instance.GetAllSkins()[currentPage];
    
        if (!player.UnlockedSkins.Contains(currentSkin.SkinType)) // Is ownted by the player?
        {
            if (player.CanUseCurrency(CurrencyType.RUBY,currentSkin.Cost))
            {
                player.UseCurrency(CurrencyType.RUBY,currentSkin.Cost);
                player.AddNewSkin(currentSkin.SkinType);
                UpdatePage();
                //SFXManager.Instance.PlaySFXByName("Buy");
            }
            else
            {
                //SFXManager.Instance.PlaySFXByName("Error");
            }
        }
        else
        {
            player.ChangeSkin(currentSkin.SkinType);
            Close();
            //SFXManager.Instance.PlaySFXByName("Select");
        }
    }
}
