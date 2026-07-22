using UnityEngine;

/// <summary>
/// Marker component. Put this on a container (or a single Selectable) whose
/// selectables should NOT show the shared UISelectionCursor frame - e.g. main
/// menu / pause buttons that already have their own selected-state feedback.
/// The cursor still works normally for everything else (sliders, toggles, etc.).
/// </summary>
public class SelectionCursorIgnore : MonoBehaviour
{
}
