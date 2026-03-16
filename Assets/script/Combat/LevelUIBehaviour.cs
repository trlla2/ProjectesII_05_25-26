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
        GameManager.Instance.PlayerStats.OnLevelUp += StartLevelUpPanel;
    }

    private void StartLevelUpPanel()
    {
        lvlUpPanel.SetActive(true);
        lvlUpPanelTranslator.ToTarget();
    }


    public void OnUpgradeElement(int i)
    {
       GameManager.Instance.PlayerStats.UpgradeElementalLevel((ElementType)i);
    }

    private void OnDestroy()
    {
        GameManager.Instance.PlayerStats.OnLevelUp -= StartLevelUpPanel;
    }
}
