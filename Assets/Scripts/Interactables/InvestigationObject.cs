using UnityEngine;

public class InvestigationObject : Interactable
{
    [TextArea]
    public string investigationText;

    public override void Interact()
    {
        UIManager.Instance.ShowInvestigationPanel(investigationText);
    }
}