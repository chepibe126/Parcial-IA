using UnityEngine;

public class Bounds : MonoBehaviour
{
    public static Bounds Instance { get; private set; }

    [SerializeField, Min(5f)] private float width = 40f;
    [SerializeField, Min(5f)] private float height = 30f;
    [SerializeField] private bool drawGizmos = true;

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }

        Instance = this;
    }
    private void OnDestroy() { if (Instance == this) Instance = null; }

    public Vector3 OutOfBounds(Vector3 position)
    {
        Vector3 center = transform.position;

        if (position.x > center.x + width / 2f) position.x = center.x - width / 2f;

        else if (position.x < center.x - width / 2f) position.x = center.x + width / 2f;

        if (position.z > center.z + height / 2f) position.z = center.z - height / 2f;

        else if (position.z < center.z - height / 2f) position.z = center.z + height / 2f;

        return position;
    }
    public bool TryRandomPosition(float y, float radius, out Vector3 position)
    {
        for (int i = 0; i < 60; i++)
        {
            position = new Vector3(transform.position.x + Random.Range(-width / 2f + radius, width / 2f - radius), y,
                transform.position.z + Random.Range(-height / 2f + radius, height / 2f - radius));

            if (Agent.HasFreeSpace(position, radius)) return true;
        }

        position = default;

        return false;
    }
    private void OnDrawGizmos()
    {
        if (!drawGizmos) return;

        Gizmos.color = Color.yellow;
        Gizmos.DrawWireCube(transform.position, new Vector3(width, 0f, height));
    }
}
