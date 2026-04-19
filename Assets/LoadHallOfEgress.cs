using UnityEngine;

public class LoadHallOfEgress : MonoBehaviour
{
    [SerializeField] private SceneTransition sceneTransition;

    public void LoadScene()
    {
        sceneTransition.LoadScene("HallOfEgress", false);
    }
}
