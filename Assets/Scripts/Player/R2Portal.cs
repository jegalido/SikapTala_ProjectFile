using UnityEngine;

/// <summary>
/// Attach to R2_Portal GameObject.
/// R2 displays what R1 is framing using a RenderTexture feed from a portal camera.
/// Also draggable by the player.
/// Hidden by default, shown only when Shift is held via InsanityVisionEffect.
/// </summary>
public class R2Portal : MonoBehaviour
{
    [Header("References")]
    [Tooltip("Drag your R1_Lens GameObject here")]
    public R1Lens r1Lens;

    [Tooltip("The secondary camera that renders R1's view into the RenderTexture")]
    public Camera portalCamera;

    [Tooltip("Assign the RenderTexture asset here")]
    public RenderTexture renderTex;

    [Tooltip("The Quad child that displays the RenderTexture (R2_Display)")]
    public Renderer r2Renderer;

    [Header("Drag Settings")]
    public bool playerCanDrag = true;

    // -- Private state --------------------------------------------------------

    private Camera mainCam;
    private bool isDragging;
    private Vector3 dragOffset;

    // -- Unity lifecycle ------------------------------------------------------

    private void Start()
    {
        mainCam = Camera.main;

        if (portalCamera != null)
            portalCamera.targetTexture = renderTex;

        if (r2Renderer != null && renderTex != null)
            r2Renderer.material.mainTexture = renderTex;
    }

    private void LateUpdate()
    {
        if (!gameObject.activeSelf) return;

        // Move portal camera to match R1's world position every frame
        if (r1Lens != null && portalCamera != null)
        {
            portalCamera.transform.position = new Vector3(
                r1Lens.transform.position.x,
                r1Lens.transform.position.y,
                portalCamera.transform.position.z
            );

            // Match orthographic size to R1's height so it frames exactly what R1 covers
            Bounds b = r1Lens.WorldBounds;
            portalCamera.orthographicSize = b.extents.y;
            portalCamera.aspect = b.size.x / b.size.y;
        }

        if (playerCanDrag)
            HandleMouseDrag();
    }

    // -- Drag logic -----------------------------------------------------------

    private void HandleMouseDrag()
    {
        if (Input.GetMouseButtonDown(0))
        {
            Vector3 mouseWorld = mainCam.ScreenToWorldPoint(Input.mousePosition);
            mouseWorld.z = 0;

            if (GetComponent<Collider2D>().OverlapPoint(mouseWorld))
            {
                if (DragManager.Instance.TryStartDrag(this))
                {
                    isDragging = true;
                    dragOffset = transform.position - mouseWorld;
                }
            }
        }

        if (Input.GetMouseButtonUp(0))
        {
            isDragging = false;
            DragManager.Instance.StopDrag(this);
        }

        if (isDragging && DragManager.Instance.IsDragging(this))
        {
            Vector3 mouseWorld = mainCam.ScreenToWorldPoint(Input.mousePosition);
            mouseWorld.z = 0;
            transform.position = mouseWorld + dragOffset;
        }
    }
}