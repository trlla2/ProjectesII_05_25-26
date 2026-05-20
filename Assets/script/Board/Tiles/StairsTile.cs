//using UnityEditor.SearchService;
using UnityEngine;
using UnityEngine.SceneManagement;

public class StairsTile : BoardTile
{
    [SerializeField] string _sceneToLoadWhenInteract;

    [SerializeField] private EmptySpellBookAlertHandler _emptySpellbookAlert;

    //private int _stageLevel;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        //if (_sceneToLoadWhenInteract.Equals(""))
        //    _sceneToLoadWhenInteract = "HallOfEgress";

    }

    public override void Interact()
    {
        if (_boardManager.IsHallOfEgress)
        {
            if (GameManager.Instance.PlayerStats.GetSpellBook().Count != GameManager.Instance.PlayerStats.SpellBookSize 
                && _emptySpellbookAlert != null) 
            {   
                Camera.main.GetComponent<TileClickHandler>().HandleBoardRaycast(false);
                _emptySpellbookAlert.Show();
                return;
            }

            if (GameManager.Instance.StageLevel >= GameManager.Instance.MaxStageLevel)
                GameManager.Instance.ResetStageLevel();

            SceneTransition.Instance.LoadScene(_sceneToLoadWhenInteract, false);
            return;
        }
        if (_boardManager.IsTutorial)
        {
            GameManager.Instance.ResetStageLevel();
            SceneTransition.Instance.LoadScene("HallOfEgress", false);
            return;
        }

        if (GameManager.Instance.StageLevel < GameManager.Instance.MaxStageLevel)
            GameManager.Instance.AdvanceStageLevel();

        SceneTransition.Instance.LoadScene("HallOfEgress", false);

        //GameManager.Instance.ResetStageLevel();
        //return;
        

        
    }

}
