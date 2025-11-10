using System.Xml;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class LevelUIBehaviour : MonoBehaviour
{
    [Header("Exp Bar")]
    [SerializeField]
    private Slider expSlider;
    [SerializeField]
    private TMP_Text lvlText;


    [Header("LevelUp Setup")]
    [SerializeField]
    private GameObject lvlUpPanel;
    [SerializeField]
    private TranslatorUI lvlUpPanelTranslator;


    private void Start()
    {
        PlayerStats.Instance.OnLevelUp += StartLevelUpPanel;

        UpdateEXPBar();
    }

    private void StartLevelUpPanel()
    {
        UpdateEXPBar();

        lvlUpPanel.SetActive(true);

        lvlUpPanelTranslator.ToTarget();
    }

    private void UpdateEXPBar()
    {
        lvlText.text = PlayerStats.Instance.GetLevel() + "lvl";

        expSlider.value = PlayerStats.Instance.GetEXP()/ PlayerStats.Instance.GetEXPForNextLevel(); 
    }

    private void OnDestroy()
    {
        PlayerStats.Instance.OnLevelUp -= StartLevelUpPanel;
    }

    public void OnUpgradeElement(int i)
    {
       PlayerStats.Instance.UpgradeElement((ElementType)i);
       UpdateEXPBar();
    }

}
