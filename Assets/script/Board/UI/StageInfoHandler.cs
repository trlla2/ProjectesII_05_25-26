using TMPro;
using UnityEngine;

[RequireComponent(typeof(TextMeshProUGUI))]
public class StageInfoHandler : MonoBehaviour
{
    [SerializeField]
    private BoardManager boardManager;

    private TMP_Text info;

    private void Awake()
    {
        info = GetComponent<TextMeshProUGUI>();
    }

    private void Start()
    {
        SetDifficultyMessage();
    }

    private void OnEnable()
    {
        SetDifficultyMessage();
    }


    private void SetDifficultyMessage()
    {
        switch (boardManager.EncountersManager.GetNextEnemyDifficulty())
        {
            case EnemyDifficulty.NONE:
                info.text = $"Next Battle:\r\n<color=red></color>";
                break;
            case EnemyDifficulty.TUTORIAL:
                info.text = $"Next Battle:\r\n<color=green>Easy</color>";
                break;
            case EnemyDifficulty.DONTHURTME:
                info.text = $"Next Battle:\r\n<color=yellow>Easy</color>";
                break;
            case EnemyDifficulty.EASY:
                info.text = $"Next Battle:\r\n<color=yellow>Easy</color>";
                break;
            case EnemyDifficulty.MEDIUM:
                info.text = $"Next Battle:\r\n<color=yellow>Medium</color>";
                break;
            case EnemyDifficulty.HARD:
                info.text = $"Next Battle:\r\n<color=red>Hard</color>";
                break;
            case EnemyDifficulty.MINIBOSS:
                info.text = $"Next Battle:\r\n<color=red>Boss</color>";
                break;
            case EnemyDifficulty.BOSS:
                info.text = $"Next Battle:\r\n<color=red>Boss</color>";
                break;
        }
    }
}
