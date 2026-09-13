using UnityEngine;

[RequireComponent(typeof(SphereCollider))]
public class InterestObject : MonoBehaviour
{
    // Solo contabiliza objetos; no dirige ni informa decisiones a los boids.
    public static int ActiveCount { get; private set; }

    [Min(1f)] public float maxHealth = 30f;

    public float Health { get; private set; }

    public bool IsAvailable { get { return isActiveAndEnabled && Health > 0f; } }

    public float BodyRadius { get { return GetComponent<SphereCollider>().radius * Mathf.Max(Mathf.Abs(transform.lossyScale.x), Mathf.Abs(transform.lossyScale.z)); } }

    private void OnEnable() { Health = maxHealth; ActiveCount++; }

    private void OnDisable() { ActiveCount = Mathf.Max(0, ActiveCount - 1); }

    public void TakeDamage(float damage)
    {
        if (!IsAvailable || damage <= 0f) return;

        Health = Mathf.Max(0f, Health - damage);

        if (Health <= 0f)
        {
            // Baja el contador y desactiva el collider inmediatamente.
            gameObject.SetActive(false);

            Destroy(gameObject);
        }
    }
}
