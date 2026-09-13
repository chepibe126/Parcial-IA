using UnityEngine;

public class GatherState : State
{
    private readonly FSMAgent _agent;
    private float timer;

    public GatherState(FSMAgent agent, FSM stateMachine) : base(stateMachine) { _agent = agent; }

    public override void Enter() { timer = 0f; _agent.Action = "Buscando cuerpo"; }

    public override void Exit() { _agent.Stop(); timer = 0f; }

    public override void Update()
    {
        AdvancesAgent body = _agent.CurrentTarget;

        if (!_agent.IsDetected(body) || !body.IsDead)
        {
            _agent.CurrentTarget = null;

            StateMachine.ChangeState(PoliceState.Patrol); return;
        }
        float distance = Vector3.Distance(_agent.transform.position, body.transform.position);

        if (distance > _agent.gatherRadius)
        {
            timer = 0f; _agent.Action = "Acercandose al cuerpo";

            _agent.ApplySteering(_agent.Arrive(body.transform.position, _agent.gatherRadius * 0.85f));

            return;
        }
        _agent.Stop(); timer += Time.deltaTime;

        _agent.Action = "Recolectando: " + timer.ToString("F1") + "/" + _agent.gatherDuration.ToString("F1") + " s";

        if (timer >= _agent.gatherDuration)
        {
            body.Collect(); _agent.CurrentTarget = null;

            StateMachine.ChangeState(PoliceState.Patrol);
        }
    }
}
