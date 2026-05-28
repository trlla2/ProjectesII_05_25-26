using UnityEngine;
using System;
using UnityEngine.UI;

public class Orb : MonoBehaviour
{
    public const int MaxCharge = 10;

    public int CurrentCharge;
    public OrbState State;
    public int OrbIndex;

    [Header("QTE Reference")]
    [SerializeField] private Orb1QTE orb1QTEController;
    [SerializeField] private Orb2QTE orb2QTEController;

    private IQTE qteController;

    public event Action<int, int> OnChargeChanged;
    public event Action<OrbState> OnStateChanged;
    public event Action OnOrbActivated;
    public event Action OnOrbDepleted;
    public bool HasBeenActivated { get; private set; } = false;

    [HideInInspector]
    public bool IsActive;

    private Button orbButton;

    private void Awake()
    {
        CurrentCharge = 0;
        State = OrbState.Inactive;

        orbButton = GetComponent<Button>();
        orbButton.onClick.AddListener(OnOrbClicked);

        // Use whichever controller is assigned; Orb1 takes priority
        if (orb1QTEController != null)
            qteController = orb1QTEController;
        else if (orb2QTEController != null)
            qteController = orb2QTEController;
        else
            Debug.LogError($"No QTE controller assigned on {gameObject.name}!");
    }

    private void OnDestroy()
    {
        if (qteController != null)
        {
            qteController.OnQTEComplete -= HandleQTEComplete;
        }
    }

    private void OnOrbClicked()
    {
        if (State != OrbState.Charged)
        {
            Debug.Log("Orb cannot be activated, not fully charged!");
            return;
        }

        if (qteController == null)
        {
            Debug.LogError("Orb has no QTE controller assigned!");
            return;
        }

        if (qteController.IsActive)
        {
            Debug.LogWarning("QTE already in progress!");
            return;
        }

        ActivateOrb();
    }

    private void ActivateOrb()
    {
        HasBeenActivated = true;
        OnOrbActivated?.Invoke();

        qteController.OnQTEComplete += HandleQTEComplete;
        qteController.start();
    }

    private void HandleQTEComplete(bool success)
    {
        qteController.OnQTEComplete -= HandleQTEComplete;

        if (success)
        {
            Debug.Log("QTE Success!");
            Deplete();
        }
        else
        {
            Debug.Log("QTE Failed!");
            int previousCharge = CurrentCharge;
            CurrentCharge = Mathf.Max(0, CurrentCharge - 4);

            if (orb1QTEController != null)
                orb1QTEController.ResetVisuals();
            else if (orb2QTEController != null)
                orb2QTEController.ResetVisuals();

            if (CurrentCharge != previousCharge)
            {
                OnChargeChanged?.Invoke(previousCharge, CurrentCharge);
                UpdateState();
            }
        }
    }

    public void AddCharge(int amount)
    {
        if (State == OrbState.Activated) return;

        int previous = CurrentCharge;
        CurrentCharge = Mathf.Clamp(CurrentCharge + amount, 0, MaxCharge);

        if (CurrentCharge != previous)
        {
            OnChargeChanged?.Invoke(previous, CurrentCharge);
            UpdateState();
        }
    }

    public void Deplete()
    {
        State = OrbState.Activated;
        CurrentCharge = 0;
        OnStateChanged?.Invoke(State);
        OnOrbDepleted?.Invoke();
    }

    public void ResetCharge()
    {
        CurrentCharge = 0;
        State = OrbState.Inactive;
        OnStateChanged?.Invoke(State);
    }

    private void UpdateState()
    {
        OrbState newState;

        switch (CurrentCharge)
        {
            case 0:
                newState = OrbState.Inactive;
                break;

            case MaxCharge:
                newState = OrbState.Charged;
                break;

            default:
                newState = OrbState.Charging;
                break;
        }

        if (newState != State)
        {
            State = newState;
            OnStateChanged?.Invoke(State);
        }
    }

    public bool IsFullyCharged()
    {
        return CurrentCharge >= MaxCharge;
    }
}