using UnityEngine;


[CreateAssetMenu(menuName = "Dialogue/Dialogue Line")]
public class DialogeLine : ScriptableObject
{
    

    public string speakerName;
    [TextArea(2, 5)]
    public string dialogueText;
}
