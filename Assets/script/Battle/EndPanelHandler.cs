using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class EndPanelHandler : MonoBehaviour
{
    private enum EndBattleStage { UNDEFINED = 0, STAGE1 = 1, STAGE2 = 2, STAGE3 = 3 }
    private EndBattleStage currentStage = EndBattleStage.UNDEFINED;

    [Header("Scenes")]
    [SerializeField]
    private string loseScene;
    [SerializeField]
    private string sceneToUnload;

    [Header("Backkground")]
    [SerializeField]
    private CanvasGroupFader bgFader;

    [Header("Colors")]
    [SerializeField]
    private Color winColor = Color.cyan;
    [SerializeField]
    private Color loseColor = Color.red;

    [Header("Container")]
    [SerializeField]
    private Image container;
    [SerializeField]
    private Sprite winContainer;
    [SerializeField]
    private Sprite loseContainer;

    [Header("Continue")]
    [SerializeField]
    private CanvasGroupFader continueFader;
    [SerializeField]
    private float duration = 5;
    private float time = 0;

    [Header("Stage 1")]
    [SerializeField]
    private TMP_Text title1;
    [SerializeField]
    private string winText = "you win";
    [SerializeField]
    private string loseText = "you lose";

    public UnityEvent OnStage1Enter;
    public UnityEvent OnStage2Enter;
    public UnityEvent OnStage3Enter;

    private bool isWin;

    private void Start()
    {
        GameManager.Instance.CombatHandler.OnEndBattle += SetWinLosePanel;
    }
    private void Update()
    {
        if (currentStage >= EndBattleStage.STAGE3 || currentStage == EndBattleStage.UNDEFINED)
            return;

        if(time < duration)
            time += Time.deltaTime;
        else
        {
            continueFader.Loop = true;
            continueFader.ToTarget();
        }
    }
    private void OnDestroy()
    {
        GameManager.Instance.EnterBoard();
        GameManager.Instance.CombatHandler.OnEndBattle -= SetWinLosePanel;
    }

    private void SetWinLosePanel(bool isWin, int exp, Spell drop)
    {
        if (isWin)
        {
            container.sprite = winContainer;
            title1.text = winText;
            title1.color = winColor;
        }
        else
        {
            container.sprite = loseContainer;
            title1.text = loseText;
            title1.color = loseColor;
        }

        bgFader.ToOrigin();

        OnStage1Enter?.Invoke();

        this.isWin = isWin;
    }

    public void NextStaga()
    {
        if (!isWin)
        {
            SceneTransition.Instance.LoadScene(loseScene, false);
            return;
        }


        switch (currentStage)
        {
            case EndBattleStage.STAGE1:
                OnStage2Enter?.Invoke();
                break;
            case EndBattleStage.STAGE2:
                OnStage3Enter?.Invoke();
                break;
            case EndBattleStage.STAGE3:
                SceneTransition.Instance.UnloadScene(sceneToUnload);
                break;
            default:
                SceneTransition.Instance.UnloadScene(sceneToUnload);
                break;
        }

        currentStage++;

        continueFader.ResetToOrigin();
        time = 0;
    }


}
