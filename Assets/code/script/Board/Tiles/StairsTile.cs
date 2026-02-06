using UnityEngine;

public class StairsTile : BoardTile
{
    [SerializeField] string _sceneToLoadWhenInteract;

    [SerializeField] private EmptySpellBookAlertHandler _emptySpellbookAlert;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        if (_sceneToLoadWhenInteract.Equals(""))
            _sceneToLoadWhenInteract = "HallOfEgress";
    }

    public override void Interact()
    {
        if (GameManager.Instance.GetPlayerSpellBook().Count == GameManager.Instance.PlayerSpellBookSize)
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
}
