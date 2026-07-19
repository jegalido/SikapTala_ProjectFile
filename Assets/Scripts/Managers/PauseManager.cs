using UnityEngine;
using UnityEngine.EventSystems;

public class PauseManager : MonoBehaviour
{
    [SerializeField] private GameObject pausePanel;
    [SerializeField] private GameObject firstSelectedButton;
    [SerializeField] private GameObject pauseMenuPanel;
    [SerializeField] private GameObject settingsPanel;
    [SerializeField] private GameObject LoadPanel;
    [SerializeField] private GameObject SavePanel;

    [Header("Blur")]
    [SerializeField] private Material screenBlurMaterial; // M_ScreenBlur
    [SerializeField] private float maxBlurSize = 3f;       // tune to taste
    [SerializeField] private float blurFadeDuration = 0.25f;

    private static readonly int BlurSizeID = Shader.PropertyToID("_BlurSize");

    private bool isPaused;
    private Coroutine blurRoutine;

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            TogglePause();
        }
    }

    public void TogglePause()
    {
        isPaused = !isPaused;
        if (isPaused)
            PauseGame();
        else
            ResumeGame();
    }

    private void PauseGame()
    {
        Time.timeScale = 0f;
        pausePanel.SetActive(true);
        EventSystem.current.SetSelectedGameObject(firstSelectedButton);
        FadeBlur(maxBlurSize);
    }

    public void ResumeGame()
    {
        Time.timeScale = 1f;
        pausePanel.SetActive(false);
        EventSystem.current.SetSelectedGameObject(null);
        FadeBlur(0f);
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

    public void OnBackClicked(GameObject currentPanel)
    {
        Transform parent = currentPanel.transform.parent;
        currentPanel.SetActive(false);
        if (parent.name == "PauseMenu_Panel")
        {
            pauseMenuPanel.SetActive(true);
            return;
        }
        parent.gameObject.SetActive(true);
    }

    public void onSettingsClicked()
    {
        pauseMenuPanel.SetActive(false);
        settingsPanel.SetActive(true);
    }

    public void onLoadClicked()
    {
        pauseMenuPanel.SetActive(false);
        LoadPanel.SetActive(true);
    }

    public void onSaveClicked()
    {
        pauseMenuPanel.SetActive(false);
        SavePanel.SetActive(true);
    }

    public void onMainMenuClicked()
    {
        Time.timeScale = 1f;
        FadeBlur(0f);
        UnityEngine.SceneManagement.SceneManager.LoadScene("MainMenu");
    }
}