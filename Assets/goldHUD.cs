using TMPro;
using UnityEngine;

public class GoldHUD : MonoBehaviour
{
    public TextMeshProUGUI goldText;
    public string prefix = "Gold: ";

    int _lastShown = int.MinValue;

    void OnEnable()
    {
        if (GoldService.Instance != null)
        {
            GoldService.Instance.OnGoldChanged += Handle;
            Handle(GoldService.Instance.CurrentGold); // immediate sync
        }
    }

    void OnDisable()
    {
        if (GoldService.Instance != null)
            GoldService.Instance.OnGoldChanged -= Handle;
    }

    void Update()  // cheap safety net
    {
        if (GoldService.Instance == null) return;
        var cur = GoldService.Instance.CurrentGold;
        if (cur != _lastShown) Handle(cur);
    }

    void Handle(int value)
    {
        _lastShown = value;
        if (goldText) goldText.text = prefix + value;
    }
}
