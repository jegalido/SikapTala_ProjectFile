using UnityEngine;

/// <summary>
/// Put on the level-end trigger (replaces LoadNextSceneUponTouch). When the player
/// touches it, shows the EndingChoice screen instead of loading a scene directly.
/// </summary>
[RequireComponent(typeof(Collider2D))]
public class EndingChoiceTrigger : MonoBehaviour
{
    [SerializeField] private EndingChoice choice;
    private bool triggered;

    private void Start()
    {
        Collider2D col = GetComponent<Collider2D>();
        if (col != null) col.isTrigger = true;
        if (choice == null) choice = FindFirstObjectByType<EndingChoice>(FindObjectsInactive.Include);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (triggered) return;
        if (!other.CompareTag("Player")) return;
        triggered = true;
        if (choice != null) choice.Show();
        else Debug.LogWarning("EndingChoiceTrigger: no EndingChoice assigned/found.");
    }
}
