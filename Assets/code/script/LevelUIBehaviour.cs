using System.Collections;
using System.Security.Cryptography;
using System.Xml;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using static UnityEngine.Rendering.DebugUI;

public class LevelUIBehaviour : MonoBehaviour
{
    [Header("LevelUp Setup")]
    [SerializeField]
    private GameObject lvlUpPanel;
    [SerializeField]
    private TranslatorUI lvlUpPanelTranslator;


    private void Start()
    {
        PlayerStats.Instance.OnLevelUp += StartLevelUpPanel;
    }

    private void StartLevelUpPanel()
    {
        lvlUpPanel.SetActive(true);
        lvlUpPanelTranslator.ToTarget();
    }


    public void OnUpgradeElement(int i)
    {
        PlayerStats.Instance.UpgradeElement((ElementType)i);
    }

    private void OnDestroy()
    {
        PlayerStats.Instance.OnLevelUp -= StartLevelUpPanel;
    }
}
