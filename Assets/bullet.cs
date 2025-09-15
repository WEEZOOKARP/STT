using UnityEngine;

public class Bullet : MonoBehaviour
{
    public GameObject bulletImpactPrefab, bloodMistPrefab;
    public int damage=10;

    void OnCollisionEnter(Collision c)
    {
        if (c.gameObject.CompareTag("shootableObject") || c.gameObject.CompareTag("Wall"))
        {
            Debug.Log($"Hit {c.gameObject.name}!");
            CreateEffect(c, bulletImpactPrefab);
        }
        else if (c.gameObject.CompareTag("Enemy"))
        {
            Debug.Log($"Hit {c.gameObject.name}!");
            CreateEffect(c, bloodMistPrefab);
            EnemyBehavior enemy = c.gameObject.GetComponent<EnemyBehavior>();
            if (enemy != null)
            {
                enemy.TakeDamage(damage);
            }
        }
        Destroy(gameObject, 0.05f);
    }

    void CreateEffect(Collision c, GameObject prefab)
    {
        if (!prefab) { Debug.LogError("Impact prefab not assigned!"); return; }

        ContactPoint contact = c.contacts[0];
        GameObject fx = Instantiate(prefab,
            contact.point + contact.normal * 0.01f,
            Quaternion.LookRotation(contact.normal));

        if (!c.gameObject.CompareTag("Wall"))
            fx.transform.SetParent(c.transform);
    }
}

