using System;
using UnityEngine;

public class Status : MonoBehaviour
{
    public int maxHealth = 100;
    private int playerHealth = 0;

    public healthBar healthBar;
    public GameObject player;
    public bool isdead = false;

    void Start()
    {
        // Apply meta progression bonuses
        ApplyMetaProgressionBonuses();
        
        // Initialize player health
        playerHealth = maxHealth;
        if (healthBar != null)
        {
            healthBar.SetMaxHealth(maxHealth);
        }
    }

    void Update()
    {
        // Testing controls
        if (Input.GetKeyDown(KeyCode.H))
        {
            Heal(20);
        }
    }
    
    void ApplyMetaProgressionBonuses()
    {
        if (MetaProgression.Instance != null)
        {
            // Apply permanent health bonus
            maxHealth += MetaProgression.Instance.GetHealthBonus();
            
            // Apply other bonuses (you can extend this based on your game mechanics)
            Debug.Log($"Applied meta progression bonuses: +{MetaProgression.Instance.GetHealthBonus()} health");
        }
    }

    private void OnCollisionStay(Collision collision)
    {
        if (collision.gameObject.CompareTag("Enemy"))
        {
            TakeDamage(1);
            //collision.gameObject.GetComponent<Renderer>().material = losermat; 
        }
    }

    public void Heal(int amount)
    {
        playerHealth += amount;
        if (playerHealth > maxHealth)
        {
            playerHealth = maxHealth;
        }
        if (healthBar != null)
        {
            healthBar.SetHealth(playerHealth);
        }
    }

    public void TakeDamage(int amount)
    {
        TakeDamage(amount, null);
    }

    public void TakeDamage(int amount, Vector3? sourcePosition)
    {
        playerHealth -= amount;

        // Debug message
        Debug.Log($"Player took {amount} damage! Current HP: {playerHealth}/{maxHealth}");

        if (sourcePosition.HasValue && DamageIndicatorController.Instance != null)
        {
            DamageIndicatorController.Instance.ReportDamage(sourcePosition.Value, DamageIndicatorType.Player);
        }

        if (playerHealth < 0)
        {
            playerHealth = 0;
            killPlayer("Enemy Collision");
        }
        if (healthBar != null)
        {
            healthBar.SetHealth(playerHealth);
        }
    }

    private void killPlayer(string cause)
    {
        isdead = true;
        Debug.Log($"Player died due to {cause}");
    }
}
