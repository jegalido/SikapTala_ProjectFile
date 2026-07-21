using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class SkipSceneOnStart : MonoBehaviour
{
    private void Update()
    {
        Gamepad pad = Gamepad.current;
        if (pad == null) return;

        if (pad.startButton.wasPressedThisFrame)
        {
            SkipToNextScene();
        }
    }

    private void SkipToNextScene()
    {
        int nextSceneIndex = SceneManager.GetActiveScene().buildIndex + 1;

        if (nextSceneIndex < SceneManager.sceneCountInBuildSettings)
        {
            SceneManager.LoadScene(nextSceneIndex);
        }
        else
        {
            SceneManager.LoadScene(1); // loop back, matches your TimelineSceneLoader fallback
        }
    }
}