using UnityEngine;

public class StairsTile : BoardTile
{
    [SerializeField] string _sceneToLoadWhenInteract;

    [SerializeField] private EmptySpellBookAlertHandler _emptySpellbookAlert;

    private int _stageLevel;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        //if (_sceneToLoadWhenInteract.Equals(""))
        //    _sceneToLoadWhenInteract = "HallOfEgress";

    }

    public override void Interact()
    {
        if (GameManager.Instance.GetPlayerSpellBook().Count > 0)
            SceneTransition.Instance.LoadScene(_sceneToLoadWhenInteract, false);
        else
        {
            if (_emptySpellbookAlert != null) 
            {
                Camera.main.GetComponent<TileClickHandler>().HandleBoardRaycast(false);
                _emptySpellbookAlert.Show();
            }
        }
    }

    public void SetStageLevel(int level)
    {
        _stageLevel = level;
        switch (_stageLevel)
        {
            case 1: _sceneToLoadWhenInteract = "Stage2";
                break;
            case 2: _sceneToLoadWhenInteract = "Stage3";
                break;
            case /*>=*/ 3: _sceneToLoadWhenInteract = "Stage4";
                break;
            case 4: _sceneToLoadWhenInteract = "HallOfEgress";
                break;
        }
    }
}
