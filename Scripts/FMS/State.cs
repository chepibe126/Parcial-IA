public abstract class State
{
    protected FSM StateMachine;
    protected State(FSM stateMachine) { StateMachine = stateMachine; }

    public virtual void Enter() { }
    public virtual void Exit() { }
    public virtual void Update() { }
}
