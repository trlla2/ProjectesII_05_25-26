using UnityEngine;

public class TutorialButton : MonoBehaviour
{
    public void LoadTutorial()
    {
        SaveManager.Instance.ResetSavedStats();

        SceneTransition.Instance.LoadScene("NewTutorialScene", false);
    }
}
