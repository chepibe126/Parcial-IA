using UnityEngine;

public class AgentSteering : Agent
{
    [Header("Stats")]
    [SerializeField, Min(0.1f)] protected float _maxSpeed = 3f;
    [SerializeField, Min(0.1f)] protected float _maxSteering = 8f;
    [SerializeField, Min(0.1f)] protected float slowingDistance = 3f;
    

   
    protected Vector3 CaculateStering(Vector3 desired)
    {
        desired.y = 0f;

        return Vector3.ClampMagnitude(desired - _velocity, _maxSteering * Time.deltaTime);
    }
    protected Vector3 DesiredVector(Vector3 position)
    { 
        return (position - transform.position).normalized * _maxSpeed;
    }

    public Vector3 Seek(Vector3 position) 
    { 
        return CaculateStering(DesiredVector(position));
    }

    public Vector3 Flee(Vector3 position) 
    { 
        return CaculateStering(-DesiredVector(position)); 
    }

    public Vector3 Arrive(Vector3 position, float stopDistance)
    {
        Vector3 direction = position - transform.position;

        direction.y = 0f;

        float remaining = direction.magnitude - stopDistance;
        
        if (remaining <= 0f) return CaculateStering(Vector3.zero);

        float speed = _maxSpeed * Mathf.Clamp01(remaining / Mathf.Max(0.1f, slowingDistance));
       
        speed = Mathf.Min(speed, Mathf.Sqrt(2f * _maxSteering * remaining));

        return CaculateStering(direction.normalized * speed);
    }

    protected Vector3 CalculateFuture(Agent other)
    {
        float distance = Vector3.Distance(other.transform.position, transform.position);

        float prediction = Mathf.Min(1.5f, distance / Mathf.Max(0.1f, _maxSpeed + other.Velocity.magnitude));

        return other.transform.position + other.Velocity * prediction;
    }

    public Vector3 Pursuit(Agent other)
    { 
        return Seek(CalculateFuture(other));
    }

    public Vector3 Evade(Agent other)
    { 
        return Flee(CalculateFuture(other));
    }

    public void ApplySteering(Vector3 steering)
    {
        _velocity += Vector3.ClampMagnitude(steering, _maxSteering * Time.deltaTime);

        MoveAgent(_maxSpeed);
    }
}
