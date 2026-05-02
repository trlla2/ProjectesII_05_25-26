using UnityEngine;

public class LoadScene : MonoBehaviour
{
    [Header("Scene Names")]
    [SerializeField]
    private string tutorialSceneName;
    [SerializeField]
    private string hallOfEgressSceneName;

    private void Start()
    {
        SaveManager.Instance.LoadPlayerStats();
        if (!SaveManager.Instance.TutorialCompleted)
            SceneTransition.Instance.LoadScene(tutorialSceneName);
        else
            SceneTransition.Instance.LoadScene(hallOfEgressSceneName);
    }
}
