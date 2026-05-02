using UnityEngine;

public class SaveHallOfEgress : MonoBehaviour
{
    void Start()
    {
        SaveManager.Instance.SavePlayerStats();

        if(!SaveManager.Instance.TutorialCompleted)
            SaveManager.Instance.TutorialCompleted = true;
    }
}
