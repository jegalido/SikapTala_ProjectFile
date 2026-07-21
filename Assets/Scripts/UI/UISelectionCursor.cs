using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using UnityEngine.InputSystem;

/// <summary>
/// A highlight frame that follows whichever UI Selectable is selected by the
/// EventSystem. Can be limited to keyboard / gamepad use (hidden while the mouse
/// is being used) and can optionally drive selection so the frame reveals itself
/// the moment the player starts navigating with the keyboard or a controller.
/// Uses unscaled time so it works while the game is paused.
/// </summary>
[RequireComponent(typeof(RectTransform))]
public class UISelectionCursor : MonoBehaviour
{
    [Header("Look & Feel")]
    [SerializeField] private Vector2 padding = new Vector2(30f, 16f);
    [SerializeField] private float moveSpeed = 16f;
    [SerializeField] private float fadeSpeed = 14f;
    [SerializeField] private CanvasGroup canvasGroup;

    [Header("Input-mode behaviour")]
    [Tooltip("Only show the frame while navigating with keyboard / gamepad; hide it when the mouse is used.")]
    [SerializeField] private bool hideWhenUsingMouse = false;
    [Tooltip("Let this cursor drive selection: restore a selection when navigation resumes, and reveal (without moving) on the first key press. Enable for menus that use Unity's built-in navigation; leave OFF for menus with their own navigation controller.")]
    [SerializeField] private bool manageSelection = false;
    [Tooltip("Element to select when navigation resumes and nothing valid is selected.")]
    [SerializeField] private GameObject fallbackSelection;

    private enum InputMode { Pointer, Nav }

    private RectTransform rect;
    private RectTransform parentRect;
    private Canvas canvas;
    private bool snapNext;
    private InputMode mode = InputMode.Pointer;
    private InputMode prevMode = InputMode.Pointer;
    private GameObject lastSelected;

    private void Awake()
    {
        rect = GetComponent<RectTransform>();
        parentRect = transform.parent as RectTransform;
        canvas = GetComponentInParent<Canvas>();
        if (canvasGroup == null) canvasGroup = GetComponent<CanvasGroup>();
        rect.anchorMin = rect.anchorMax = rect.pivot = new Vector2(0.5f, 0.5f);
    }

    private void OnEnable()
    {
        snapNext = true;
        if (canvasGroup != null) canvasGroup.alpha = 0f;
        mode = InputMode.Pointer;
        prevMode = InputMode.Pointer;
    }

    private void Update()
    {
        UpdateInputMode();

        if (manageSelection && mode == InputMode.Nav && CurrentValidSelection() == null)
        {
            GameObject target = IsValid(lastSelected) ? lastSelected : fallbackSelection;
            if (IsValid(target) && EventSystem.current != null)
            {
                EventSystem.current.SetSelectedGameObject(target);
                lastSelected = target;
            }
        }
    }

    private void UpdateInputMode()
    {
        if (!hideWhenUsingMouse)
        {
            mode = InputMode.Nav; // gating disabled: visible whenever something is selected
            return;
        }
        if (NavInputThisFrame()) mode = InputMode.Nav;
        else if (PointerInputThisFrame()) mode = InputMode.Pointer;
        // otherwise keep the current mode (sticky)
    }

    private bool NavInputThisFrame()
    {
        Keyboard kb = Keyboard.current;
        if (kb != null)
        {
            if (kb.upArrowKey.wasPressedThisFrame || kb.downArrowKey.wasPressedThisFrame ||
                kb.leftArrowKey.wasPressedThisFrame || kb.rightArrowKey.wasPressedThisFrame ||
                kb.wKey.wasPressedThisFrame || kb.aKey.wasPressedThisFrame ||
                kb.sKey.wasPressedThisFrame || kb.dKey.wasPressedThisFrame ||
                kb.enterKey.wasPressedThisFrame || kb.numpadEnterKey.wasPressedThisFrame ||
                kb.escapeKey.wasPressedThisFrame || kb.tabKey.wasPressedThisFrame)
                return true;
        }
        Gamepad gp = Gamepad.current;
        if (gp != null)
        {
            if (gp.dpad.up.wasPressedThisFrame || gp.dpad.down.wasPressedThisFrame ||
                gp.dpad.left.wasPressedThisFrame || gp.dpad.right.wasPressedThisFrame ||
                gp.buttonSouth.wasPressedThisFrame || gp.buttonEast.wasPressedThisFrame ||
                gp.buttonWest.wasPressedThisFrame || gp.buttonNorth.wasPressedThisFrame ||
                gp.startButton.wasPressedThisFrame)
                return true;
            if (gp.leftStick.ReadValue().sqrMagnitude > 0.25f) return true;
        }
        return false;
    }

    private bool PointerInputThisFrame()
    {
        Mouse m = Mouse.current;
        if (m == null) return false;
        if (m.delta.ReadValue().sqrMagnitude > 4f) return true;
        if (m.leftButton.wasPressedThisFrame || m.rightButton.wasPressedThisFrame || m.middleButton.wasPressedThisFrame) return true;
        if (Mathf.Abs(m.scroll.ReadValue().y) > 0.01f) return true;
        return false;
    }

    private static bool IsValid(GameObject g)
    {
        return g != null && g.activeInHierarchy && g.GetComponent<Selectable>() != null;
    }

    private GameObject CurrentValidSelection()
    {
        var es = EventSystem.current;
        if (es == null) return null;
        GameObject g = es.currentSelectedGameObject;
        return IsValid(g) ? g : null;
    }

    private void LateUpdate()
    {
        GameObject selGO = CurrentValidSelection();

        // On the first frame navigation resumes, reveal at the last item without moving.
        bool justEnteredNav = mode == InputMode.Nav && prevMode != InputMode.Nav;
        if (manageSelection && justEnteredNav && IsValid(lastSelected) && EventSystem.current != null)
        {
            EventSystem.current.SetSelectedGameObject(lastSelected);
            selGO = lastSelected;
        }

        bool show = mode == InputMode.Nav && selGO != null;

        if (show)
        {
            PositionOver(selGO.transform as RectTransform);
            if (canvasGroup != null)
                canvasGroup.alpha = Mathf.MoveTowards(canvasGroup.alpha, 1f, Time.unscaledDeltaTime * fadeSpeed);
            lastSelected = selGO;
        }
        else
        {
            if (canvasGroup != null)
                canvasGroup.alpha = Mathf.MoveTowards(canvasGroup.alpha, 0f, Time.unscaledDeltaTime * fadeSpeed);
            snapNext = true;
            if (selGO != null) lastSelected = selGO;
        }

        prevMode = mode;
    }

    private void PositionOver(RectTransform sel)
    {
        if (sel == null) return;

        Vector3[] corners = new Vector3[4];
        sel.GetWorldCorners(corners);
        Vector3 worldCenter = (corners[0] + corners[2]) * 0.5f;

        Vector2 localCenter = parentRect.InverseTransformPoint(worldCenter);
        float ps = Mathf.Abs(parentRect.lossyScale.x) > 1e-5f ? parentRect.lossyScale.x : 1f;
        float w = Vector3.Distance(corners[3], corners[0]) / ps;
        float h = Vector3.Distance(corners[1], corners[0]) / ps;
        Vector2 targetSize = new Vector2(w + padding.x, h + padding.y);

        if (snapNext)
        {
            rect.anchoredPosition = localCenter;
            rect.sizeDelta = targetSize;
            snapNext = false;
        }
        else
        {
            float t = 1f - Mathf.Exp(-moveSpeed * Time.unscaledDeltaTime);
            rect.anchoredPosition = Vector2.Lerp(rect.anchoredPosition, localCenter, t);
            rect.sizeDelta = Vector2.Lerp(rect.sizeDelta, targetSize, t);
        }
    }
}
