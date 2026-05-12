using UnityEngine;
using UnityEngine.SceneManagement;

public class IntroManager : MonoBehaviour
{
    float cs = 8.0f;
    void Update()
    {
        cs -= Time.deltaTime;
        if (cs <= 0)
        {
            SceneManager.LoadScene("MainMenu");
        }
    }
}