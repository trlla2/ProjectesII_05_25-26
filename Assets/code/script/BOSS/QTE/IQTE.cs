using System;
using System.Collections.Generic;

public interface IQTE
{
    void Start();
    void Update();
    void Cancel();

    List<IQTEPhase> Phases { get; }
    bool IsActive { get; }
    event Action<bool> OnQTEComplete;
}