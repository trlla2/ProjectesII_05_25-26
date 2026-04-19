using TMPro;
using UnityEngine;

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
    private Color spellDropTextColor = Color.green;
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
    private string sceneToUnload;
    [SerializeField]
    private bool goToHallOfEgress = false;


    private void Start()
    {
        GameManager.Instance.CombatHandler.OnEndBattle += SetWinLosePanel;

        winPanel.SetActive(false);
        losePanel.SetActive(false);
    }

    private void SetWinLosePanel(bool isWin, int exp, int g, Spell drop)
    {
        if (winText == null || winPanel == null || losePanel == null) return;

        if(isWin)
        {
            string spellDropColorHex = ColorUtility.ToHtmlStringRGB(spellDropTextColor);
            string expColorHex = ColorUtility.ToHtmlStringRGB(expTextColor);

            winText.text = $"You gain: \n <color=#{expColorHex}>{exp} Exp</color> ";
            if (drop != null)
                winText.text += $" \n <color=#{spellDropColorHex}>NEW SPELL: {drop.name}</color> \n";
            winText.text += "\n Click to continue";

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
        if (GameManager.Instance != null)
        {
            GameManager.Instance.EnterBoard();

            if (GameManager.Instance.CombatHandler != null)
            {
                GameManager.Instance.CombatHandler.OnEndBattle -= SetWinLosePanel;
            }
        }
    }
}
