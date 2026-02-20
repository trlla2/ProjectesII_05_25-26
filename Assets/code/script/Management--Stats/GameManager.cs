using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }
    
    private PlayerStats playerStats = new PlayerStats();
    public PlayerStats PlayerStats => playerStats;

    private int stageLevel = 1;
    public int StageLevel => stageLevel;

    [SerializeField] private List<StageLevelData> _stageDatas;

    [SerializeField] private TutorialManager tutorialManager;
    
    private CombatHandler cH;
    public CombatHandler CombatHandler => cH;

    private EnemyType _pendingEnemy;
    public EnemyType PendingEnemy => _pendingEnemy;

    //Events
    public event System.Action OnEnterBoard;
    public event System.Action OnEnterBattle;
    public event System.Action OnPlayerMove;

    [SerializeField] private TileClickHandler _boardClickHandler;

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

    public void SetCombatHandler(CombatHandler cHRef) { cH = cHRef; }
    public void SetBoardClickHandler(TileClickHandler tch) { _boardClickHandler = tch; }
    public void EnterBoard() { OnEnterBoard?.Invoke(); /*_boardClickHandler.HandleBoardRaycast(true);*/ }
    public void EnterBattle() { OnEnterBattle?.Invoke(); }

    public void SetPendingEnemy(EnemyType type)
    {
        _pendingEnemy = type;
    }

    public EnemyType ClearPendingEnemy()
    {
        EnemyType e = _pendingEnemy;
        _pendingEnemy = null;
        return e;
    }

    public void NotifyPlayerMove() { OnPlayerMove?.Invoke(); }

    public void Destroy()
    {
        Destroy(this.gameObject);
    }

    public StageLevelData GetCurrentStageData()
    {
        return _stageDatas[stageLevel];
    }
}