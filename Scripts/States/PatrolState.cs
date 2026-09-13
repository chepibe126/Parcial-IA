using UnityEngine;

public class PatrolState : State
{
    private readonly FSMAgent _agent;
    private readonly PatrolData _data;
    private int currentNode;
    private int direction = 1;

    public PatrolState(FSMAgent agent, PatrolData data, FSM stateMachine) : base(stateMachine) { _agent = agent; _data = data; }
    public override void Enter() { _agent.CurrentTarget = null; _agent.Action = "Patrullando"; }
    public override void Exit() { _agent.Stop(); }
    public override void Update()
    {
        AdvancesAgent dead = _agent.ClosestBoid(true);

        if (dead != null)
        {
            _agent.CurrentTarget = dead;

            StateMachine.ChangeState(PoliceState.Gather); return;
        }
        AdvancesAgent alive = _agent.ClosestBoid(false);

        if (_agent.CanAttack && alive != null)
        {
            _agent.CurrentTarget = alive;

            StateMachine.ChangeState(PoliceState.Attack); return;
        }
        _agent.TickInterestSpawn();

        PatrolLoop();
    }
    private void PatrolLoop()
    {
        if (_data.waypoints == null || _data.waypoints.Count == 0) { _agent.Stop(); _agent.Action = "Faltan waypoints"; return; }

        currentNode = Mathf.Clamp(currentNode, 0, _data.waypoints.Count - 1);

        Transform nextWaypoint = _data.waypoints[currentNode];

        if (nextWaypoint == null) { AdvanceNode(); _agent.Stop(); return; }

        if (Vector3.Distance(_agent.transform.position, nextWaypoint.position) <= _data.waypointCheckDistance)
        {
            if (_data.waypoints.Count == 1) { _agent.Stop(); return; }

            AdvanceNode(); nextWaypoint = _data.waypoints[currentNode];

            if (nextWaypoint == null) return;
        }

        _agent.ApplySteering(_agent.Seek(nextWaypoint.position));
    }
    private void AdvanceNode()
    {
        int count = _data.waypoints.Count;

        if (count <= 1) { currentNode = 0; return; }

        if (!_data.pingPong) { currentNode = (currentNode + 1) % count; return; }

        if (currentNode == count - 1) direction = -1;

        else if (currentNode == 0) direction = 1;

        currentNode += direction;
    }
}
