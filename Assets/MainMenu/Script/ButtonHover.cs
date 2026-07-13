using UnityEngine;
using UnityEngine.EventSystems;

public class ButtonHover : MonoBehaviour,
    IPointerEnterHandler,
    IPointerExitHandler
{
    public Animator glowAnimator;
    public Animator normalTextAnimator;
    public Animator glowTextAnimator;

    public void OnPointerEnter(PointerEventData eventData)
    {
        glowAnimator.SetBool("Hover", true);
        normalTextAnimator.SetBool("Hover", true);
        glowTextAnimator.SetBool("Hover", true);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        glowAnimator.SetBool("Hover", false);
        normalTextAnimator.SetBool("Hover", false);
        glowTextAnimator.SetBool("Hover", false);
    }
}