using System;
using UnityEngine;

public class GoldService : MonoBehaviour
{
    public static GoldService Instance { get; private set; }

    // Fires with the latest total gold every time it changes
    public event Action<int> OnGoldChanged;

    // Local cache in case MetaProgression isn't ready yet
    private int _cachedGold = 0;

    void Awake()
    {
        if (Instance != null) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);
        HookMeta();
    }

    void OnDestroy()
    {
        UnhookMeta();
        if (Instance == this) Instance = null;
    }

    // Ensure there is always one service available
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    static void Bootstrap()
    {
        if (Instance == null)
        {
            var go = new GameObject("GoldService");
            go.AddComponent<GoldService>();
        }
    }

    void HookMeta()
    {
        if (MetaProgression.Instance != null)
        {
            MetaProgression.Instance.OnDataChanged += HandleMetaChanged;
            _cachedGold = MetaProgression.Instance.GetMetaCurrency();
        }
    }

    void UnhookMeta()
    {
        if (MetaProgression.Instance != null)
            MetaProgression.Instance.OnDataChanged -= HandleMetaChanged;
    }

    void HandleMetaChanged(MetaProgressionData _)
    {
        _cachedGold = MetaProgression.Instance != null
            ? MetaProgression.Instance.GetMetaCurrency()
            : _cachedGold;

        OnGoldChanged?.Invoke(_cachedGold);
    }

    public int CurrentGold =>
        MetaProgression.Instance != null ? MetaProgression.Instance.GetMetaCurrency() : _cachedGold;

    public void Add(int amount)
    {
        if (amount == 0) return;

        if (MetaProgression.Instance != null)
        {
            // This will also trigger OnDataChanged -> HandleMetaChanged -> OnGoldChanged for the HUD
            MetaProgression.Instance.AddMetaCurrency(amount);
        }
        else
        {
            // Fallback if MetaProgression wasn’t in the scene yet
            _cachedGold += amount;
            OnGoldChanged?.Invoke(_cachedGold);
        }
    }
}
