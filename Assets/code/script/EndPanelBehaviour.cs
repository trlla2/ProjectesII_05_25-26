using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class EndPanelBehaviour : MonoBehaviour
{
    [Header("Panels")]
    [SerializeField]
    private GameObject winPanel;
    [SerializeField]
    private GameObject losePanel;

    [Header("Text")]
    [SerializeField]
    private TMP_Text winText;
    [SerializeField]
    private Color coinTextColor = Color.yellow;
    [SerializeField]
    private Color expTextColor = Color.blue;

    [Header("Translators")]
    [SerializeField]
    private TranslatorUI winTranslator;
    [SerializeField]
    private TranslatorUI loseTranslator;

    [Header("Scenes")]
    [SerializeField]
    private string loseScene;
    [SerializeField]
    private string sceneToUnload = "BattlePhone";
    [SerializeField]
    private bool goToHallOfEgress = false;


    private void Start()
    {
        G.GM.CombatHandler.OnEndBattle += SetWinLosePanel;

        winPanel.SetActive(false);
        losePanel.SetActive(false);
    }

    private void SetWinLosePanel(bool isWin, int exp, int money)
    {


        if(isWin)
        {
            string coinColorHex = ColorUtility.ToHtmlStringRGB(coinTextColor);
            string expColorHex = ColorUtility.ToHtmlStringRGB(expTextColor);

            winText.text = $"You gain: \n <color=#{expColorHex}>{exp} Exp</color> \n <color=#{coinColorHex}>{money} coins</color> \n \n Click to continue";
            winPanel.SetActive(true);
            losePanel.SetActive(false);

            winTranslator.ToTarget();

           
        }
        else 
        {
            winPanel.SetActive(false);
            losePanel.SetActive(true);

            loseTranslator.ToTarget();
        }
    }

    public void UnloadBatle()
    {
        winTranslator.ToOrigin();
        if(goToHallOfEgress)
            SceneTransition.Instance.LoadScene("HallOfEgress", false);
        else
            SceneTransition.Instance.UnloadScene(sceneToUnload);
    }

    public void GoMenu()
    {
        loseTranslator.ToOrigin();
        SceneTransition.Instance.LoadScene(loseScene, false);
    }

    private void OnDestroy()
    {
        GameManager.Instance.EnterBoard();
        G.GM.CombatHandler.OnEndBattle -= SetWinLosePanel;
    }
}
