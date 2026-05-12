using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Button))]
public class UIButtonClickSound : MonoBehaviour
{
    public AudioClip clickSound;

    void Awake()
    {
        GetComponent<Button>().onClick.AddListener(PlaySound);
    }

    void PlaySound()
    {
        if (UIAudioManager.Instance != null)
        {
            UIAudioManager.Instance.PlayClick(clickSound);
        }
    }
}