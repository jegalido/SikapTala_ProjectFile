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

    [Header("TMP")]
    public TMP_FontAsset fontAsset;

    [Header("Size")]
    public Vector2 signSize = new Vector2(4f, 1.5f);

    [Tooltip("How high above the object")]
    public Vector2 signOffset = new Vector2(0f, 2f);

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

        // DEBUG:
        // change to false later
        SetVisible(true);
    }

    // -------------------------------------------------------

    private void Update()
    {
        if (!visible) return;

        bounceTimer += Time.deltaTime * bounceSpeed;

        float bounce =
            Mathf.Abs(Mathf.Sin(bounceTimer)) * bounceHeight;

        if (messageObj != null)
        {
            messageObj.transform.localPosition =
                messageBasePos + new Vector3(0f, bounce, 0f);
        }

        if (buttonObj != null)
        {
            buttonObj.transform.localPosition =
                buttonBasePos + new Vector3(0f, bounce * 0.7f, 0f);
        }
    }

    // -------------------------------------------------------
    // TRIGGERS
    // -------------------------------------------------------

    private void OnTriggerEnter2D(Collider2D other)
    {
        Debug.Log("ENTER: " + other.name);

        if (!other.CompareTag("Player"))
            return;

        Show();
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        Debug.Log("EXIT: " + other.name);

        if (!other.CompareTag("Player"))
            return;

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
        {
            signRoot.SetActive(state);
        }
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

            float scale = EaseOutBack(t);

            signRoot.transform.localScale =
                Vector3.one * scale;

            yield return null;
        }

        signRoot.transform.localScale = Vector3.one;
    }

    private float EaseOutBack(float t)
    {
        float c1 = 1.70158f;
        float c3 = c1 + 1f;

        return 1f +
               c3 * Mathf.Pow(t - 1f, 3f) +
               c1 * Mathf.Pow(t - 1f, 2f);
    }

    // -------------------------------------------------------
    // BUILD SIGN
    // -------------------------------------------------------

    private void BuildSign()
    {
        // ROOT
        signRoot = new GameObject("SignRoot");

        signRoot.transform.SetParent(transform);

        signRoot.transform.localPosition =
            new Vector3(signOffset.x, signOffset.y, 0f);

        signRoot.transform.localRotation =
            Quaternion.identity;

        signRoot.transform.localScale =
            Vector3.one;

        // ---------------------------------------------------
        // BACKGROUND
        // ---------------------------------------------------

        GameObject bg =
            new GameObject("Background");

        bg.transform.SetParent(signRoot.transform);

        bg.transform.localPosition =
            new Vector3(0f, 0f, 1f);

        bg.transform.localScale =
            new Vector3(signSize.x, signSize.y, 1f);

        SpriteRenderer bgSR =
            bg.AddComponent<SpriteRenderer>();

        bgSR.sprite = CreateWhiteSprite();
        bgSR.color = backgroundColor;
        bgSR.sortingOrder = 1;

        // ---------------------------------------------------
        // BORDERS
        // ---------------------------------------------------

        CreateBorder(
            "Top",
            new Vector3(0f, signSize.y * 0.5f, 0f),
            new Vector3(signSize.x, 0.05f, 1f)
        );

        CreateBorder(
            "Bottom",
            new Vector3(0f, -signSize.y * 0.5f, 0f),
            new Vector3(signSize.x, 0.05f, 1f)
        );

        CreateBorder(
            "Left",
            new Vector3(-signSize.x * 0.5f, 0f, 0f),
            new Vector3(0.05f, signSize.y, 1f)
        );

        CreateBorder(
            "Right",
            new Vector3(signSize.x * 0.5f, 0f, 0f),
            new Vector3(0.05f, signSize.y, 1f)
        );

        // ---------------------------------------------------
        // MESSAGE
        // ---------------------------------------------------

        messageObj =
            new GameObject("Message");

        messageObj.transform.SetParent(signRoot.transform);

        messageBasePos =
            new Vector3(0f, 0.25f, -1f);

        messageObj.transform.localPosition =
            messageBasePos;

        messageObj.transform.localScale =
            Vector3.one;

        messageTMP =
            messageObj.AddComponent<TextMeshPro>();

        if (fontAsset != null)
        {
            messageTMP.font = fontAsset;
        }

        messageTMP.text = message;
        messageTMP.fontSize = messageFontSize;
        messageTMP.color = textColor;
        messageTMP.alignment =
            TextAlignmentOptions.Center;

        messageTMP.enableWordWrapping = true;

        messageTMP.rectTransform.sizeDelta =
            new Vector2(20f, 5f);

        MeshRenderer msgRenderer =
            messageTMP.GetComponent<MeshRenderer>();

        msgRenderer.sortingOrder = 10;

        // ---------------------------------------------------
        // BUTTON
        // ---------------------------------------------------

        buttonObj =
            new GameObject("Button");

        buttonObj.transform.SetParent(signRoot.transform);

        buttonBasePos =
            new Vector3(0f, -0.3f, -1f);

        buttonObj.transform.localPosition =
            buttonBasePos;

        buttonObj.transform.localScale =
            Vector3.one;

        buttonTMP =
            buttonObj.AddComponent<TextMeshPro>();

        if (fontAsset != null)
        {
            buttonTMP.font = fontAsset;
        }

        string hex =
            ColorUtility.ToHtmlStringRGB(buttonColor);

        buttonTMP.text =
            $"<color=#{hex}>[ {inputButton} ]</color> {buttonDescription}";

        buttonTMP.fontSize = buttonFontSize;
        buttonTMP.color = textColor;
        buttonTMP.alignment =
            TextAlignmentOptions.Center;

        buttonTMP.rectTransform.sizeDelta =
            new Vector2(20f, 5f);

        MeshRenderer btnRenderer =
            buttonTMP.GetComponent<MeshRenderer>();

        btnRenderer.sortingOrder = 10;
    }

    // -------------------------------------------------------
    // CREATE BORDER
    // -------------------------------------------------------

    private void CreateBorder(
        string borderName,
        Vector3 localPos,
        Vector3 scale)
    {
        GameObject border =
            new GameObject(borderName);

        border.transform.SetParent(signRoot.transform);

        border.transform.localPosition = localPos;
        border.transform.localScale = scale;

        SpriteRenderer sr =
            border.AddComponent<SpriteRenderer>();

        sr.sprite = CreateWhiteSprite();
        sr.color = borderColor;
        sr.sortingOrder = 2;
    }

    // -------------------------------------------------------
    // WHITE SPRITE
    // -------------------------------------------------------

    private Sprite CreateWhiteSprite()
    {
        Texture2D tex =
            new Texture2D(1, 1);

        tex.SetPixel(0, 0, Color.white);

        tex.Apply();

        return Sprite.Create(
            tex,
            new Rect(0, 0, 1, 1),
            new Vector2(0.5f, 0.5f)
        );
    }

    // -------------------------------------------------------
    // PUBLIC API
    // -------------------------------------------------------

    public void SetMessage(string newMessage)
    {
        message = newMessage;

        if (messageTMP != null)
        {
            messageTMP.text = newMessage;
        }
    }

    public void SetButtonHint(
        string button,
        string description)
    {
        inputButton = button;
        buttonDescription = description;

        if (buttonTMP != null)
        {
            string hex =
                ColorUtility.ToHtmlStringRGB(buttonColor);

            buttonTMP.text =
                $"<color=#{hex}>[ {button} ]</color> {description}";
        }
    }
}