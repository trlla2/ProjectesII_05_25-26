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
        GameManager.Instance.OnPlayerLevelUp += StartLevelUpPanel;
    }

    private void StartLevelUpPanel()
    {
        lvlUpPanel.SetActive(true);
        lvlUpPanelTranslator.ToTarget();
    }


    public void OnUpgradeElement(int i)
    {
        GameManager.Instance.UpgradePlayerElementLevel((ElementType)i);
    }

    private void OnDestroy()
    {
        GameManager.Instance.OnPlayerLevelUp -= StartLevelUpPanel;
    }
}
