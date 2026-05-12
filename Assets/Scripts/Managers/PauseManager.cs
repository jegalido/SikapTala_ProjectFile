using UnityEngine;
using UnityEngine.EventSystems;

public class PauseManager : MonoBehaviour
{
    [SerializeField] private GameObject pausePanel;
    [SerializeField] private GameObject firstSelectedButton;

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
}