using Pathfinding;
using System.Collections;
using UnityEngine;

public class Enemy01_AI : MonoBehaviour
{
    [Header("Pathfinding")]
    public Transform target;
    public float activateDistance = 25f;
    public float pathUpdateSeconds = 0.1f;

    [Header("Physics")]
    public float speed = 15f;
    public float jumpForce = 20f;
    public float nextWaypointDistance = 3f;
    public int maxPathCount;
    
    [Header("Custom Behavior")]
    public bool followEnabled = true;
    public bool jumpEnabled = true;
    public bool directionLookEnabled = true;
    [SerializeField] Vector3 startOffset;

    [Header("State")] 
    //public bool isJumping;
    public bool isInAir;
    public bool isGrounded;
    
    
    private Path path;
    private int currentWaypoint = 0;
    private Seeker seeker;
    private Rigidbody2D rb;
    private bool isOnCoolDown;
    private Rigidbody2D targetRb;
    
    public void Start()
    {
        seeker = GetComponent<Seeker>();
        rb = GetComponent<Rigidbody2D>();
        targetRb = target.GetComponent<Rigidbody2D>();
        //isJumping = false;
        isInAir = false;
        isOnCoolDown = false; 

        InvokeRepeating("UpdatePath", 0f, pathUpdateSeconds);
    }

    private void FixedUpdate()
    {
        if (TargetInDistance() && followEnabled)
        {
            PathFollow();
        }
    }

    private void UpdatePath()
    {
        if (followEnabled && TargetInDistance() && seeker.IsDone())
        {
            seeker.StartPath(rb.position, target.position, OnPathComplete);
        }
    }

    private void PathFollow()
    {
        if (path == null) return;
        // Reached end of path
        if (currentWaypoint >= path.vectorPath.Count) return;
        

        // See if colliding with anything
        startOffset = transform.position;
        isGrounded = Physics2D.Raycast(startOffset, -Vector2.up, 1.00f, LayerMask.GetMask("Ground")).collider != null;
        Debug.DrawRay(startOffset, -Vector3.up * 1f, Color.red);
        
        // Direction Calculation
        Vector2 direction = ((Vector2)path.vectorPath[currentWaypoint] - rb.position).normalized;
        Vector2 force = direction * speed;

        // Jump
        if (jumpEnabled && isGrounded && !isInAir && !isOnCoolDown)
        {
            if (target.position.y - 1f > rb.transform.position.y && targetRb.velocity.y == 0 && path.path.Count < maxPathCount)
            {
                if (isInAir) return; 
                //isJumping = true;
                rb.velocity = new Vector2(rb.velocity.x, jumpForce);
                StartCoroutine(JumpCoolDown());

            }
        }
        if (isGrounded)
        {
            //isJumping = false;
            isInAir = false; 
        }
        else
        {
            isInAir = true;
        }

        // Movement
        rb.velocity = new Vector2(force.x, rb.velocity.y);

        // Next Waypoint
        float distance = Vector2.Distance(rb.position, path.vectorPath[currentWaypoint]);
        if (distance < nextWaypointDistance)
        {
            currentWaypoint++;
        }

        // Direction Graphics Handling
        if (directionLookEnabled)
        {
            float directionSign = Mathf.Sign(rb.velocity.x);
            // Velocity check threshold to avoid flipping when nearly stationary
            if (Mathf.Abs(rb.velocity.x) > 0.05f)
            {
                transform.localScale = new Vector3(directionSign * Mathf.Abs(transform.localScale.x), transform.localScale.y, transform.localScale.z);
            }
        }
    }

    private bool TargetInDistance()
    {
        return Vector2.Distance(transform.position, target.transform.position) < activateDistance;
    }

    private void OnPathComplete(Path p)
    {
        if (!p.error)
        {
            path = p;
            currentWaypoint = 0;
        }
    }

    IEnumerator JumpCoolDown()
    {
        isOnCoolDown = true; 
        yield return new WaitForSeconds(1f);
        isOnCoolDown = false;
    }
}