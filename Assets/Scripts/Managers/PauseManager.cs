using UnityEngine;
using UnityEngine.EventSystems;

public class PauseManager : MonoBehaviour
{
    [SerializeField] private GameObject pausePanel; // the whole pause panel
    [SerializeField] private GameObject firstSelectedButton;

    [SerializeField] private GameObject pauseMenuPanel; // buttons container and "pause text"
    [SerializeField] private GameObject settingsPanel;
    [SerializeField] private GameObject LoadPanel;
    [SerializeField] private GameObject SavePanel;


    private bool isPaused;

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



        EventSystem.current.SetSelectedGameObject(
            firstSelectedButton
        );
    }

    public void ResumeGame()
    {
        Time.timeScale = 1f;

        pausePanel.SetActive(false);

        EventSystem.current.SetSelectedGameObject(
            null
        );
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
        UnityEngine.SceneManagement.SceneManager.LoadScene("MainMenu");
    }

}