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

    private int currentPage = 1;
    private int maxPage = 3;
    private int minPage = 1;
    [SerializeField]
    private List<int> prices = new List<int>(3);


    private void Awake()
    {
        prevButton.onClick.AddListener(PreviousPage);
        nextButton.onClick.AddListener(NextPage);
        buyButton.onClick.AddListener(Close);
    }

    private void OnEnable()
    {
        translator.ToTarget();
        currentPage = 1;
        UpdateVisuals();
        Camera.main.GetComponent<TileClickHandler>().HandleBoardRaycast(true);
    }

    public void PreviousPage()
    {
        if (currentPage > minPage)
        {
            currentPage--;
            UpdateVisuals();
        }
    }   
    
    public void NextPage() 
    {
        if (currentPage < maxPage)
        {
            currentPage++;
            UpdateVisuals();
        }
    }

    private void UpdateVisuals()
    {
        textMoney.text = $"{prices[currentPage - 1 ]}";

        if (currentPage > minPage)
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

        if (currentPage >= maxPage)
            nextButton.interactable = false;
        else
            nextButton.interactable = true;



        if (currentPage <= minPage)
            prevButton.interactable = false;
        else
            prevButton.interactable = true;
    }

    private void Close()
    {
        if (currentPage == minPage)
        {
            translator.ToOrigin();
            Camera.main.GetComponent<TileClickHandler>().HandleBoardRaycast(true);
        }
    }
}
