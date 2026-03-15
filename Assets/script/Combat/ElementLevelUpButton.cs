using NUnit.Framework;
using System;
using TMPro;
using UnityEngine;
using UnityEngine.U2D;
using UnityEngine.UI;

public class ElementLevelUpButton : MonoBehaviour
{
    [Header("Upgrade Slots")]
    [SerializeField]
    private Transform upgradesParent;
    [SerializeField]
    private GameObject elementSlotPrefab;
    [SerializeField]
    private Sprite greenSlot;
    [SerializeField]
    private Sprite yellowSlot;
    [SerializeField]
    private Sprite redSlot;
    [SerializeField]
    private Sprite blueSlot;

    [Header("ELementSetup")]
    [SerializeField]
    private TMP_Text lvl;
    [SerializeField]
    private Image elementImage;



    [SerializeField]
    private Element currentElement;


    public void OnEnable()
    {
        UpdateVisuals();
    }

    public void UpdateVisuals()
    {
        elementImage.sprite = currentElement.Sprite;
        lvl.color = currentElement.Color;
        lvl.text = $"Lvl {GameManager.Instance.PlayerStats.GetElementLevel(currentElement.Type)}";

        int upgradesRequired = (int)Math.Pow(2, GameManager.Instance.PlayerStats.GetElementLevel(currentElement.Type) - 1);

        foreach (Transform child in transform)
        {
            Destroy(child.gameObject);
        }

        for (int i = 0; i <= upgradesRequired; i++)
        {
            GameObject temp = Instantiate(elementSlotPrefab, upgradesParent);

            if (i < GameManager.Instance.PlayerStats.GetElementUpgradeLevel(currentElement.Type))
            {
                switch (currentElement.Type)
                {
                    case ElementType.Red:
                        temp.transform.GetChild(0).GetComponent<Image>().sprite = redSlot;
                        break;
                    case ElementType.Blue:
                        temp.transform.GetChild(0).GetComponent<Image>().sprite = blueSlot;
                        break;
                    case ElementType.Green:
                        temp.transform.GetChild(0).GetComponent<Image>().sprite = greenSlot;
                        break;
                    case ElementType.Yellow:
                        temp.transform.GetChild(0).GetComponent<Image>().sprite = yellowSlot;
                        break;
                }
            }
            else if (i == GameManager.Instance.PlayerStats.GetElementUpgradeLevel(currentElement.Type))
            {
                temp.transform.localScale = Vector3.one;
            }
        }
    }
}
