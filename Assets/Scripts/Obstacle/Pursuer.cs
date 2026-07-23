using UnityEngine;

/// <summary>
/// A reality-bound haunt that chases the player. Dormant (invisible, frozen at home)
/// in the warm/illusion world; wakes and hunts only in the dark/real world.
///
/// - Adjustable move speed; Floating (chases in 2D) or Walking (horizontal only).
/// - Optional patrol left<->right over a set distance while it hasn't seen you.
/// - Detection zone = a child collider you resize with the Scale tool.
/// - Optional line-of-sight (won't see you through walls).
/// - Leash: after losing you it hunts your last-seen spot, then returns home.
/// - Catch: touching you sends you to the last checkpoint (sanity kept).
/// </summary>
public class Pursuer : MonoBehaviour
{
    public enum MoveStyle { Floating, Walking }

    [Header("Movement")]
    public MoveStyle moveStyle = MoveStyle.Floating;
    public float moveSpeed = 3.2f;

    [Header("Patrol (used when it hasn't seen you)")]
    public bool isPatrolling = false;
    [Tooltip("Total left<->right patrol span in meters, centered on the start position.")]
    public float patrolDistance = 4f;
    public float patrolSpeed = 1.5f;

    [Header("Detection")]
    [Tooltip("Child collider whose SCALE defines the detection zone. Resize it with the Scale tool.")]
    public Collider2D detectionZone;
    [Tooltip("Fallback radius used if no detection zone collider is assigned.")]
    public float detectionRadius = 4f;
    public bool requireLineOfSight = false;
    [Tooltip("Layers that block sight (walls / floor).")]
    public LayerMask sightBlockers;
    [Tooltip("Seconds it keeps hunting your last-seen spot after losing you.")]
    public float loseInterestTime = 2f;

    [Header("Catch")]
    [Tooltip("Distance at which it grabs you and sends you to the last checkpoint.")]
    public float catchDistance = 0.7f;

    [Header("Reality")]
    [Tooltip("Only awake / dangerous in the dark (real) world; dormant in the warm world.")]
    public bool darkWorldOnly = true;

    [Header("References (auto-found if empty)")]
    public Transform player;
    public InsanityVisionEffect vision;
    public InsanityBar insanityBar;

    private enum State { Idle, Patrol, Chase, Hunt, Return }
    private State state = State.Idle;
    private Vector3 origin;
    private Vector3 lastKnownPos;
    private float loseTimer;
    private int patrolDir = 1;
    private bool dormant;
    private bool facingRight = true;
    private SpriteRenderer[] visuals;

    private void Start()
    {
        origin = transform.position;
        if (player == null)
        {
            GameObject p = GameObject.FindGameObjectWithTag("Player");
            if (p != null) player = p.transform;
        }
        if (vision == null) vision = FindFirstObjectByType<InsanityVisionEffect>();
        if (insanityBar == null) insanityBar = FindFirstObjectByType<InsanityBar>();
        visuals = GetComponentsInChildren<SpriteRenderer>(true);
        state = isPatrolling ? State.Patrol : State.Idle;
    }

private void Update()
    {
        if (ScreenFader.Instance != null && ScreenFader.Instance.IsTransitioning) return;

        bool awake = !darkWorldOnly || (vision != null && vision.ShiftBlend > 0.5f);
        SetDormant(!awake);
        if (!awake || player == null) return;

        bool sees = CanSeePlayer();

        if (sees)
        {
            state = State.Chase;
            lastKnownPos = player.position;
            loseTimer = loseInterestTime;
        }
        else if (state == State.Chase)
        {
            state = State.Hunt;
        }
        else if (state == State.Hunt)
        {
            loseTimer -= Time.deltaTime;
            if (loseTimer <= 0f) state = State.Return;
        }

        switch (state)
        {
            case State.Chase: MoveToward(player.position, moveSpeed); break;
            case State.Hunt: MoveToward(lastKnownPos, moveSpeed); break;
            case State.Return:
                MoveToward(origin, moveSpeed);
                if (Vector2.Distance(transform.position, origin) < 0.15f)
                    state = isPatrolling ? State.Patrol : State.Idle;
                break;
            case State.Patrol: Patrol(); break;
        }

        if (Vector2.Distance(transform.position, player.position) <= catchDistance)
            Catch();
    }

    private bool CanSeePlayer()
    {
        bool inZone = detectionZone != null
            ? detectionZone.OverlapPoint(player.position)
            : Vector2.Distance(transform.position, player.position) <= detectionRadius;
        if (!inZone) return false;

        if (requireLineOfSight)
        {
            Vector2 from = transform.position;
            Vector2 dir = (Vector2)player.position - from;
            RaycastHit2D hit = Physics2D.Raycast(from, dir.normalized, dir.magnitude, sightBlockers);
            if (hit.collider != null) return false;
        }
        return true;
    }

    private void MoveToward(Vector3 target, float speed)
    {
        Vector3 pos = transform.position;
        Vector3 next = (moveStyle == MoveStyle.Floating)
            ? Vector3.MoveTowards(pos, new Vector3(target.x, target.y, pos.z), speed * Time.deltaTime)
            : new Vector3(Mathf.MoveTowards(pos.x, target.x, speed * Time.deltaTime), pos.y, pos.z);
        transform.position = next;
        FaceMoveDir(next.x - pos.x);
    }

    private void Patrol()
    {
        float leftX = origin.x - patrolDistance * 0.5f;
        float rightX = origin.x + patrolDistance * 0.5f;
        float targetX = patrolDir > 0 ? rightX : leftX;
        Vector3 pos = transform.position;
        float nx = Mathf.MoveTowards(pos.x, targetX, patrolSpeed * Time.deltaTime);
        float ny = (moveStyle == MoveStyle.Floating)
            ? Mathf.MoveTowards(pos.y, origin.y, patrolSpeed * Time.deltaTime)
            : pos.y;
        transform.position = new Vector3(nx, ny, pos.z);
        FaceMoveDir(nx - pos.x);
        if (Mathf.Abs(nx - targetX) < 0.05f) patrolDir = -patrolDir;
    }

private void FaceMoveDir(float dx)
    {
        if (Mathf.Abs(dx) < 0.0001f) return;
        bool right = dx > 0f;
        if (right != facingRight)
        {
            facingRight = right;
            // Flip the sprite only (not the transform), so child objects like the
            // chatbox are never mirrored.
            if (visuals != null)
                foreach (SpriteRenderer v in visuals) if (v != null) v.flipX = !right;
        }
    }

private void Catch()
    {
        if (player == null) return;
        Vector3 target = insanityBar != null ? insanityBar.GetLastCheckpointPosition() : player.position;
        ScreenFader.Respawn(player, target);
        // No snap-home: the leash (Return state) brings it back after it loses you.
    }

private void SetDormant(bool value)
    {
        if (value == dormant) return;
        dormant = value;
        if (visuals != null)
            foreach (SpriteRenderer v in visuals) if (v != null) v.enabled = !value;
        // Keeps its position and state while dormant, so it resumes from where it was
        // when reality returns. It only heads home via the Return state (after losing you).
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = new Color(1f, 0.3f, 0.3f, 0.6f);
        if (detectionZone != null)
        {
            Bounds b = detectionZone.bounds;
            Gizmos.DrawWireCube(b.center, b.size);
        }
        else
        {
            Gizmos.DrawWireSphere(transform.position, detectionRadius);
        }

        Gizmos.color = new Color(0.3f, 0.6f, 1f, 0.7f);
        Vector3 o = Application.isPlaying ? origin : transform.position;
        Gizmos.DrawLine(o + Vector3.left * patrolDistance * 0.5f, o + Vector3.right * patrolDistance * 0.5f);
    }
}
