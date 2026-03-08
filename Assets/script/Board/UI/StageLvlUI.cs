using TMPro;
using UnityEngine;

[RequireComponent(typeof(TextMeshProUGUI))]
public class StageLvlUI : MonoBehaviour
{
    private TMP_Text lvl;

    private void Awake()
    {
        lvl = GetComponent<TextMeshProUGUI>();
    }

    private void OnEnable()
    {
        lvl.text = $"Stage {GameManager.Instance.StageLevel}";
    }
}
