using UnityEngine;

public class EndPanelBehaviour : MonoBehaviour
{
    [Header("SETUP")]
    [SerializeField]
    private GameObject endPanel;
    [SerializeField]
    private GameObject winPanel;
    [SerializeField]
    private GameObject losePanel;


    private void Start()
    {
        GameManager.Instance.OnEndGame += SetWinLosePanel;

        endPanel.SetActive(false);
        winPanel.SetActive(false);
        losePanel.SetActive(false);
    }

    private void SetWinLosePanel(bool isWin)
    {
        endPanel.SetActive(true);
        
        if(isWin)
        {
            winPanel.SetActive(true);
            losePanel.SetActive(false);
        }
        else 
        {
            winPanel.SetActive(false);
            losePanel.SetActive(true);
        }
    }

    private void OnDestroy()
    {
        GameManager.Instance.OnEndGame -= SetWinLosePanel;
    }
}
