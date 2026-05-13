using UnityEngine;

/// <summary>
/// Attach to R1_Lens and R2_Portal.
/// Draws a visible colored border and fill that stays visible
/// even when the insanity grayscale effect is active.
/// No sprite needed — uses LineRenderer and a generated texture.
/// </summary>
[RequireComponent(typeof(BoxCollider2D))]
public class BoxOutline : MonoBehaviour
{
    [Header("Outline Settings")]
    [Tooltip("Color of the border outline")]
    public Color outlineColor = Color.green;

    [Tooltip("Thickness of the border line")]
    public float lineWidth = 0.08f;

    [Header("Fill Settings")]
    [Tooltip("Show a semi-transparent fill inside the box")]
    public bool showFill = true;

    [Tooltip("Color of the inside fill — increase alpha if not visible (0-255)")]
    public Color fillColor = new Color(0f, 1f, 0.5f, 0.35f);

    [Header("Pulse Effect")]
    [Tooltip("Make the outline pulse/glow when visible")]
    public bool enablePulse = true;

    [Tooltip("Speed of the pulse animation")]
    public float pulseSpeed = 2f;

    [Tooltip("How much the alpha pulses (0 = no pulse, 0.3 = noticeable)")]
    public float pulseAmount = 0.3f;

    [Header("Label")]
    public string label = "R1";
    public bool showLabel = true;

    // -- Private state --------------------------------------------------------

    private LineRenderer lineRenderer;
    private SpriteRenderer fillRenderer;
    private BoxCollider2D boxCollider;
    private float baseOutlineAlpha;
    private float baseFillAlpha;

    // -- Unity lifecycle ------------------------------------------------------

    private void Awake()
    {
        boxCollider = GetComponent<BoxCollider2D>();
        baseOutlineAlpha = outlineColor.a;
        baseFillAlpha = fillColor.a;

        SetupOutline();

        if (showFill)
            SetupFill();
    }

    private void Update()
    {
        UpdateOutlinePoints();

        if (enablePulse)
            ApplyPulse();
    }

    // -- Outline setup --------------------------------------------------------

    private void SetupOutline()
    {
        GameObject outlineObj = new GameObject("BoxOutline_Line");
        outlineObj.transform.SetParent(transform);
        outlineObj.transform.localPosition = Vector3.zero;
        outlineObj.transform.localRotation = Quaternion.identity;
        outlineObj.transform.localScale = Vector3.one;

        lineRenderer = outlineObj.AddComponent<LineRenderer>();
        lineRenderer.loop = true;
        lineRenderer.positionCount = 4;
        lineRenderer.startWidth = lineWidth;
        lineRenderer.endWidth = lineWidth;
        lineRenderer.useWorldSpace = false;

        // Use Sprites/Default so it always renders with full color
        Material mat = new Material(Shader.Find("Sprites/Default"));
        mat.color = outlineColor;
        lineRenderer.material = mat;
        lineRenderer.startColor = outlineColor;
        lineRenderer.endColor = outlineColor;

        // High sorting order so it draws on top of everything
        lineRenderer.sortingLayerName = "Default";
        lineRenderer.sortingOrder = 100;

        UpdateOutlinePoints();
    }

    private void UpdateOutlinePoints()
    {
        if (lineRenderer == null || boxCollider == null) return;

        Vector2 size = boxCollider.size;
        Vector2 offset = boxCollider.offset;

        float halfW = size.x * 0.5f;
        float halfH = size.y * 0.5f;

        lineRenderer.SetPosition(0, new Vector3(offset.x - halfW, offset.y + halfH, 0));
        lineRenderer.SetPosition(1, new Vector3(offset.x + halfW, offset.y + halfH, 0));
        lineRenderer.SetPosition(2, new Vector3(offset.x + halfW, offset.y - halfH, 0));
        lineRenderer.SetPosition(3, new Vector3(offset.x - halfW, offset.y - halfH, 0));
    }

    // -- Fill setup -----------------------------------------------------------

    private void SetupFill()
    {
        GameObject fillObj = new GameObject("BoxOutline_Fill");
        fillObj.transform.SetParent(transform);
        fillObj.transform.localPosition = new Vector3(
            boxCollider.offset.x,
            boxCollider.offset.y,
            0.01f
        );
        fillObj.transform.localRotation = Quaternion.identity;

        fillRenderer = fillObj.AddComponent<SpriteRenderer>();
        fillRenderer.sprite = CreateWhiteSprite();
        fillRenderer.color = fillColor;
        fillRenderer.sortingOrder = 99;

        fillObj.transform.localScale = new Vector3(
            boxCollider.size.x,
            boxCollider.size.y,
            1f
        );
    }

    // -- Pulse effect ---------------------------------------------------------

    private void ApplyPulse()
    {
        float pulse = (Mathf.Sin(Time.time * pulseSpeed) + 1f) * 0.5f; // 0 to 1

        // Pulse outline alpha
        Color outlineCol = outlineColor;
        outlineCol.a = Mathf.Clamp01(baseOutlineAlpha - pulseAmount + pulse * pulseAmount * 2f);

        if (lineRenderer != null)
        {
            lineRenderer.startColor = outlineCol;
            lineRenderer.endColor = outlineCol;
        }

        // Pulse fill alpha
        if (fillRenderer != null)
        {
            Color fillCol = fillColor;
            fillCol.a = Mathf.Clamp01(baseFillAlpha - (pulseAmount * 0.5f) + pulse * pulseAmount);
            fillRenderer.color = fillCol;
        }
    }

    // -- White sprite generator -----------------------------------------------

    private Sprite CreateWhiteSprite()
    {
        Texture2D tex = new Texture2D(1, 1);
        tex.SetPixel(0, 0, Color.white);
        tex.Apply();
        return Sprite.Create(tex, new Rect(0, 0, 1, 1), new Vector2(0.5f, 0.5f));
    }

    // -- Public API -----------------------------------------------------------

    public void SetOutlineColor(Color color)
    {
        outlineColor = color;
        baseOutlineAlpha = color.a;
        if (lineRenderer != null)
        {
            lineRenderer.startColor = color;
            lineRenderer.endColor = color;
        }
    }

    // -- Editor gizmo ---------------------------------------------------------

    private void OnDrawGizmos()
    {
        if (!showLabel) return;
#if UNITY_EDITOR
        UnityEditor.Handles.color = outlineColor;
        UnityEditor.Handles.Label(transform.position, label);
#endif
    }
}