using UnityEngine;
using UnityEngine.SceneManagement;

public class MenuManager : MonoBehaviour
{
    [SerializeField] private GameObject mainMenuPanel;
    [SerializeField] private GameObject settingsPanel;
    [SerializeField] private GameObject creditsPanel;
    [SerializeField] private GameObject continuePanel;
    

    public void OnPlayClicked()
    {
        Debug.Log("Play clicked");

        SceneManager.LoadScene("SampleScene");
    }

    public void OnSettingsClicked()
    {
        mainMenuPanel.SetActive(false);
        settingsPanel.SetActive(true);
    }

    public void OnQuitClicked()
    {
        Debug.Log("Quit clicked");
        Application.Quit();
    }
    public void onCreditsClicked()
    {
        mainMenuPanel.SetActive(false);
        creditsPanel.SetActive(true);
    }
    public void OnBackClicked(GameObject currentPanel)
    {
        Transform parent = currentPanel.transform.parent;

        currentPanel.SetActive(false);

        if (parent.name == "Main_Canvas")
        {
            mainMenuPanel.SetActive(true);
            return;
        }

        parent.gameObject.SetActive(true);
    }

    public void OnContinueClicked()
    {
        mainMenuPanel.SetActive(false);
        continuePanel.SetActive(true);
    }
}