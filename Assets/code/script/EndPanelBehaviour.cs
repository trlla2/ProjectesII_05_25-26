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

    [Header("Translators")]
    [SerializeField]
    private TranslatorUI winTranslator;
    [SerializeField]
    private TranslatorUI loseTranslator;


    private void Start()
    {
        GameManager.Instance.OnEndBattle += SetWinLosePanel;

        winPanel.SetActive(false);
        losePanel.SetActive(false);
    }

    private void SetWinLosePanel(bool isWin, int exp, int money)
    {


        if(isWin)
        {
            winText.text = "You gain: \n" + exp + "Exp \n" + money + " coins \n \n Click to continue";
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
        SceneTransition.Instance.UnloadScene("BattlePhone");
        
    }

    public void GoMenu()
    {
        loseTranslator.ToOrigin();
        SceneTransition.Instance.LoadScene("MainMenu", false);
    }

    private void OnDestroy()
    {
        GameManager.Instance.OnEndBattle -= SetWinLosePanel;
    }
}
