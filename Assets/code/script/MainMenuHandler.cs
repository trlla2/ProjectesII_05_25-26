using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuHandler : MonoBehaviour
{
    [Header("SETUP")]
    [SerializeField]
    private string gameSceneName = "TestRoc 1";


    [Header("Panels")]
    [SerializeField]
    private GameObject mainMenu;
    [SerializeField]
    private GameObject settings;
    [SerializeField]
    private GameObject credits;

    

    public void OnBack() 
    {
        mainMenu.SetActive(true);
        settings.SetActive(false);  
        credits.SetActive(false);
    }

    public void OnSettings() 
    {
        mainMenu.SetActive(false);
        settings.SetActive(true);  
        credits.SetActive(false);
    }

    public void OnCredits() 
    {
        mainMenu.SetActive(false);
        settings.SetActive(false);  
        credits.SetActive(true);
    }

    public void OnPlay()
    {
        SceneTransition.Instance.LoadScene(gameSceneName, false);
    }
}
