using TMPro;
using UnityEngine;

public class HallOfEgressPlayerBarLevelInfoHandler : MonoBehaviour
{
    [SerializeField] private TMP_Text stageText;
    void Start()
    {
        if (GameManager.Instance.StageLevel == 1)
            stageText.text = "Hall of Egress";
        else
            stageText.text = "Stage " + (GameManager.Instance.StageLevel) + "\n(Hall of Egress)";
    }
}
