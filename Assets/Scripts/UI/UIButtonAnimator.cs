using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.EventSystems;

[RequireComponent(typeof(Button))]
public class UIButtonAnimator : MonoBehaviour,
    IPointerEnterHandler,
    IPointerExitHandler,
    IPointerDownHandler,
    IPointerUpHandler
{
    private Outline outline;
    private TextMeshProUGUI buttonText;



    public enum OutlineMode
    {
        Visible,
        FadeIn,
        None
    }

    [Header("Outline Settings")]
    [SerializeField] private OutlineMode outlineMode;



    [Header("Animation")]
    [SerializeField] private float transitionSpeed = 12f;



    [Header("Text Colors")]
    [SerializeField] private Color normalText = Color.white;
    [SerializeField] private Color hoverText = new Color(.85f, .85f, .85f);
    [SerializeField] private Color pressedText = new Color(.65f, .65f, .65f);



    [Header("Outline Colors")]
    [SerializeField] private Color normalOutline = Color.white;
    [SerializeField] private Color hoverOutline = new Color(.85f, .85f, .85f);
    [SerializeField] private Color pressedOutline = new Color(.65f, .65f, .65f);



    private Color targetTextColor;
    private Color targetOutlineColor;



    private void Awake()
    {
        outline = GetComponent<Outline>();

        buttonText = GetComponentInChildren<TextMeshProUGUI>();

        targetTextColor = normalText;

        SetupInitialOutline();

        ApplyInitialState();
    }



    private void Update()
    {
        buttonText.color = Color.Lerp(
            buttonText.color,
            targetTextColor,
            transitionSpeed * Time.unscaledDeltaTime
        );

        if (outline != null && outlineMode != OutlineMode.None)
        {
            outline.effectColor = Color.Lerp(
                outline.effectColor,
                targetOutlineColor,
                transitionSpeed * Time.unscaledDeltaTime
            );
        }
    }



    private void SetupInitialOutline()
    {
        if (outline == null)
            return;

        switch (outlineMode)
        {
            case OutlineMode.Visible:

                targetOutlineColor = normalOutline;

                break;


            case OutlineMode.FadeIn:

                Color hiddenColor = normalOutline;
                hiddenColor.a = 0f;

                outline.effectColor = hiddenColor;

                targetOutlineColor = hiddenColor;

                break;


            case OutlineMode.None:

                outline.enabled = false;

                break;
        }
    }



    public void OnPointerEnter(PointerEventData eventData)
    {
        targetTextColor = hoverText;

        SetOutlineState(hoverOutline);
    }



    public void OnPointerExit(PointerEventData eventData)
    {
        targetTextColor = normalText;

        SetOutlineState(normalOutline, true);
    }



    public void OnPointerDown(PointerEventData eventData)
    {
        targetTextColor = pressedText;

        SetOutlineState(pressedOutline);
    }



    public void OnPointerUp(PointerEventData eventData)
    {
        targetTextColor = hoverText;

        SetOutlineState(hoverOutline);
    }



    private void SetOutlineState(
        Color color,
        bool hideIfFadeMode = false)
    {
        if (outline == null)
            return;

        if (outlineMode == OutlineMode.None)
            return;

        if (outlineMode == OutlineMode.FadeIn &&
            hideIfFadeMode)
        {
            color.a = 0f;
        }

        targetOutlineColor = color;
    }



    private void ApplyInitialState()
    {
        buttonText.color = normalText;
    }
}