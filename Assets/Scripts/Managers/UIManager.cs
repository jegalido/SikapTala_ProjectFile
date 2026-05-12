using UnityEngine;
using TMPro;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance;

    [Header("Prompt UI")]
    public GameObject promptPanel;
    public TMP_Text promptText;

    [Header("Investigation Panel")]
    public GameObject investigatePanel;
    public TMP_Text investigateText;

    void Awake()
    {
        Instance = this;
    }

    public void ShowPrompt(string text)
    {
        promptPanel.SetActive(true);
        promptText.text = text;
    }

    public void HidePrompt()
    {
        promptPanel.SetActive(false);
    }

    public void ShowInvestigationPanel(string text)
    {
        investigatePanel.SetActive(true);
        investigateText.text = text;
    }

    public void HideInvestigationPanel()
    {
        investigatePanel.SetActive(false);
    }
}