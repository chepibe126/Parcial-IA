using UnityEngine;

public class AgentFeedback : MonoBehaviour
{
    [SerializeField] private bool showHUD = true;

    private AdvancesAgent boid;
    private FSMAgent hunter;
    private Renderer visual;
    private MaterialPropertyBlock properties;

    private void Awake()
    {
        boid = GetComponent<AdvancesAgent>();
        hunter = GetComponent<FSMAgent>();

        visual = GetComponentInChildren<Renderer>();
        properties = new MaterialPropertyBlock();
    }

    private void LateUpdate()
    {
        if (visual == null)
            return;

        Color color = Color.white;

        if (boid != null)
        {
            if (boid.IsCollected)
                return;

            if (boid.IsDead)
                color = Color.gray;

            else if (boid.Behaviour == "Evade")

                color = new Color(1f, 0.45f, 0.05f);

            else if (boid.Behaviour == "Interact")

                color = Color.green;

            else if (boid.Behaviour == "Arrive")

                color = Color.cyan;

            else
                color = new Color(0.2f, 0.45f, 1f);
        }

        if (hunter != null)
        {
            if (hunter.StateName == "Attack")

                color = Color.red;

            else if (hunter.StateName == "Gather")

                color = Color.magenta;

            else

                color = Color.yellow;
        }

        visual.GetPropertyBlock(properties);

        properties.SetColor("_BaseColor", color);

        properties.SetColor("_Color", color);

        visual.SetPropertyBlock(properties);
    }

    private void OnGUI()
    {
        if (!showHUD || hunter == null || hunter.Sensor == null)
            return;

        GUI.Box( new Rect(10, 10, 380, 185), "Cazador - FSM");
        GUI.Label( new Rect(20, 35, 360, 22),"Estado: " + hunter.StateName + " | TBA: " + hunter.AttackRemaining.ToString("F1"));

        string targetName = "Ninguno";

        if (hunter.CurrentTarget != null)

            targetName = hunter.CurrentTarget.name;

        GUI.Label(new Rect(20, 60, 360, 22),"Objetivo: " + targetName);

        GUI.Label(new Rect(20, 85, 360, 22),"Detectados: " + hunter.Sensor.Boids.Count +" | Objetos: " + InterestObject.ActiveCount + "/5");

        GUI.Label(new Rect(20, 110, 360, 22), hunter.Action);

        GUI.Label(new Rect(20, 135, 360, 22),"Ultimo ataque: " + hunter.LastAttack);

        GUI.Label(new Rect(20, 160, 360, 45), "Azul: grupo | naranja: huida | gris: muerto | cyan: vio cebo | verde: interactua");
    }

    private void OnDrawGizmosSelected()
    {
        if (hunter == null)

            hunter = GetComponent<FSMAgent>();

        if (hunter == null)
            return;

        Gizmos.color = Color.yellow; 

        Gizmos.DrawWireSphere(hunter.transform.position,hunter.RangeAttackRadius);

        Gizmos.color = Color.red;

        Gizmos.DrawWireSphere( hunter.transform.position,hunter.MeleeAttackRadius);
    }

    private void OnDrawGizmos()
    {
        if (!Application.isPlaying || hunter == null)
            return;

        if (Time.time < hunter.ShotUntil)
        {
            Gizmos.color = Color.yellow;

            Gizmos.DrawLine(hunter.transform.position, hunter.ShotEnd);
        }
    }
}
