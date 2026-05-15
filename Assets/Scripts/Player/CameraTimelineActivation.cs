using UnityEngine;
using UnityEngine.Playables;

public class CameraTimelineActivation : MonoBehaviour
{

    [SerializeField] private PlayableDirector timeline;
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {

            timeline.Play();
            GetComponent<BoxCollider2D>().enabled = false;
        }
    }
}
