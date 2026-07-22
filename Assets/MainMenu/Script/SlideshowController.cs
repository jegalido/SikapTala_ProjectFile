using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

public class SlideshowController : MonoBehaviour
{
    [Header("Slides")]
    [SerializeField] private RectTransform[] slides;
    [SerializeField] private CanvasGroup[] slideCanvasGroups;

    [Header("Layout")]
    [SerializeField] private float slideSpacing = 800f;
    [SerializeField] private float moveDuration = 0.35f;

    [Header("Fade")]
    [SerializeField] private float centeredAlpha = 1f;
    [SerializeField] private float sideAlpha = 0.3f;

    [Header("Scale")]
    [SerializeField] private float centeredScale = 1.1f;
    [SerializeField] private float sideScale = 0.9f;

    [Header("Vertical Offset")]
    [SerializeField] private float centeredYOffset = 30f; // how much higher the centered slide sits
    [SerializeField] private float sideYOffset = 0f;

    [SerializeField] private AnimationCurve fadeCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);

    [Header("Navigation Buttons (optional)")]
    [SerializeField] private GameObject nextButton;
    [SerializeField] private GameObject prevButton;

    private int currentIndex = 0;
    private Coroutine moveRoutine;
    private float[] baseYPositions; // each slide's original Y, before offset is applied

    private void OnEnable()
    {
        currentIndex = 0;

        baseYPositions = new float[slides.Length];
        for (int i = 0; i < slides.Length; i++)
            baseYPositions[i] = slides[i].anchoredPosition.y;

        SnapToIndex(currentIndex);
    }

    private void Update()
    {
        Gamepad pad = Gamepad.current;
        if (pad == null) return;

        if (pad.dpad.right.wasPressedThisFrame || pad.rightShoulder.wasPressedThisFrame)
            NextSlide();

        if (pad.dpad.left.wasPressedThisFrame || pad.leftShoulder.wasPressedThisFrame)
            PreviousSlide();
    }

    public void NextSlide()
    {
        if (currentIndex >= slides.Length - 1) return;
        currentIndex++;
        MoveToIndex(currentIndex);
    }

    public void PreviousSlide()
    {
        if (currentIndex <= 0) return;
        currentIndex--;
        MoveToIndex(currentIndex);
    }

    private void SnapToIndex(int index)
    {
        for (int i = 0; i < slides.Length; i++)
        {
            float xOffset = (i - index) * -slideSpacing;
            slides[i].anchoredPosition = new Vector2(xOffset, baseYPositions[i]);
        }
        UpdateVisuals(index);
        UpdateButtonVisibility();
    }

    private void MoveToIndex(int index)
    {
        if (moveRoutine != null) StopCoroutine(moveRoutine);
        moveRoutine = StartCoroutine(MoveRoutine(index));
        UpdateButtonVisibility();
    }

    private System.Collections.IEnumerator MoveRoutine(int index)
    {
        Vector2[] startPositions = new Vector2[slides.Length];
        Vector2[] targetPositions = new Vector2[slides.Length];

        for (int i = 0; i < slides.Length; i++)
        {
            startPositions[i] = slides[i].anchoredPosition;
            float xOffset = (i - index) * -slideSpacing;
            targetPositions[i] = new Vector2(xOffset, baseYPositions[i]);
        }

        float t = 0f;
        while (t < moveDuration)
        {
            t += Time.unscaledDeltaTime;
            float normalized = fadeCurve.Evaluate(t / moveDuration);

            for (int i = 0; i < slides.Length; i++)
            {
                Vector2 pos = Vector2.Lerp(startPositions[i], targetPositions[i], normalized);
                slides[i].anchoredPosition = pos;
            }

            UpdateVisuals(index);
            yield return null;
        }

        for (int i = 0; i < slides.Length; i++)
            slides[i].anchoredPosition = targetPositions[i];

        UpdateVisuals(index);
    }

    private void UpdateVisuals(int centerIndex)
    {
        for (int i = 0; i < slides.Length; i++)
        {
            float distance = Mathf.Abs(slides[i].anchoredPosition.x) / slideSpacing;
            float normalized = Mathf.Clamp01(distance); // 0 = centered, 1 = fully to the side

            // Alpha
            slideCanvasGroups[i].alpha = Mathf.Lerp(centeredAlpha, sideAlpha, normalized);

            // Scale
            float scale = Mathf.Lerp(centeredScale, sideScale, normalized);
            slides[i].localScale = new Vector3(scale, scale, 1f);

            // Vertical offset (added on top of base Y)
            float yOffset = Mathf.Lerp(centeredYOffset, sideYOffset, normalized);
            Vector2 pos = slides[i].anchoredPosition;
            slides[i].anchoredPosition = new Vector2(pos.x, baseYPositions[i] + yOffset);
        }
    }

    private void UpdateButtonVisibility()
    {
        if (prevButton != null) prevButton.SetActive(currentIndex > 0);
        if (nextButton != null) nextButton.SetActive(currentIndex < slides.Length - 1);
    }
}