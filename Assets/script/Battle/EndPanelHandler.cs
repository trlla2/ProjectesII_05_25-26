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
    [SerializeField]
    private bool forceToHallOfEgress;

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
    private float continueDuration = 2;
    private float continueTime = 0;

    [Header("Stage 1")]
    [SerializeField]
    private TMP_Text title1;
    [SerializeField]
    private string winText = "you win";
    [SerializeField]
    private string loseText = "you lose";

    [Header("Audio")]
    [SerializeField]
    private AudioClip winSFX;
    [SerializeField]
    private AudioClip loseSFX;
    [SerializeField]
    private AudioSource aS;

    public UnityEvent OnStage1Enter;
    public UnityEvent OnStage2Enter;
    public UnityEvent OnStage3Enter;

    private bool isWin;

    private bool willLevelUp = false;

    private void Start()
    {
        GameManager.Instance.CombatHandler.OnEndBattle += SetWinLosePanel;

        GameManager.Instance.PlayerStats.OnLevelUp += () => { willLevelUp = true; };
    }
    private void Update()
    {
        if (currentStage < EndBattleStage.STAGE3 && currentStage != EndBattleStage.UNDEFINED)
        {

            if (continueTime < continueDuration)
                continueTime += Time.deltaTime;
            else
            {
                continueFader.Loop = true;
                continueFader.ToTarget();
            }
        }
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

    private void SetWinLosePanel(bool isWin, int exp, int g, Spell drop)
    {
        this.isWin = isWin;

        if (isWin)
        {
            container.sprite = winContainer;
            title1.text = winText;
            title1.color = winColor;
            aS.PlayOneShot(winSFX);
        }
        else
        {
            container.sprite = loseContainer;
            title1.text = loseText;
            title1.color = loseColor;
            aS.PlayOneShot(loseSFX);
        }

        bgFader.ToTarget();
        currentStage = EndBattleStage.STAGE1;
        OnStage1Enter?.Invoke();
    }

    public void NextStage()
    {
        if (continueTime < continueDuration && currentStage != EndBattleStage.STAGE3 )
            return;

        continueFader.ResetToOrigin();
        continueTime = 0;
        Debug.Log($"Current stage: {currentStage}");
        if (!isWin)
        {
            SceneTransition.Instance.LoadScene(loseScene, false);
            return;
        }
        switch (currentStage)
        {
            case EndBattleStage.STAGE1:
                OnStage2Enter?.Invoke();
                currentStage++;

                break;
            case EndBattleStage.STAGE2:
                OnStage3Enter?.Invoke();
                if (willLevelUp)
                    currentStage++;
                else
                    
                    SceneTransition.Instance.UnloadScene(sceneToUnload);
                break;
            case EndBattleStage.STAGE3:
                SceneTransition.Instance.UnloadScene(sceneToUnload);
                break;
            default:
                SceneTransition.Instance.UnloadScene(sceneToUnload);
                break;
        }

    }


    private void LoadWinScene()
    {
        if(forceToHallOfEgress)
        {
            SceneTransition.Instance.UnloadScene(loseScene);
            return;
        }
        SceneTransition.Instance.UnloadScene(sceneToUnload);
    }

}
