using UnityEngine;

public class LoadHallOfEgress : MonoBehaviour
{
    [SerializeField] private SceneTransition sceneTransition;

    public void LoadScene()
    {
        GameManager.Instance.ResetStageLevel();
        sceneTransition.LoadScene("HallOfEgress", false);
    }
}
