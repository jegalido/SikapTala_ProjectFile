using UnityEngine;
using Unity.Cinemachine;


public class CameraConfinerSwitcher : MonoBehaviour
{
    [Header("Cinemachine Reference")]
    [Tooltip("Drag your CinemachineVirtualCamera here")]
    public CinemachineCamera virtualCamera;

    [Header("Confiner Boundaries Per Level")]
    [Tooltip("Index 0 = Level 1, Index 1 = Level 2, Index 2 = Level 3, etc.")]
    public PolygonCollider2D[] confinerBoundaries;

    // -- Private state --------------------------------------------------------

    private CinemachineConfiner2D confiner2D;
    private int currentIndex = -1;

    // -- Unity lifecycle ------------------------------------------------------

    private void Start()
    {
        if (virtualCamera == null)
        {
            Debug.LogWarning("CameraConfinerSwitcher: No Virtual Camera assigned!");
            return;
        }

        confiner2D = virtualCamera.GetComponent<CinemachineConfiner2D>();

        if (confiner2D == null)
        {
            Debug.LogWarning("CameraConfinerSwitcher: No CinemachineConfiner2D found on Virtual Camera!");
            return;
        }

        // Start with Level 1 boundary (index 0)
        SwitchConfiner(0);
    }

    public void SwitchConfiner(int index)
    {
        if (confiner2D == null) return;

        if (index < 0 || index >= confinerBoundaries.Length)
        {
            Debug.LogWarning("CameraConfinerSwitcher: Index " + index + " is out of range!");
            return;
        }

        if (index == currentIndex) return; // already using this boundary

        currentIndex = index;
        confiner2D.BoundingShape2D = confinerBoundaries[index];

        // Force Cinemachine to recalculate the confiner immediately
        confiner2D.InvalidateBoundingShapeCache();

        Debug.Log("CameraConfinerSwitcher: Switched to boundary index " + index);
    }

    public int GetCurrentIndex() => currentIndex;
}