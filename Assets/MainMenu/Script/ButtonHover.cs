using UnityEngine;
using UnityEngine.EventSystems;

public class ButtonHover : MonoBehaviour,
    IPointerEnterHandler,
    IPointerExitHandler,
    ISelectHandler,
    IDeselectHandler
{
    public Animator glowAnimator;
    public Animator normalTextAnimator;
    public Animator glowTextAnimator;

    public void OnPointerEnter(PointerEventData eventData)
    {
        SetGlow(true);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        SetGlow(false);
    }

    public void OnSelect(BaseEventData eventData)
    {
        SetGlow(true);
    }

    public void OnDeselect(BaseEventData eventData)
    {
        SetGlow(false);
    }

    private void SetGlow(bool state)
    {
        glowAnimator.SetBool("Hover", state);
        normalTextAnimator.SetBool("Hover", state);
        glowTextAnimator.SetBool("Hover", state);
    }
}