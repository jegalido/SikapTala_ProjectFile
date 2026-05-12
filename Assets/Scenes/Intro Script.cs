using UnityEngine;
using UnityEngine.SceneManagement;

public class IntroManager : MonoBehaviour
{
    float cs = 9.0f;
    void Update()
    {
        cs -= Time.deltaTime;
        if (cs <= 0)
        {
            SceneManager.LoadScene("MainMenu");
        }
    }
}