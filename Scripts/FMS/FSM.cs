using System;
using System.Collections.Generic;

public class FSM
{
    public State CurrentState { get; private set; }
    public Enum CurrentKey { get; private set; }
    private readonly Dictionary<Enum, State> states = new Dictionary<Enum, State>();

    public void RegisterState(Enum key, State state) { states[key] = state; }

    public void ChangeState(Enum key)
    {
        if (!states.TryGetValue(key, out State newState)) throw new ArgumentException("Estado no registrado: " + key);

        if (newState == CurrentState) return;

        CurrentState?.Exit();

        CurrentState = newState; CurrentKey = key;

        CurrentState.Enter();
    }

    public void Update() { CurrentState?.Update(); }
}
