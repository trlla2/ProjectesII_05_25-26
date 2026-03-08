using System;

// Interface para 1 fase de Quick time event, las fases se controlan desde los QTE
public interface IQTEPhase
{
    void Enter();
    void update();
    void Exit();
    event Action<bool> OnPhaseComplete;
}