using UnityEngine;

/// <summary>
/// Attach to R1_Lens GameObject.
/// R1 is a draggable viewport lens — the player drags it over the alternate map.
/// R2Portal reads R1's position to know what area to display.
/// R1 is hidden by default and only shown when Shift is held
/// via the InsanityVisionEffect hiddenObjects array.
/// </summary>
public class R1Lens : MonoBehaviour
{
    [Header("Drag Settings")]
    [Tooltip("Allow the player to drag R1 with the mouse")]
    public bool playerCanDrag = true;

    [Header("Bounds — keep R1 inside the Level 3 map area")]
    public Vector2 minPosition;
    public Vector2 maxPosition;

    // -- Private state --------------------------------------------------------

    private Camera mainCam;
    private bool isDragging = false;
    private Vector3 dragOffset;

    // R2Portal reads this to know what world area to display
    public Bounds WorldBounds => GetComponent<Collider2D>().bounds;

    // -- Unity lifecycle ------------------------------------------------------

    private void Start()
    {
        mainCam = Camera.main;
    }

    private void Update()
    {
        // Only allow dragging when this object is active (i.e. Shift is held)
        if (!playerCanDrag) return;
        if (!gameObject.activeSelf) return;

        HandleMouseDrag();
        ClampPosition();
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

    private void ClampPosition()
    {
        transform.position = new Vector3(
            Mathf.Clamp(transform.position.x, minPosition.x, maxPosition.x),
            Mathf.Clamp(transform.position.y, minPosition.y, maxPosition.y),
            transform.position.z
        );
    }

    // -- Editor helper --------------------------------------------------------

    private void OnDrawGizmos()
    {
        Gizmos.color = new Color(0f, 1f, 0.6f, 0.3f);
        Gizmos.DrawWireCube(transform.position, transform.localScale);
    }
}