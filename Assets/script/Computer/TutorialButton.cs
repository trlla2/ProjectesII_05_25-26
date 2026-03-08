using UnityEngine;

public class TutorialButton : MonoBehaviour
{
    public void LoadTutorial()
    {
        SceneTransition.Instance.LoadScene("NewTutorialScene", false);
    }
}
