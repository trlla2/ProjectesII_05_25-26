using UnityEngine;
using UnityEngine.UI;

public class ModoRichard : MonoBehaviour
{
    public static ModoRichard Instance { get; private set; }

    [SerializeField] private Button toggleButton;

    public bool IsActive { get; private set; }

    private void Awake()
    {
        if (Instance != null) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    private void Start()
    {
        toggleButton.onClick.AddListener(Toggle);
    }

    private void Toggle()
    {
        IsActive = !IsActive;

        if (IsActive)
        {
            GameManager.Instance.SetMaxUnlockedStage(GameManager.Instance.MaxStageLevel);
            GameManager.Instance.PlayerStats?.SetHpCheat(10000f);
        }

        Debug.Log($"[ModoRichard] {(IsActive ? "ON" : "OFF")}");
    }

    public bool ShouldBlockDeath() => IsActive;
}
