using System.Collections.Generic;
using UnityEngine;

public class EnterBattleHandler : MonoBehaviour
{
    [Header("Setup")]
    [SerializeField] private List<GameObject> gameObjectsToDeactivate = new List<GameObject>();

    private void Start()
    {
        GameManager.Instance.OnEnterBattle += DeactivateGameObjects;
        GameManager.Instance.OnEnterBoard += ActivateGameObjects;
    }

    private void DeactivateGameObjects()
    {
        foreach (var gameObject in gameObjectsToDeactivate) { gameObject.SetActive(false); }
    }

    private void ActivateGameObjects()
    {
        foreach (var gameObject in gameObjectsToDeactivate) { gameObject.SetActive(true); }
    }

    private void OnDestroy()
    {
        GameManager.Instance.OnEnterBattle -= DeactivateGameObjects;
        GameManager.Instance.OnEnterBoard -= ActivateGameObjects;
    }
}
