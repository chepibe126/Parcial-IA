using UnityEngine;

public enum PoliceState { Patrol, Attack, Gather }

[RequireComponent(typeof(AgentSensor))]
public class FSMAgent : AgentSteering
{
    [Header("Patrol")]
    [SerializeField] private PatrolData dataPatrol = new PatrolData();

    [Header("Attack")]
    [Min(0.1f)] public float TBA = 2f;
    [Min(0.1f)] public float RangeAttackRadius = 7f;
    [Min(0.1f)] public float MeleeAttackRadius = 3f;
    [Tooltip("Distancia real para golpear, dentro de la zona de persecucion melee.")]
    [Min(0.1f)] public float meleeHitRadius = 1.35f;

    [Header("Gather")]
    [Min(0.1f)] public float gatherDuration = 2f;
    [Min(0.1f)] public float gatherRadius = 1.4f;

    [Header("Interest objects")]
    public InterestObject interestPrefab;
    [Min(0.1f)] public float spawnInterval = 4f;

    [Header("Feedback")]
    public AgentSensor Sensor { get; private set; }
    public AdvancesAgent CurrentTarget { get; set; }
    public string Action { get; set; } = "Patrullando";
    public float AttackRemaining { get { return Mathf.Max(0f, nextAttackTime - Time.time); } }
    public bool CanAttack { get { return Time.time >= nextAttackTime; } }
    public string StateName { get { return stateMachine == null ? "Inicializando" : stateMachine.CurrentKey.ToString(); } }
    private FSM stateMachine;
    private float nextAttackTime;
    private float spawnTimer;
    private string lastAttack = "Ninguno";
    private float shotUntil;
    private Vector3 shotEnd;

    public string LastAttack {get{ return lastAttack;}}
    public float ShotUntil {get{ return shotUntil;}}
    public Vector3 ShotEnd {get{ return shotEnd;}}

    private void Awake()
    {
        Sensor = GetComponent<AgentSensor>();

        TBA = Mathf.Max(0.1f, TBA);

        gatherDuration = Mathf.Max(0.1f, gatherDuration);

        spawnInterval = Mathf.Max(0.1f, spawnInterval);

        meleeHitRadius = Mathf.Max(BodyRadius + 0.6f, meleeHitRadius);

        MeleeAttackRadius = Mathf.Max(meleeHitRadius, MeleeAttackRadius);

        RangeAttackRadius = Mathf.Max(MeleeAttackRadius, RangeAttackRadius);

        gatherRadius = Mathf.Max(BodyRadius + 0.6f, gatherRadius);

        if (dataPatrol == null) 

         dataPatrol = new PatrolData();
    }
    private void Start()
    {
       
        stateMachine = new FSM();

        stateMachine.RegisterState(PoliceState.Patrol, new PatrolState(this, dataPatrol, stateMachine));

        stateMachine.RegisterState(PoliceState.Attack, new AttackState(this, stateMachine));

        stateMachine.RegisterState(PoliceState.Gather, new GatherState(this, stateMachine));

        stateMachine.ChangeState(PoliceState.Patrol);
    }
    private void Update()
    {
        Sensor.Scan();
        
        stateMachine?.Update();
    }
    public AdvancesAgent ClosestBoid(bool dead)
    {
        AdvancesAgent result = null; float closest = float.PositiveInfinity;

        foreach (AdvancesAgent boid in Sensor.Boids)
        {
            if (boid == null || boid.IsCollected || boid.IsDead != dead) continue;

            float distance = (boid.transform.position - transform.position).sqrMagnitude;

            if (distance < closest) { closest = distance; result = boid; }
        }

        return result;
    }
    public bool IsDetected(AdvancesAgent boid)
    {
        return boid != null && !boid.IsCollected && Sensor.Boids.Contains(boid);
    }
    public bool PerformAttack(AdvancesAgent boid, bool melee)
    {
        if (!CanAttack || !IsDetected(boid) || boid.IsDead) return false;

        float distance = Vector3.Distance(transform.position, boid.transform.position);

        if (melee)
        {
            // Debe estar en la zona melee y suficientemente cerca para golpear.
            if (distance > MeleeAttackRadius || distance > meleeHitRadius) return false;
        }
        else
        {
            // Dentro de la zona melee no corresponde disparar.
            if (distance <= MeleeAttackRadius || distance > RangeAttackRadius) return false;
        }

        boid.TakeDamage(1f);

        nextAttackTime = Time.time + TBA;

        Action = melee ? "Golpe cuerpo a cuerpo" : "Disparo a distancia";

        lastAttack = Action + " a " + boid.name;

        shotEnd = boid.transform.position;

        shotUntil = Time.time + 0.3f;

        return true;
    }

    public void TickInterestSpawn()
    {
        spawnTimer += Time.deltaTime;

        if (spawnTimer < spawnInterval || interestPrefab == null || InterestObject.ActiveCount >= 5)

            return;

        // Posicion actual, sin desplazamiento aleatorio y sin parenting al cazador.

        InterestObject instance = Instantiate(interestPrefab, transform.position, Quaternion.identity);

        instance.gameObject.SetActive(true);

        Physics.SyncTransforms();

        spawnTimer = 0f;
    }
}
