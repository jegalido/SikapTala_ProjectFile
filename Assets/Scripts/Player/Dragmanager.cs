using UnityEngine;

/// <summary>
/// Attach to your existing GameManager or CameraManager.
/// Prevents R1 and R2 from both being dragged at the same time.
/// Whichever the player clicks first gets priority.
/// </summary>
public class DragManager : MonoBehaviour
{
    public static DragManager Instance;

    private MonoBehaviour currentDragTarget = null;

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }

    public bool TryStartDrag(MonoBehaviour requester)
    {
        if (currentDragTarget == null)
        {
            currentDragTarget = requester;
            return true;
        }
        return false;
    }

    public void StopDrag(MonoBehaviour requester)
    {
        if (currentDragTarget == requester)
            currentDragTarget = null;
    }

    public bool IsDragging(MonoBehaviour requester)
        => currentDragTarget == requester;
}