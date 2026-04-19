using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class SellerBehaviour : MonoBehaviour
{
    [Header("Setup")]
    [SerializeField]
    private TMP_Text message;
    [SerializeField]
    private Animator sellerAnimator;
    [SerializeField]
    private float timePerCharacter = 0.05f;

    private Coroutine currentCoroutine = null;

    [Header("Messages")]
    [SerializeField]
    private List<string> enterShopMessages = new List<string>();
    [SerializeField]
    private List<string> notEnoughMoneyMessages = new List<string>();
    [SerializeField]
    private List<string> purchasedItemMessages = new List<string>();
    [SerializeField]
    private List<string> purchasedHealMessages = new List<string>();
    [SerializeField]
    private List<string> closeShopMessages = new List<string>();
    [SerializeField]
    private List<string> inventoryFullMessages = new List<string>();
    [SerializeField]
    private List<string> outOfStockMessages = new List<string>();

    private SFXController textSFX;

    private void Awake()
    {
        textSFX = GetComponent<SFXController>();
    }

    public void EnterShopReacion()
    {
        int radomIndex = Random.Range(0, enterShopMessages.Count);

        DisplayMessage(enterShopMessages[radomIndex]);
    }
    public void NotEnoughMoneyReaction()
    {
        int radomIndex = Random.Range(0, notEnoughMoneyMessages.Count);

        DisplayMessage(notEnoughMoneyMessages[radomIndex]);
    }
    public void PurchasedItemReaction()
    {
        int radomIndex = Random.Range(0, purchasedItemMessages.Count);

        DisplayMessage(purchasedItemMessages[radomIndex]);

        sellerAnimator.SetTrigger("Bought");
    }

    public void PurchasedHealReaction()
    {
        int radomIndex = Random.Range(0, purchasedHealMessages.Count);

        DisplayMessage(purchasedHealMessages[radomIndex]);

        sellerAnimator.SetTrigger("Bought");
    }

    public void CloseShopReaction()
    {
        int radomIndex = Random.Range(0, closeShopMessages.Count);

        DisplayMessage(closeShopMessages[radomIndex]);
    }
    public void InventoryFullReaction()
    {
        int radomIndex = Random.Range(0, inventoryFullMessages.Count);

        DisplayMessage(inventoryFullMessages[radomIndex]);
    }

    public void OutOfStockReaction()
    {
        int radomIndex = Random.Range(0, outOfStockMessages.Count);

        DisplayMessage(outOfStockMessages[radomIndex]);
    }


    private void DisplayMessage(string dialogue)
    {
        if (currentCoroutine != null)
            StopCoroutine(currentCoroutine);

        currentCoroutine = StartCoroutine(ShowText(dialogue));
    }

    private IEnumerator ShowText(string dialogue)
    {
        message.text = dialogue;
        message.maxVisibleCharacters = 0;

        for (int i = 0; i <= dialogue.Length; i++)
        {
            message.maxVisibleCharacters = i;
            textSFX.PlayRandomSFX();
            yield return new WaitForSeconds(timePerCharacter);
        }

        currentCoroutine = null;
    }
}
