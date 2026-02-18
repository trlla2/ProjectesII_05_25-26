using UnityEngine;


public class LevelUIBehaviour : MonoBehaviour
{
    [Header("LevelUp Setup")]
    [SerializeField]
    private GameObject lvlUpPanel;
    [SerializeField]
    private TranslatorUI lvlUpPanelTranslator;


    private void Start()
    {
        G.GM.PlayerStats.OnLevelUp += StartLevelUpPanel;
    }

    private void StartLevelUpPanel()
    {
        lvlUpPanel.SetActive(true);
        lvlUpPanelTranslator.ToTarget();
    }


    public void OnUpgradeElement(int i)
    {
       G.GM.PlayerStats.UpgradeElementalLevel((ElementType)i);
    }

    private void OnDestroy()
    {
        G.GM.PlayerStats.OnLevelUp -= StartLevelUpPanel;
    }
}
