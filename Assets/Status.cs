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
        // Initialize player health
        playerHealth = maxHealth;
        healthBar.SetMaxHealth(maxHealth);
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
        healthBar.SetHealth(playerHealth);
    }

    public void TakeDamage(int amount)
    {
        playerHealth -= amount;
        if (playerHealth < 0)
        {
            playerHealth = 0;
            killPlayer("Enemy Collision");
        }
        healthBar.SetHealth(playerHealth);
    }

    private void killPlayer(String cause)
    {
        bool isdead = true;
    }
}
