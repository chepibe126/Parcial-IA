using UnityEngine;

[RequireComponent(typeof(SphereCollider))]
public class Agent : MonoBehaviour
{
    protected Vector3 _velocity;
    public Vector3 Velocity { get { return _velocity; } }
    public float BodyRadius
    {
        get { return GetComponent<SphereCollider>().radius * Mathf.Max(Mathf.Abs(transform.lossyScale.x), Mathf.Abs(transform.lossyScale.z)); }
    }

    public void Stop() { _velocity = Vector3.zero; }

    // El movimiento se subdivide para no atravesar otro cuerpo a FPS bajos.
    // No hay un controlador global: cada agente consulta su entorno inmediato.
    protected void MoveAgent(float maxSpeed)
    {
        _velocity.y = 0f;

        _velocity = Vector3.ClampMagnitude(_velocity, maxSpeed);

        Vector3 displacement = _velocity * Time.deltaTime;

        int steps = Mathf.Max(1, Mathf.CeilToInt(displacement.magnitude / 0.15f));

        for (int i = 0; i < steps; i++)
        {
            Vector3 next = transform.position + displacement / steps;

            if (Bounds.Instance != null) next = Bounds.Instance.OutOfBounds(next);

            if (!HasFreeSpace(next, BodyRadius, this))
            {
                // Desvio local alrededor de cuerpos: evita quedar trabado
                // contra un objeto de interes o un vecino detenido.
                Vector3 step = displacement / steps;

                bool found = false;

                for (int side = 0; side < 2; side++)
                {
                    Vector3 alternative = Quaternion.Euler(0f, side == 0 ? 80f : -80f, 0f) * step;

                    Vector3 candidate = transform.position + alternative;

                    if (Bounds.Instance != null) candidate = Bounds.Instance.OutOfBounds(candidate);

                    if (!HasFreeSpace(candidate, BodyRadius, this)) continue;

                    next = candidate; found = true;

                    _velocity = alternative.normalized * _velocity.magnitude;

                    break;
                }

                if (!found) { Stop(); break; }
            }
            transform.position = next;
            // Los siguientes sensores deben consultar las posiciones actuales.
            Physics.SyncTransforms();
        }

        if (_velocity.sqrMagnitude > 0.0001f) transform.forward = _velocity.normalized;
    }

    public static bool HasFreeSpace(Vector3 position, float radius, Agent ignore = null)
    {
        Collider[] hits = Physics.OverlapSphere(position, radius + 0.05f, Physics.AllLayers, QueryTriggerInteraction.Collide);

        foreach (Collider hit in hits)
        {
            Agent other = hit.GetComponentInParent<Agent>();

            if (other != null)
            {
                if (other == ignore) continue;

                if (Vector3.Distance(position, other.transform.position) < radius + other.BodyRadius + 0.02f) return false;
            }
            InterestObject interest = hit.GetComponentInParent<InterestObject>();

            if (interest != null && interest.IsAvailable)
            {
                // El cazador deja el objeto en su posicion y puede salir de el.
                // Los boids siguen manteniendo distancia del objeto.
                if (ignore is FSMAgent) continue;

                float distance = Vector3.Distance(position, interest.transform.position);

                if (distance < radius + interest.BodyRadius + 0.02f) return false;
            }
        }

        return true;
    }
}
