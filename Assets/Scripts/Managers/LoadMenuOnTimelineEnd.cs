using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.SceneManagement;

/// <summary>
/// Loads a scene (default: MainMenu) when a Timeline / PlayableDirector finishes.
/// Put this on the cutscene's director object so the ending cutscenes return to
/// the main menu regardless of build-index order.
/// </summary>
public class LoadMenuOnTimelineEnd : MonoBehaviour
{
    public PlayableDirector director;
    public string sceneToLoad = "MainMenu";

    private void OnEnable()
    {
        if (director == null) director = GetComponent<PlayableDirector>();
        if (director != null) director.stopped += OnStopped;
    }

    private void OnDisable()
    {
        if (director != null) director.stopped -= OnStopped;
    }

    private void OnStopped(PlayableDirector d)
    {
        SceneManager.LoadScene(sceneToLoad);
    }
}
