using UnityEngine;

public class AttackState : State
{
    private readonly FSMAgent _agent;

    public AttackState(FSMAgent agent, FSM stateMachine) : base(stateMachine)
    {
        _agent = agent;
    }

    public override void Enter() { _agent.Action = "Evaluando distancia al objetivo"; }

    public override void Exit() { _agent.Stop(); }

    public override void Update()
    {
        AdvancesAgent victim = _agent.CurrentTarget;

        if (!_agent.IsDetected(victim))
        {
            // Perder vision no reinicia TBA.
            StateMachine.ChangeState(PoliceState.Patrol);

            return;
        }
        if (victim.IsDead)
        {
            StateMachine.ChangeState(PoliceState.Gather);

            return;
        }
        AdvancesAgent dead = _agent.ClosestBoid(true);

        if (dead != null)
        {
            _agent.CurrentTarget = dead;

            StateMachine.ChangeState(PoliceState.Gather);

            return;
        }
        if (!_agent.CanAttack)
        {
            StateMachine.ChangeState(PoliceState.Patrol);

            return;
        }

        float distance = Vector3.Distance(_agent.transform.position, victim.transform.position);

        // Prioridad 1: zona de cuerpo a cuerpo. No se dispara en esta rama.
        if (distance <= _agent.MeleeAttackRadius)
        {
            if (distance <= _agent.meleeHitRadius)
            {
                _agent.Stop();

                if (_agent.PerformAttack(victim, true)) FinishAttack(victim);
            }
            else
            {
                _agent.Action = "Persiguiendo para golpe cuerpo a cuerpo";
                // Cerca buscamos al boid actual, sin apuntar por delante de el.
                _agent.ApplySteering(_agent.Seek(victim.transform.position));
            }
            return;
        }

        // Prioridad 2: fuera de melee, pero dentro del rango de disparo.
        if (distance <= _agent.RangeAttackRadius)
        {
            _agent.Stop();

            if (_agent.PerformAttack(victim, false)) FinishAttack(victim);

            return;
        }

        // Prioridad 3: visible, pero todavia fuera de ambos rangos de ataque.
        _agent.Action = "Persiguiendo hasta rango de ataque";

        _agent.ApplySteering(_agent.Pursuit(victim));
    }

    private void FinishAttack(AdvancesAgent victim)
    {
        // PerformAttack aplica el golpe y reinicia TBA. Salimos de Attack.
        StateMachine.ChangeState(victim.IsDead ? PoliceState.Gather : PoliceState.Patrol);
    }
}
