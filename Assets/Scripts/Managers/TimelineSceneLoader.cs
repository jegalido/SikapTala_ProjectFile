using UnityEngine;
using UnityEngine.Playables; 
using UnityEngine.SceneManagement; 

public class TimelineSceneLoader : MonoBehaviour
{
    [Header("References")]
    public PlayableDirector director;

    private void OnEnable()
    {
        if (director != null)
            director.stopped += OnTimelineFinished;
    }

    private void OnDisable()
    {
        if (director != null)
            director.stopped -= OnTimelineFinished;
    }

    private void OnTimelineFinished(PlayableDirector aDirector)
    {
        Debug.Log("Timeline finished. Loading next scene...");
        
        int nextSceneIndex = SceneManager.GetActiveScene().buildIndex + 1;
        
        if (nextSceneIndex < SceneManager.sceneCountInBuildSettings)
        {
            SceneManager.LoadScene(nextSceneIndex);
        }
        else
        {
            SceneManager.LoadScene(1); // Loop back to the first scene if we've reached the end
        }
    }
}
