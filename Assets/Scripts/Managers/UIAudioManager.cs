using UnityEngine;

public class UIAudioManager : MonoBehaviour
{
    public static UIAudioManager Instance;

    public AudioSource audioSource;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void PlayClick(AudioClip clip)
    {
        audioSource.PlayOneShot(clip);
    }
}