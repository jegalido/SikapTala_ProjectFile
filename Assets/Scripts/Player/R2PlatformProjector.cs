using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Attach to R2_Portal alongside R2Portal.cs.
/// Scans the world for platforms overlapping R1's area,
/// calculates where they appear inside R2, and spawns
/// invisible ghost colliders so the player can stand on them.
/// </summary>
public class R2PlatformProjector : MonoBehaviour
{
    [Header("References")]
    public R1Lens r1Lens;
    public R2Portal r2Portal;

    [Tooltip("Set this to your Ground layer")]
    public LayerMask platformLayer;

    // -- Private state --------------------------------------------------------

    private List<GameObject> ghostColliders = new List<GameObject>();

    // -- Unity lifecycle ------------------------------------------------------

    private void FixedUpdate()
    {
        // Only project when R2 is visible (Shift held)
        if (!gameObject.activeSelf)
        {
            ClearGhosts();
            return;
        }

        ProjectPlatforms();
    }

    // -- Platform projection --------------------------------------------------

    private void ProjectPlatforms()
    {
        ClearGhosts();

        Bounds r1Bounds = r1Lens.WorldBounds;
        Bounds r2Bounds = r2Portal.GetComponent<Collider2D>().bounds;

        // Find all platform colliders inside R1's world bounds
        Collider2D[] hits = Physics2D.OverlapBoxAll(
            r1Bounds.center,
            r1Bounds.size,
            0f,
            platformLayer
        );

        foreach (var hit in hits)
        {
            Bounds pb = hit.bounds;

            // Calculate relative position inside R1 (0..1 range)
            float relX = (pb.center.x - r1Bounds.min.x) / r1Bounds.size.x;
            float relY = (pb.center.y - r1Bounds.min.y) / r1Bounds.size.y;
            float relW = pb.size.x / r1Bounds.size.x;
            float relH = pb.size.y / r1Bounds.size.y;

            // Map to R2 screen space
            float ghostX = r2Bounds.min.x + relX * r2Bounds.size.x;
            float ghostY = r2Bounds.min.y + relY * r2Bounds.size.y;
            float ghostW = relW * r2Bounds.size.x;
            float ghostH = relH * r2Bounds.size.y;

            // Create ghost collider
            GameObject ghost = new GameObject("GhostPlatform");
            ghost.transform.position = new Vector3(ghostX, ghostY, 0);
            ghost.layer = LayerMask.NameToLayer("Ground");

            BoxCollider2D bc = ghost.AddComponent<BoxCollider2D>();
            bc.size = new Vector2(ghostW, ghostH);

            ghostColliders.Add(ghost);
        }
    }

    private void ClearGhosts()
    {
        foreach (var g in ghostColliders)
        {
            if (g != null) Destroy(g);
        }
        ghostColliders.Clear();
    }

    private void OnDestroy()
    {
        ClearGhosts();
    }
}