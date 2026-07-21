using UnityEngine;
using UnityEngine.EventSystems;

public class HoverRevealPanel : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, ISelectHandler, IDeselectHandler
{
    public CanvasGroup panelToReveal; // Layout's Canvas Group
    [SerializeField] private float closeDelay = 0.15f;
    [SerializeField] private float fadeDuration = 0.15f;

    private int activeFocusCount = 0;
    private Coroutine closeRoutine;
    private Coroutine fadeRoutine;

    private void Awake()
    {
        SetGroupState(0f, false, false);
    }

    public void OnPointerEnter(PointerEventData eventData) => Enter();
    public void OnSelect(BaseEventData eventData) => Enter();

    public void OnPointerExit(PointerEventData eventData) => Exit();
    public void OnDeselect(BaseEventData eventData) => Exit();

    private void Enter()
    {
        activeFocusCount++;
        if (closeRoutine != null)
        {
            StopCoroutine(closeRoutine);
            closeRoutine = null;
        }
        FadeTo(1f, true, true);
    }

    private void Exit()
    {
        activeFocusCount = Mathf.Max(0, activeFocusCount - 1);
        if (activeFocusCount == 0)
        {
            if (closeRoutine != null) StopCoroutine(closeRoutine);
            closeRoutine = StartCoroutine(CloseAfterDelay());
        }
    }

    private System.Collections.IEnumerator CloseAfterDelay()
    {
        yield return new WaitForSecondsRealtime(closeDelay);
        if (activeFocusCount == 0)
            FadeTo(0f, false, false);
    }

    private void FadeTo(float targetAlpha, bool interactable, bool blocksRaycasts)
    {
        if (fadeRoutine != null) StopCoroutine(fadeRoutine);
        fadeRoutine = StartCoroutine(FadeRoutine(targetAlpha, interactable, blocksRaycasts));
    }

    private System.Collections.IEnumerator FadeRoutine(float targetAlpha, bool interactable, bool blocksRaycasts)
    {
        float start = panelToReveal.alpha;
        float t = 0f;

        while (t < fadeDuration)
        {
            t += Time.unscaledDeltaTime;
            panelToReveal.alpha = Mathf.Lerp(start, targetAlpha, t / fadeDuration);
            yield return null;
        }

        SetGroupState(targetAlpha, interactable, blocksRaycasts);
    }

    private void SetGroupState(float alpha, bool interactable, bool blocksRaycasts)
    {
        panelToReveal.alpha = alpha;
        panelToReveal.interactable = interactable;
        panelToReveal.blocksRaycasts = blocksRaycasts;
    }
}