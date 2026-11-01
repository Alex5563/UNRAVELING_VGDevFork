using UnityEngine;
public class HandController : MonoBehaviour
{
    [Header("Inertia")]
    [SerializeField] float stiffness = 60f; // pull strength
    [SerializeField] float damping = 10f; // drag
    [Header("Reach")]
    [SerializeField] Camera playerCamera;
    [SerializeField] float handDepth = 3.5f; // distance in front of cam
    [Header("Rotation")]
    [SerializeField] float rotationSmoothing = 15f;
    [SerializeField] float swayTilt = 4f;
    [SerializeField] float maxTilt = 25f;
    [Header("Swat")]
    [SerializeField] float hitRadius = 0.4f;
    [SerializeField] float swatCooldown = 0.50f;
    [SerializeField] float minimumSwatSpeed = 15f;

    [Header("Stats")]
    public int hits;
    Vector3 velocity;
    float cooldown;
    Quaternion restRotation;

    void Start()
    {
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = false;
        restRotation = Quaternion.Inverse(playerCamera.transform.rotation) * transform.rotation;
        // Start at the target so hand doesn't move eradically during game start
        transform.position = GetTarget();
    }
    // so the hand can't read the camera before it has turned
    void LateUpdate()
    {
        float dt = Mathf.Min(Time.deltaTime, 0.05f);

        if (dt <= 0f)
        {
            return;
        }
        // Remember where the hand started
        Vector3 previousPosition = transform.position;

        // physics formula for hand 
        Vector3 acceleration = stiffness * (GetTarget() - transform.position) - damping * velocity; 
        velocity += acceleration * dt;
        transform.position += velocity * dt;
        cooldown -= dt;

        UpdateRotation(dt);
          
        // A fast hand movement triggers the swat
        if (velocity.magnitude >= minimumSwatSpeed && cooldown <= 0f)
        {
            bool hitSomething = Swat(previousPosition);

            if (hitSomething)
            {
                cooldown = swatCooldown;
            }
        }
    }

    void UpdateRotation(float dt)
    {
        Transform cam = playerCamera.transform;

        Vector3 localVel = cam.InverseTransformDirection(velocity);
        float roll  = Mathf.Clamp(-localVel.x * swayTilt, -maxTilt, maxTilt);
        float pitch = Mathf.Clamp(-localVel.y * swayTilt, -maxTilt, maxTilt);

        Quaternion target = cam.rotation * Quaternion.Euler(pitch, 0f, roll) * restRotation;

        float t = 1f - Mathf.Exp(-rotationSmoothing * dt);
        transform.rotation = Quaternion.Slerp(transform.rotation, target, t);
    }

    bool Swat(Vector3 previousPosition)
    {
        //changed to capsule to check path from prev to current position
        Collider[] found = Physics.OverlapCapsule(previousPosition, transform.position, hitRadius, Physics.AllLayers, QueryTriggerInteraction.Collide);
        Debug.Log("Colliders found: " + found.Length);

        bool hitSomething = false;

        // Track fireflies hit during this swat so each one counts only once
        var hitThisSwat = new System.Collections.Generic.HashSet<FireflyParticle>();

        foreach (Collider c in found)
        {
            // dont detect the hand itself and children objects
            if (c.transform == transform || c.transform.IsChildOf(transform))
                continue;
            Debug.Log("Found collider: " + c.name);
            
            FireflyParticle firefly = c.GetComponentInParent<FireflyParticle>();
           if (firefly != null && hitThisSwat.Add(firefly))
            {
                Debug.Log("FOUND FIREFLY!");
                hits++;
                firefly.Despawn();
                hitSomething = true;
                Debug.Log("HIT " + hits);
            }
        }
        return hitSomething;
    }

    // Helper method that works out where the hand wants to be
    Vector3 GetTarget()
    {
        Ray ray = playerCamera.ScreenPointToRay(Input.mousePosition);
        Plane plane = new Plane(-playerCamera.transform.forward,
                                playerCamera.transform.position + playerCamera.transform.forward * handDepth);
        return plane.Raycast(ray, out float dist) 
            ? ray.GetPoint(dist) 
            : transform.position;
    }
    //to visually show the hit radius when gizmos is turned on
    void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, hitRadius);
    }
}