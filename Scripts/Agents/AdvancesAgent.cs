using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(AgentSensor))]

public class AdvancesAgent : AgentSteering
{
    [Header("Flocking")]
    [SerializeField, Min(0.1f)] private float separationRadius = 1.6f;
    [SerializeField, Min(0.2f)] private float alignmentRadius = 5f;
    [SerializeField, Min(0.2f)] private float choesionRadius = 5f;
    [SerializeField, Range(0f, 3f)] private float separationWeight = 1.8f;
    [SerializeField, Range(0f, 3f)] private float cohesionWeight = 0.7f;
    [SerializeField, Range(0f, 3f)] private float alignmentWeight = 1f;

    [Header("Life and interaction")]
    private const float maxHealth = 1f;

    [SerializeField, Min(0.1f)] private float damageToInterest = 10f;
    [SerializeField, Min(0.1f)] private float interactionInterval = 1f;
    [SerializeField, Min(0.1f)] private float respawnDelay = 8f;
    public float Health { get; private set; }
    public bool IsDead { get { return Health <= 0f; } }
    public bool IsCollected { get; private set; }
    public string Behaviour { get; private set; } = "Flocking";
    public AgentSensor Sensor { get; private set; }
    private float nextInteraction;
    private Renderer[] visuals;
    private Collider[] bodies;
    private bool[] visualEnabled;
    private bool[] colliderEnabled;

    private void Awake()
    {
        Sensor = GetComponent<AgentSensor>();

        visuals = GetComponentsInChildren<Renderer>();
        
        bodies = GetComponentsInChildren<Collider>();

        visualEnabled = new bool[visuals.Length]; colliderEnabled = new bool[bodies.Length];

        for (int i = 0; i < visuals.Length; i++) visualEnabled[i] = visuals[i].enabled;

        for (int i = 0; i < bodies.Length; i++) colliderEnabled[i] = bodies[i].enabled;

        Health = maxHealth;

        InitializeVelocity();

        ValidateRadio();
    }
    private void InitializeVelocity()
    {
        float angle = Random.Range(0f, Mathf.PI * 2f);

        _velocity = new Vector3(Mathf.Cos(angle), 0f, Mathf.Sin(angle)) * _maxSpeed;

        transform.forward = _velocity.normalized;
    }
    private void OnValidate() 
    {
        ValidateRadio(); 
    }

    private void ValidateRadio()
    {
        separationRadius = Mathf.Max(0.1f, separationRadius);

        alignmentRadius = Mathf.Max(separationRadius + 0.1f, alignmentRadius);

        choesionRadius = Mathf.Max(separationRadius + 0.1f, choesionRadius);

        AgentSensor sensor = GetComponent<AgentSensor>();

        if (sensor != null) sensor.perceptionRadius = Mathf.Max(sensor.perceptionRadius, Mathf.Max(alignmentRadius, choesionRadius));
    }

    private void Update()
    {
        if (IsCollected || IsDead) 
        {
            Stop(); return; 
        }

        Sensor.Scan();

        FSMAgent threat = ClosestHunter();

        if (threat != null)
        {
            Behaviour = "Evade"; 
            // Prioridad absoluta: no sumamos cohesion, alineacion ni Arrive.
            ApplySteering(Evade(threat));

            return;
        }
        InterestObject food = ClosestInterest();

        if (food != null)
        {
            float stopDistance = BodyRadius + food.BodyRadius + 0.12f;

            if (Vector3.Distance(transform.position, food.transform.position) <= stopDistance + 0.2f)
            {
                Behaviour = "Interact"; Stop();

                if (Time.time >= nextInteraction)
                {
                    food.TakeDamage(damageToInterest);

                    nextInteraction = Time.time + interactionInterval;
                }
            }
            else
            {
                Behaviour = "Arrive";

                ApplySteering(Arrive(food.transform.position, stopDistance) + CalculateSeparation(Sensor.Boids, separationRadius) * separationWeight);
            }

            return;
        }
        Behaviour = "Flocking"; 

        ApplySteering(Flocking());
    }
    private Vector3 Flocking()
    {
        Vector3 steering = CalculateSeparation(Sensor.Boids, separationRadius) * separationWeight
            + CalculateAlignment(Sensor.Boids, alignmentRadius) * alignmentWeight
            + CalculateChoesion(Sensor.Boids, choesionRadius) * cohesionWeight;
        
        if (steering.sqrMagnitude < 0.0001f && _velocity.sqrMagnitude < 0.0001f)

        return CaculateStering(transform.forward * _maxSpeed);

        return steering;
    }
    private Vector3 CalculateSeparation(List<AdvancesAgent> list, float radius)
    {
        Vector3 desired = Vector3.zero;

        foreach (AdvancesAgent item in list)
        {
            if (item == null || item.IsCollected) 
                
            continue;

            Vector3 away = transform.position - item.transform.position;

            float distance = away.magnitude;

            if (distance > radius)
                
            continue;

            if (distance < 0.001f) away = GetInstanceID() < item.GetInstanceID() ? Vector3.left : Vector3.right;

            desired += away.normalized / Mathf.Max(distance, 0.05f);
        }

        return desired.sqrMagnitude > 0.0001f ? CaculateStering(desired.normalized * _maxSpeed) : Vector3.zero;
    }
    private Vector3 CalculateAlignment(List<AdvancesAgent> list, float radius)
    {
        Vector3 desired = Vector3.zero; int count = 0;

        foreach (AdvancesAgent item in list)
        {
            if (item == null || item.IsDead || item.IsCollected || Vector3.Distance(transform.position, item.transform.position) > radius)
                
            continue;

            desired += item.Velocity;
            
            count++;
        }

        return count > 0 ? CaculateStering((desired / count).normalized * _maxSpeed) : Vector3.zero;
    }
    private Vector3 CalculateChoesion(List<AdvancesAgent> list, float radius)
    {
        Vector3 center = Vector3.zero;
        
        int count = 0;

        foreach (AdvancesAgent item in list)
        {
            if (item == null || item.IsDead || item.IsCollected || Vector3.Distance(transform.position, item.transform.position) > radius)
                
            continue;

            center += item.transform.position;
            
            count++;
        }

        return count > 0 ? Seek(center / count) : Vector3.zero;
    }
    private FSMAgent ClosestHunter()
    {
        FSMAgent result = null; 

        float closest = float.PositiveInfinity;

        foreach (FSMAgent hunter in Sensor.Hunters)
        {
            float distance = (hunter.transform.position - transform.position).sqrMagnitude;

            if (distance < closest)
            { 
                closest = distance; result = hunter; 
            }
        }

        return result;
    }
    private InterestObject ClosestInterest()
    {
        InterestObject result = null;

        float closest = float.PositiveInfinity;

        foreach (InterestObject interest in Sensor.Interests)
        {
            float distance = (interest.transform.position - transform.position).sqrMagnitude;

            if (distance < closest)
            {
                closest = distance; result = interest;
            }
        }
        return result;
    }
    public void TakeDamage(float damage)
    {
        if (IsDead || IsCollected || damage <= 0f)
            
        return;

        Health = Mathf.Max(0f, Health - damage);

        if (IsDead)
        {
            Stop();

            Behaviour = "Dead";
        }
    }
    public bool Collect()
    {
        if (!IsDead || IsCollected) return false;

        IsCollected = true; 

        Behaviour = "Respawning"; 

        Stop();

        foreach (Renderer visual in visuals)

        if (visual != null) visual.enabled = false;

        foreach (Collider body in bodies)

        if (body != null) body.enabled = false;

        Sensor.Boids.Clear();

        Sensor.Hunters.Clear();

        Sensor.Interests.Clear();

        StartCoroutine(Respawn());

        return true;
    }
    private IEnumerator Respawn()
    {
        yield return new WaitForSeconds(respawnDelay);

        Vector3 position = transform.position;
        // Si no hay espacio se reintenta; nunca reaparece encima de otro cuerpo.
        while (Bounds.Instance == null || !Bounds.Instance.TryRandomPosition(transform.position.y, BodyRadius, out position))

            yield return new WaitForSeconds(0.5f);

        transform.position = position;

        Health = maxHealth; IsCollected = false; nextInteraction = Time.time + interactionInterval;

        for (int i = 0; i < visuals.Length; i++)
            
            if (visuals[i] != null) visuals[i].enabled = visualEnabled[i];

        for (int i = 0; i < bodies.Length; i++) 
            
            if (bodies[i] != null) bodies[i].enabled = colliderEnabled[i];

        InitializeVelocity(); Behaviour = "Flocking";

        Physics.SyncTransforms();
    }
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red; Gizmos.DrawWireSphere(transform.position, separationRadius);
        Gizmos.color = Color.green; Gizmos.DrawWireSphere(transform.position, alignmentRadius);
        Gizmos.color = Color.blue; Gizmos.DrawWireSphere(transform.position, choesionRadius);
    }
}
