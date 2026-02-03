using UnityEngine;
using System;

public class Orb
{
    public const int MaxCharge = 10;

    public int CurrentCharge;
    public OrbState State;
    public int OrbIndex;

    public event Action<int, int> OnChargeChanged;
    public event Action<OrbState> OnStateChanged;

    public Orb(int index)
    {
        OrbIndex = index;
        CurrentCharge = 0;
        State = OrbState.Inactive;
    }

    public void AddCharge(int amount)
    {
        if (State == OrbState.Activated) return;

        int previous = CurrentCharge;
        CurrentCharge = Mathf.Clamp(CurrentCharge + amount, 0, MaxCharge);

        if (CurrentCharge != previous)
        {
            UpdateState();
            OnChargeChanged?.Invoke(previous, CurrentCharge);
        }
    }

    public void Deplete()
    {
        State = OrbState.Activated;
        CurrentCharge = 0;
        OnStateChanged?.Invoke(State);
    }

    public void Reset()
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
}