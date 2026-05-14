using UnityEngine;

public class DirectionalArrow : MonoBehaviour
{
   
    [Tooltip("Which way the arrow points")]
    public enum ArrowDirection { Up, Down, Left, Right }
    public ArrowDirection direction = ArrowDirection.Up;

    [Header("Appearance")]
    public Color arrowColor = Color.white;
    public float arrowSize = 0.5f;

    [Header("Sorting")]
    public string sortingLayerName = "Default";
    public int sortingOrder = 200;

    [Header("Bob Animation")]
    public float bobDistance = 0.3f;
    public float bobSpeed = 2.5f;

    [Header("Pulse Animation")]
    public bool usePulse = true;
    public float pulseMin = 0.7f;
    public float pulseMax = 1.0f;
    public float pulseSpeed = 2.5f;

    [Header("Multiple Arrows")]
    [Tooltip("How many arrows to stack in sequence")]
    public int arrowCount = 3;
    [Tooltip("Gap between each arrow")]
    public float arrowSpacing = 0.45f;
    [Tooltip("Delay between each arrow's pulse (creates wave effect)")]
    public float cascadeDelay = 0.2f;

    // -------------------------------------------------------

    private GameObject[] arrowObjs;
    private SpriteRenderer[] arrowRenderers;
    private Vector3[] basePositions;
    private float bobTimer;

    // -------------------------------------------------------

    private void Start()
    {
        BuildArrows();
    }

    private void Update()
    {
        bobTimer += Time.deltaTime * bobSpeed;

        float bob = Mathf.Sin(bobTimer) * bobDistance;
        Vector3 bobDir = GetBobDirection();

        for (int i = 0; i < arrowCount; i++)
        {
            if (arrowObjs[i] == null) continue;

            // Bob — all arrows move together
            arrowObjs[i].transform.localPosition = basePositions[i] + bobDir * bob;

            // Pulse — cascades across arrows for wave effect
            if (usePulse && arrowRenderers[i] != null)
            {
                float offset = i * cascadeDelay;
                float pulse = Mathf.Lerp(pulseMin, pulseMax,
                    (Mathf.Sin(bobTimer * pulseSpeed - offset) + 1f) * 0.5f);

                Color c = arrowColor;
                c.a = pulse;
                arrowRenderers[i].color = c;
            }
        }
    }

    // -------------------------------------------------------
    // BUILD
    // -------------------------------------------------------

    private void BuildArrows()
    {
        arrowObjs = new GameObject[arrowCount];
        arrowRenderers = new SpriteRenderer[arrowCount];
        basePositions = new Vector3[arrowCount];

        Vector3 stepDir = GetStepDirection();
        Quaternion rot = GetRotation();
        Sprite arrowSprite = CreateArrowSprite();

        for (int i = 0; i < arrowCount; i++)
        {
            GameObject obj = new GameObject($"Arrow_{i}");
            obj.transform.SetParent(transform);

            // Stack arrows along pointing direction
            Vector3 basePos = stepDir * (arrowSpacing * i);
            obj.transform.localPosition = basePos;
            obj.transform.localRotation = rot;
            obj.transform.localScale = Vector3.one * arrowSize;

            SpriteRenderer sr = obj.AddComponent<SpriteRenderer>();
            sr.sprite = arrowSprite;
            sr.color = arrowColor;
            sr.sortingLayerName = sortingLayerName;
            sr.sortingOrder = sortingOrder + i;

            arrowObjs[i] = obj;
            arrowRenderers[i] = sr;
            basePositions[i] = basePos;
        }
    }

    // -------------------------------------------------------
    // ARROW SPRITE (procedural)
    // -------------------------------------------------------

    private Sprite CreateArrowSprite()
    {
        // Texture canvas
        int w = 32, h = 32;
        Texture2D tex = new Texture2D(w, h, TextureFormat.RGBA32, false);
        tex.filterMode = FilterMode.Point;

        // Clear to transparent
        Color clear = Color.clear;
        for (int x = 0; x < w; x++)
            for (int y = 0; y < h; y++)
                tex.SetPixel(x, y, clear);

        Color fill = Color.white;

        // Arrow head (triangle pointing UP, sprite is always up — rotation handles direction)
        //
        //        *          row 31
        //       ***         row 30
        //      *****        row 29
        //     *******       row 28
        //    *********      row 27
        //   ***********     row 26
        //      *****        rows 0-25 (shaft)
        //      *****
        //      *****

        int shaftW = 6;
        int shaftLeft = w / 2 - shaftW / 2;
        int shaftRight = shaftLeft + shaftW;
        int headBase = 10; // y where head starts
        int headTip = 30; // y where head ends (tip)

        // Draw shaft
        for (int y = 0; y < headBase; y++)
            for (int x = shaftLeft; x < shaftRight; x++)
                tex.SetPixel(x, y, fill);

        // Draw triangular head
        for (int y = headBase; y <= headTip; y++)
        {
            float progress = (float)(y - headBase) / (headTip - headBase); // 0 at base, 1 at tip
            int halfWidth = Mathf.RoundToInt((1f - progress) * (w / 2f - 1f));
            int left = w / 2 - halfWidth;
            int right = w / 2 + halfWidth;
            for (int x = left; x <= right; x++)
                tex.SetPixel(x, y, fill);
        }

        tex.Apply();

        return Sprite.Create(tex,
            new Rect(0, 0, w, h),
            new Vector2(0.5f, 0.5f),
            w); // pixels per unit = texture width so 1 unit = 1 sprite
    }

    // -------------------------------------------------------
    // HELPERS
    // -------------------------------------------------------

    private Vector3 GetStepDirection()
    {
        switch (direction)
        {
            case ArrowDirection.Up: return Vector3.up;
            case ArrowDirection.Down: return Vector3.down;
            case ArrowDirection.Left: return Vector3.left;
            case ArrowDirection.Right: return Vector3.right;
            default: return Vector3.up;
        }
    }

    private Vector3 GetBobDirection()
    {
        switch (direction)
        {
            case ArrowDirection.Up: return Vector3.up;
            case ArrowDirection.Down: return Vector3.down;
            case ArrowDirection.Left: return Vector3.left;
            case ArrowDirection.Right: return Vector3.right;
            default: return Vector3.up;
        }
    }

    private Quaternion GetRotation()
    {
        switch (direction)
        {
            case ArrowDirection.Up: return Quaternion.Euler(0f, 0f, 0f);
            case ArrowDirection.Down: return Quaternion.Euler(0f, 0f, 180f);
            case ArrowDirection.Left: return Quaternion.Euler(0f, 0f, 90f);
            case ArrowDirection.Right: return Quaternion.Euler(0f, 0f, -90f);
            default: return Quaternion.identity;
        }
    }
}