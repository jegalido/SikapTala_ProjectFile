using UnityEngine;
using TMPro;
using System.Collections;

public class WorldSpaceSign : MonoBehaviour
{
    [Header("Message")]
    [TextArea(2, 5)]
    public string message = "Press SHIFT to reveal the hidden world!";

    public string inputButton = "SHIFT";
    public string buttonDescription = "to reveal hidden world";

    [Tooltip("Show or hide the [ BUTTON ] hint below the message")]
    public bool showButtonHint = true;

    [Header("Visibility")]
    [Tooltip("If TRUE, sign only appears when the player walks into the trigger zone.\n" +
             "If FALSE, the sign is always visible from the start.")]
    public bool requireTrigger = true;

    [Header("TMP")]
    public TMP_FontAsset fontAsset;

    [Header("Size")]
    public Vector2 signSize = new Vector2(4f, 1.5f);

    [Tooltip("How high above the object")]
    public Vector2 signOffset = new Vector2(0f, 2f);

    [Header("Sorting")]
    [Tooltip("Must match the Sorting Layer your tilemap/map uses (e.g. 'Default', 'Foreground'). " +
             "All sign renderers will be placed on this layer so they appear above the map.")]
    public string sortingLayerName = "Default";

    [Tooltip("Order in layer for the sign text. Background and borders sit just below this.")]
    public int sortingOrder = 100;

    [Header("Colors")]
    public Color backgroundColor = new Color(0.1f, 0.1f, 0.1f, 0.95f);
    public Color borderColor = Color.yellow;
    public Color textColor = Color.white;
    public Color buttonColor = Color.cyan;

    [Header("Font")]
    public float messageFontSize = 10f;
    public float buttonFontSize = 8f;

    [Header("Animation")]
    public float bounceHeight = 0.05f;
    public float bounceSpeed = 3f;
    public float popSpeed = 8f;

    // -------------------------------------------------------

    private GameObject signRoot;

    private GameObject messageObj;
    private GameObject buttonObj;

    private TextMeshPro messageTMP;
    private TextMeshPro buttonTMP;

    private Vector3 messageBasePos;
    private Vector3 buttonBasePos;

    private bool visible;
    private float bounceTimer;

    // -------------------------------------------------------

    private void Start()
    {
        Collider2D col = GetComponent<Collider2D>();

        if (col == null)
        {
            BoxCollider2D box = gameObject.AddComponent<BoxCollider2D>();
            box.isTrigger = true;
        }
        else
        {
            col.isTrigger = true;
        }

        BuildSign();

        // If requireTrigger is OFF, show immediately with no pop animation
        // If requireTrigger is ON, stay hidden until player enters
        if (!requireTrigger)
        {
            visible = true;
            SetVisible(true);
            signRoot.transform.localScale = Vector3.one;
        }
        else
        {
            SetVisible(false);
        }
    }

    // -------------------------------------------------------

    private void Update()
    {
        if (!visible) return;

        bounceTimer += Time.deltaTime * bounceSpeed;

        float bounce = Mathf.Abs(Mathf.Sin(bounceTimer)) * bounceHeight;

        if (messageObj != null)
            messageObj.transform.localPosition = messageBasePos + new Vector3(0f, bounce, 0f);

        if (buttonObj != null)
            buttonObj.transform.localPosition = buttonBasePos + new Vector3(0f, bounce * 0.7f, 0f);
    }

    // -------------------------------------------------------
    // TRIGGERS
    // -------------------------------------------------------

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;

        // If always visible, trigger enter does nothing
        if (!requireTrigger) return;

        Show();
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;

        // If always visible, trigger exit does nothing
        if (!requireTrigger) return;

        Hide();
    }

    // -------------------------------------------------------
    // SHOW / HIDE
    // -------------------------------------------------------

    private void Show()
    {
        visible = true;
        SetVisible(true);
        StopAllCoroutines();
        StartCoroutine(PopIn());
    }

    private void Hide()
    {
        visible = false;
        SetVisible(false);
    }

    private void SetVisible(bool state)
    {
        if (signRoot != null)
            signRoot.SetActive(state);
    }

    // -------------------------------------------------------
    // POP ANIMATION
    // -------------------------------------------------------

    private IEnumerator PopIn()
    {
        signRoot.transform.localScale = Vector3.zero;
        float t = 0f;

        while (t < 1f)
        {
            t += Time.deltaTime * popSpeed;
            signRoot.transform.localScale = Vector3.one * EaseOutBack(t);
            yield return null;
        }

        signRoot.transform.localScale = Vector3.one;
    }

    private float EaseOutBack(float t)
    {
        float c1 = 1.70158f;
        float c3 = c1 + 1f;
        return 1f + c3 * Mathf.Pow(t - 1f, 3f) + c1 * Mathf.Pow(t - 1f, 2f);
    }

    // -------------------------------------------------------
    // BUILD SIGN
    // -------------------------------------------------------

    private void BuildSign()
    {
        // ROOT
        signRoot = new GameObject("SignRoot");
        signRoot.transform.SetParent(transform);
        signRoot.transform.localPosition = new Vector3(signOffset.x, signOffset.y, 0f);
        signRoot.transform.localRotation = Quaternion.identity;
        signRoot.transform.localScale = Vector3.one;

        // BACKGROUND
        GameObject bg = new GameObject("Background");
        bg.transform.SetParent(signRoot.transform);
        bg.transform.localPosition = Vector3.zero;
        bg.transform.localScale = new Vector3(signSize.x, signSize.y, 1f);

        SpriteRenderer bgSR = bg.AddComponent<SpriteRenderer>();
        bgSR.sprite = CreateWhiteSprite();
        bgSR.color = backgroundColor;
        ApplySorting(bgSR, sortingOrder - 2);

        // BORDERS
        CreateBorder("Top", new Vector3(0f, signSize.y * 0.5f, 0f), new Vector3(signSize.x, 0.05f, 1f));
        CreateBorder("Bottom", new Vector3(0f, -signSize.y * 0.5f, 0f), new Vector3(signSize.x, 0.05f, 1f));
        CreateBorder("Left", new Vector3(-signSize.x * 0.5f, 0f, 0f), new Vector3(0.05f, signSize.y, 1f));
        CreateBorder("Right", new Vector3(signSize.x * 0.5f, 0f, 0f), new Vector3(0.05f, signSize.y, 1f));

        // MESSAGE
        messageObj = new GameObject("Message");
        messageObj.transform.SetParent(signRoot.transform);
        messageBasePos = showButtonHint ? new Vector3(0f, 0.25f, 0f) : new Vector3(0f, 0f, 0f);
        messageObj.transform.localPosition = messageBasePos;
        messageObj.transform.localScale = Vector3.one;

        messageTMP = messageObj.AddComponent<TextMeshPro>();
        if (fontAsset != null) messageTMP.font = fontAsset;
        messageTMP.text = message;
        messageTMP.fontSize = messageFontSize;
        messageTMP.color = textColor;
        messageTMP.alignment = TextAlignmentOptions.Center;
        messageTMP.enableWordWrapping = true;
        messageTMP.rectTransform.sizeDelta = new Vector2(signSize.x * 4f, signSize.y * 4f);
        ApplySorting(messageTMP.GetComponent<MeshRenderer>(), sortingOrder);

        // BUTTON HINT
        if (showButtonHint)
        {
            buttonObj = new GameObject("Button");
            buttonObj.transform.SetParent(signRoot.transform);
            buttonBasePos = new Vector3(0f, -0.3f, 0f);
            buttonObj.transform.localPosition = buttonBasePos;
            buttonObj.transform.localScale = Vector3.one;

            buttonTMP = buttonObj.AddComponent<TextMeshPro>();
            if (fontAsset != null) buttonTMP.font = fontAsset;
            buttonTMP.text = BuildButtonText();
            buttonTMP.fontSize = buttonFontSize;
            buttonTMP.color = textColor;
            buttonTMP.alignment = TextAlignmentOptions.Center;
            buttonTMP.rectTransform.sizeDelta = new Vector2(signSize.x * 4f, signSize.y * 4f);
            ApplySorting(buttonTMP.GetComponent<MeshRenderer>(), sortingOrder);
        }
    }

    private void ApplySorting(SpriteRenderer sr, int order)
    {
        sr.sortingLayerName = sortingLayerName;
        sr.sortingOrder = order;
    }

    private void ApplySorting(MeshRenderer mr, int order)
    {
        mr.sortingLayerName = sortingLayerName;
        mr.sortingOrder = order;
    }

    private void CreateBorder(string borderName, Vector3 localPos, Vector3 scale)
    {
        GameObject border = new GameObject(borderName);
        border.transform.SetParent(signRoot.transform);
        border.transform.localPosition = localPos;
        border.transform.localScale = scale;

        SpriteRenderer sr = border.AddComponent<SpriteRenderer>();
        sr.sprite = CreateWhiteSprite();
        sr.color = borderColor;
        ApplySorting(sr, sortingOrder - 1);
    }

    private Sprite CreateWhiteSprite()
    {
        Texture2D tex = new Texture2D(1, 1);
        tex.SetPixel(0, 0, Color.white);
        tex.Apply();
        return Sprite.Create(tex, new Rect(0, 0, 1, 1), new Vector2(0.5f, 0.5f));
    }

    private string BuildButtonText()
    {
        string hex = ColorUtility.ToHtmlStringRGB(buttonColor);
        return $"<color=#{hex}>[ {inputButton} ]</color> {buttonDescription}";
    }

    public void SetMessage(string newMessage)
    {
        message = newMessage;
        if (messageTMP != null) messageTMP.text = newMessage;
    }

    public void SetButtonHint(string button, string description)
    {
        inputButton = button;
        buttonDescription = description;
        if (buttonTMP != null) buttonTMP.text = BuildButtonText();
    }
}