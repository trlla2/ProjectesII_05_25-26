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
        // Skip objects already destroyed during scene unload
        foreach (var gameObject in gameObjectsToDeactivate) { if (gameObject != null) gameObject.SetActive(false); }
    }

    private void ActivateGameObjects()
    {
        // Skip objects already destroyed during scene unload
        foreach (var gameObject in gameObjectsToDeactivate) { if (gameObject != null) gameObject.SetActive(true); }
    }

    private void OnDestroy()
    {
        if (GameManager.Instance == null) return;

        GameManager.Instance.OnEnterBattle -= DeactivateGameObjects;
        GameManager.Instance.OnEnterBoard -= ActivateGameObjects;
    }
}
