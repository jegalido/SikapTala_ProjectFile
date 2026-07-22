using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using UnityEngine.InputSystem;

public class MenuManager : MonoBehaviour
{
    [SerializeField] private GameObject mainMenuPanel;
    [SerializeField] private GameObject settingsPanel;
    [SerializeField] private GameObject creditsPanel;
    [SerializeField] private GameObject howToPlayPanel;

    [Header("Panel Visibility")]
    [SerializeField] private CanvasGroup mainMenuCanvasGroup;

    [Header("Gamepad First Selected")]
    [SerializeField] private GameObject mainMenuFirstSelected;
    [SerializeField] private GameObject settingsFirstSelected;
    [SerializeField] private GameObject creditsFirstSelected;
    [SerializeField] private GameObject howToPlayFirstSelected;

    [Header("Settings")]
    [SerializeField] private Toggle fullscreenToggle;

    [Header("Blur")]
    [SerializeField] private Material screenBlurMaterial; // same M_ScreenBlur asset
    [SerializeField] private float maxBlurSize = 3f;
    [SerializeField] private float blurFadeDuration = 0.25f;
    private static readonly int BlurSizeID = Shader.PropertyToID("_BlurSize");
    private Coroutine blurRoutine;

    [Header("Intro Animation")]
    [SerializeField] private Animator mainMenuAnimator; // MainMenu_Panel's Animator
    private static readonly int PlayIntroID = Animator.StringToHash("PlayIntro");

    private void Start()
    {
        if (screenBlurMaterial != null)
            screenBlurMaterial.SetFloat(BlurSizeID, 0f);

        if (fullscreenToggle != null)
        {
            fullscreenToggle.SetIsOnWithoutNotify(Screen.fullScreen);
            fullscreenToggle.onValueChanged.AddListener(SetFullscreen);
        }

        if (mainMenuAnimator != null)
            mainMenuAnimator.SetTrigger(PlayIntroID);

        EventSystem.current.SetSelectedGameObject(mainMenuFirstSelected);
    }

    private void Update()
    {
        Gamepad pad = Gamepad.current;
        if (pad == null) return;
        // Circle = back, only if a sub-panel is open
        if (pad.buttonEast.wasPressedThisFrame)
        {
            if (settingsPanel.activeSelf) OnBackClicked(settingsPanel);
            else if (creditsPanel.activeSelf) OnBackClicked(creditsPanel);
            else if (howToPlayPanel.activeSelf) OnBackClicked(howToPlayPanel);
        }
    }

    private void ShowMainMenu()
    {
        mainMenuCanvasGroup.alpha = 1f;
        mainMenuCanvasGroup.interactable = true;
        mainMenuCanvasGroup.blocksRaycasts = true;
    }

    private void HideMainMenu()
    {
        mainMenuCanvasGroup.alpha = 0f;
        mainMenuCanvasGroup.interactable = false;
        mainMenuCanvasGroup.blocksRaycasts = false;
    }

    private void FadeBlur(float target)
    {
        if (screenBlurMaterial == null) return;
        if (blurRoutine != null)
            StopCoroutine(blurRoutine);
        blurRoutine = StartCoroutine(FadeBlurRoutine(target));
    }

    private System.Collections.IEnumerator FadeBlurRoutine(float target)
    {
        float start = screenBlurMaterial.GetFloat(BlurSizeID);
        float t = 0f;
        while (t < blurFadeDuration)
        {
            t += Time.unscaledDeltaTime;
            float value = Mathf.Lerp(start, target, t / blurFadeDuration);
            screenBlurMaterial.SetFloat(BlurSizeID, value);
            yield return null;
        }
        screenBlurMaterial.SetFloat(BlurSizeID, target);
    }

    public void OnPlayClicked()
    {
        Debug.Log("Play clicked at time: " + Time.realtimeSinceStartup);
        SceneManager.LoadScene("MainGameIntroCutScene");
    }

    public void OnSettingsClicked()
    {
        HideMainMenu();
        settingsPanel.SetActive(true);
        if (fullscreenToggle != null) fullscreenToggle.SetIsOnWithoutNotify(Screen.fullScreen);
        EventSystem.current.SetSelectedGameObject(settingsFirstSelected);
    }

    public void SetFullscreen(bool value)
    {
        Screen.fullScreen = value;
    }

    public void OnQuitClicked()
    {
        Debug.Log("Quit clicked");
        Application.Quit();
    }

    public void onCreditsClicked()
    {
        HideMainMenu();
        creditsPanel.SetActive(true);
        EventSystem.current.SetSelectedGameObject(creditsFirstSelected);
    }

    public void OnBackClicked(GameObject currentPanel)
    {
        Transform parent = currentPanel.transform.parent;
        currentPanel.SetActive(false);

        if (currentPanel == howToPlayPanel)
            FadeBlur(0f);

        if (parent.name == "Main_Canvas")
        {
            ShowMainMenu();
            EventSystem.current.SetSelectedGameObject(mainMenuFirstSelected);
            return;
        }
        parent.gameObject.SetActive(true);
        EventSystem.current.SetSelectedGameObject(mainMenuFirstSelected);
    }

    public void OnHowToPlayClicked()
    {
        HideMainMenu();
        howToPlayPanel.SetActive(true);
        FadeBlur(maxBlurSize);
        EventSystem.current.SetSelectedGameObject(howToPlayFirstSelected);
    }
}