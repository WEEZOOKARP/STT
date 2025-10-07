


using UnityEngine;

public class BakeAndSwitchUI : MonoBehaviour
{
    [Header("References")]
    public ARNavMeshBakeOnClick baker;   // <- drag the component with BakeNow on it

    [Header("UI Panels")]
    public GameObject scanPanel;
    public GameObject placementPanel;

    public void BakeThenSwitch()
    {
        if (!baker) { Debug.LogError("[BakeAndSwitchUI] No baker assigned."); return; }

        baker.BakeNow();                           // call the real method directly
        if (scanPanel) scanPanel.SetActive(false);
        if (placementPanel) placementPanel.SetActive(true);
    }



// if you ever want to just flip UI from another button
public void SwitchOnly()
    {
        if (scanPanel) scanPanel.SetActive(false);
        if (placementPanel) placementPanel.SetActive(true);
    }
}
