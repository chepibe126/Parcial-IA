using System.Collections.Generic;
using UnityEngine;

public class AgentSensor : MonoBehaviour
{
    [Min(0.1f)] public float perceptionRadius = 8f;
    [Range(1f, 360f)] public float visionAngle = 360f;

    public readonly List<AdvancesAgent> Boids = new List<AdvancesAgent>();
    public readonly List<FSMAgent> Hunters = new List<FSMAgent>();
    public readonly List<InterestObject> Interests = new List<InterestObject>();

    public bool CanSee(Transform candidate)
    {
        if (candidate == null || !candidate.gameObject.activeInHierarchy)
            
         return false;

        Vector3 offset = candidate.position - transform.position;

        if (offset.magnitude > perceptionRadius) 
            
        return false;

        if (visionAngle < 360f && Vector3.Angle(transform.forward, offset) > visionAngle * 0.5f)
            
        return false;

        return true;
    }
    public void Scan()
    {
        Boids.Clear();
        Hunters.Clear(); 
        Interests.Clear();

        Collider[] hits = Physics.OverlapSphere(transform.position, perceptionRadius, Physics.AllLayers, QueryTriggerInteraction.Collide);

        foreach (Collider hit in hits)
        {
            AdvancesAgent boid = hit.GetComponentInParent<AdvancesAgent>();

            if (boid != null && boid.transform != transform && boid.isActiveAndEnabled && !boid.IsCollected && CanSee(boid.transform) && !Boids.Contains(boid))
                
                Boids.Add(boid);

            FSMAgent hunter = hit.GetComponentInParent<FSMAgent>();

            if (hunter != null && hunter.transform != transform && hunter.isActiveAndEnabled && CanSee(hunter.transform) && !Hunters.Contains(hunter))
                
                Hunters.Add(hunter);

            InterestObject interest = hit.GetComponentInParent<InterestObject>();

            if (interest != null && interest.IsAvailable && CanSee(interest.transform) && !Interests.Contains(interest)) 
                
                Interests.Add(interest);
        }
    }
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;

        Gizmos.DrawWireSphere(transform.position, perceptionRadius);

        if (visionAngle >= 360f)
            
         return;

        Gizmos.DrawRay(transform.position, Quaternion.Euler(0, -visionAngle / 2f, 0) * transform.forward * perceptionRadius);

        Gizmos.DrawRay(transform.position, Quaternion.Euler(0, visionAngle / 2f, 0) * transform.forward * perceptionRadius);
    }
}
