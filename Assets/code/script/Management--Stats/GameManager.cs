using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }
    
    private PlayerStats playerStats = new PlayerStats();
    public PlayerStats PlayerStats => playerStats;

    
    private int stageLevel = 1;
    public int StageLevel => stageLevel;

    private EnemyType forcedEnemyType;

    [SerializeField]private TutorialManager tutorialManager;
    
    private CombatHandler cH;
    public CombatHandler CombatHandler => cH;

    //Events
    public event System.Action OnEnterBoard;
    public event System.Action OnEnterBattle;
    
    public event System.Action OnPlayerMove;

    private void Awake()
    {
        if (Instance != null)
        {
            Destroy(this.gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(this.gameObject);
    }

    public void SetCombatHandler(CombatHandler cHRef) { cH = cHRef;  }
    public void EnterBoard() { OnEnterBoard?.Invoke(); }
    public void EnterBattle() { OnEnterBattle?.Invoke(); }


    public void SetForcedEnemy(EnemyType type) { forcedEnemyType = type; }

    public void NotifyPlayerMove() { OnPlayerMove?.Invoke(); }

    public void Destroy()
    {
        Destroy(this.gameObject);
    }
}