using UnityEngine;

public class StairsTile : BoardTile
{
    [SerializeField] string _sceneToLoadWhenInteract;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        if (_sceneToLoadWhenInteract.Equals(""))
            _sceneToLoadWhenInteract = "HallOfEgress";
    }

    public override void Interact()
    {
        SceneTransition.Instance.LoadScene(_sceneToLoadWhenInteract, false);
    }
}
