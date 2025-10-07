using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class MetaProgressionUI : MonoBehaviour
{
    [Header("UI References")]
    public GameObject shopPanel;
    public Button healthUpgradeButton;
    public Button damageUpgradeButton;
    public Button speedUpgradeButton;
    public Button ammoUpgradeButton;
    public Button experienceUpgradeButton;
    
    [Header("Text References")]
    public TextMeshProUGUI currencyText;
    public TextMeshProUGUI healthCostText;
    public TextMeshProUGUI damageCostText;
    public TextMeshProUGUI speedCostText;
    public TextMeshProUGUI ammoCostText;
    public TextMeshProUGUI experienceCostText;
    
    [Header("Stats Display")]
    public TextMeshProUGUI healthBonusText;
    public TextMeshProUGUI damageBonusText;
    public TextMeshProUGUI speedBonusText;
    public TextMeshProUGUI ammoBonusText;
    public TextMeshProUGUI experienceBonusText;
    
    private MetaProgression metaProgression;
    
    void Start()
    {
        metaProgression = MetaProgression.Instance;
        if (metaProgression == null)
        {
            metaProgression = FindObjectOfType<MetaProgression>();
        }
        
        if (metaProgression != null)
        {
            metaProgression.OnDataChanged += UpdateUI;
            metaProgression.OnUpgradePurchased += OnUpgradePurchased;
        }
        
        SetupButtons();
        UpdateUI(metaProgression?.GetData());
    }
    
    void OnDestroy()
    {
        if (metaProgression != null)
        {
            metaProgression.OnDataChanged -= UpdateUI;
            metaProgression.OnUpgradePurchased -= OnUpgradePurchased;
        }
    }
    
    void SetupButtons()
    {
        if (healthUpgradeButton != null)
        {
            healthUpgradeButton.onClick.AddListener(() => PurchaseUpgrade("Health"));
        }
        
        if (damageUpgradeButton != null)
        {
            damageUpgradeButton.onClick.AddListener(() => PurchaseUpgrade("Damage"));
        }
        
        if (speedUpgradeButton != null)
        {
            speedUpgradeButton.onClick.AddListener(() => PurchaseUpgrade("Speed"));
        }
        
        if (ammoUpgradeButton != null)
        {
            ammoUpgradeButton.onClick.AddListener(() => PurchaseUpgrade("Ammo"));
        }
        
        if (experienceUpgradeButton != null)
        {
            experienceUpgradeButton.onClick.AddListener(() => PurchaseUpgrade("Experience"));
        }
    }
    
    void UpdateUI(MetaProgressionData data)
    {
        if (data == null) return;
        
        // Update currency display
        if (currencyText != null)
        {
            currencyText.text = $"Meta Currency: {data.metaCurrency}";
        }
        
        // Update cost texts
        if (healthCostText != null)
        {
            healthCostText.text = $"Cost: {metaProgression.healthUpgradeCost}";
        }
        
        if (damageCostText != null)
        {
            damageCostText.text = $"Cost: {metaProgression.damageUpgradeCost}";
        }
        
        if (speedCostText != null)
        {
            speedCostText.text = $"Cost: {metaProgression.speedUpgradeCost}";
        }
        
        if (ammoCostText != null)
        {
            ammoCostText.text = $"Cost: {metaProgression.ammoUpgradeCost}";
        }
        
        if (experienceCostText != null)
        {
            experienceCostText.text = $"Cost: {metaProgression.experienceUpgradeCost}";
        }
        
        // Update stats display
        if (healthBonusText != null)
        {
            healthBonusText.text = $"Health Bonus: +{data.permanentHealthBonus}";
        }
        
        if (damageBonusText != null)
        {
            damageBonusText.text = $"Damage Bonus: +{data.permanentDamageBonus}";
        }
        
        if (speedBonusText != null)
        {
            speedBonusText.text = $"Speed Bonus: +{data.permanentSpeedBonus:F1}";
        }
        
        if (ammoBonusText != null)
        {
            ammoBonusText.text = $"Ammo Bonus: +{data.permanentAmmoBonus}";
        }
        
        if (experienceBonusText != null)
        {
            experienceBonusText.text = $"XP Bonus: +{data.permanentExperienceBonus:P0}";
        }
        
        // Update button interactability
        UpdateButtonStates(data);
    }
    
    void UpdateButtonStates(MetaProgressionData data)
    {
        if (healthUpgradeButton != null)
        {
            healthUpgradeButton.interactable = metaProgression.CanPurchaseUpgrade("Health");
        }
        
        if (damageUpgradeButton != null)
        {
            damageUpgradeButton.interactable = metaProgression.CanPurchaseUpgrade("Damage");
        }
        
        if (speedUpgradeButton != null)
        {
            speedUpgradeButton.interactable = metaProgression.CanPurchaseUpgrade("Speed");
        }
        
        if (ammoUpgradeButton != null)
        {
            ammoUpgradeButton.interactable = metaProgression.CanPurchaseUpgrade("Ammo");
        }
        
        if (experienceUpgradeButton != null)
        {
            experienceUpgradeButton.interactable = metaProgression.CanPurchaseUpgrade("Experience");
        }
    }
    
    void PurchaseUpgrade(string upgradeType)
    {
        if (metaProgression != null)
        {
            bool success = metaProgression.PurchaseUpgrade(upgradeType);
            if (success)
            {
                Debug.Log($"Successfully purchased {upgradeType} upgrade!");
                
                // Play success sound or show notification
                ShowPurchaseNotification(upgradeType);
            }
            else
            {
                Debug.Log($"Not enough currency for {upgradeType} upgrade!");
                
                // Play error sound or show notification
                ShowInsufficientCurrencyNotification();
            }
        }
    }
    
    void OnUpgradePurchased(string upgradeType)
    {
        // This is called when an upgrade is successfully purchased
        Debug.Log($"Upgrade purchased: {upgradeType}");
    }
    
    void ShowPurchaseNotification(string upgradeType)
    {
        // Create a simple notification
        Debug.Log($"<color=green>✓ {upgradeType} upgrade purchased!</color>");
        
        // You can add more sophisticated UI notifications here
        // For example, show a popup or play a sound
    }
    
    void ShowInsufficientCurrencyNotification()
    {
        // Create a simple notification
        Debug.Log($"<color=red>✗ Insufficient currency!</color>");
        
        // You can add more sophisticated UI notifications here
        // For example, show a popup or play a sound
    }
    
    // UI Button Methods
    public void ToggleShopPanel()
    {
        if (shopPanel != null)
        {
            shopPanel.SetActive(!shopPanel.activeSelf);
        }
    }
    
    public void ShowShopPanel()
    {
        if (shopPanel != null)
        {
            shopPanel.SetActive(true);
        }
    }
    
    public void HideShopPanel()
    {
        if (shopPanel != null)
        {
            shopPanel.SetActive(false);
        }
    }
    
    // Debug methods
    [ContextMenu("Add Test Currency")]
    void AddTestCurrency()
    {
        if (metaProgression != null)
        {
            metaProgression.AddMetaCurrency(1000);
        }
    }
    
    [ContextMenu("Reset All Progress")]
    void ResetAllProgress()
    {
        if (metaProgression != null)
        {
            metaProgression.ResetAllProgress();
        }
    }
}
