//using UnityEditor.SearchService;
using UnityEngine;
using UnityEngine.SceneManagement;

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
        if (G.GM.PlayerStats.GetSpellBook().Count == G.GM.PlayerStats.SpellBookSize)
        {
            if (SceneManager.GetActiveScene().name == _sceneToLoadWhenInteract) G.GM.AdvanceStageLevel();
            SceneTransition.Instance.LoadScene(_sceneToLoadWhenInteract, false);
        }
        else
        {
            if (_emptySpellbookAlert != null) 
            {
                Camera.main.GetComponent<TileClickHandler>().HandleBoardRaycast(false);
                _emptySpellbookAlert.Show();
            }
        }
    }

}
