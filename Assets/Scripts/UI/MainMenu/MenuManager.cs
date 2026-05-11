using UnityEngine;
using UnityEngine.SceneManagement;

public class MenuManager : MonoBehaviour
{
    public void OnPlayClicked()
    {
        Debug.Log("Play clicked");

        // SceneManager.LoadScene("GameScene");
    }

    public void OnSettingsClicked()
    {
        Debug.Log("Settings clicked");
    }

    public void OnQuitClicked()
    {
        Debug.Log("Quit clicked");
        Application.Quit();
    }
    public void onCreditsClicked()
    {
        Debug.Log("Credits clicked");
    }
}