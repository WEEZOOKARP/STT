using UnityEngine;

public class Bullet : MonoBehaviour
{
    public GameObject bulletImpactPrefab;

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Enemy"))
        {
            Debug.Log($"Hit {collision.gameObject.name}!");
            CreateBulletEffect(collision);
            Destroy(gameObject, 0.05f); // small delay for safety
        }
        else if (collision.gameObject.CompareTag("Wall"))
        {
            Debug.Log("Hit a wall");
            CreateBulletEffect(collision);
            Destroy(gameObject, 0.05f);
        }
    }

    private void CreateBulletEffect(Collision collision)
    {
        if (bulletImpactPrefab == null)
        {
            Debug.LogError("Bullet impact prefab is NOT assigned!");
            return;
        }

        // Slight offset to avoid the hole being hidden inside the surface
        ContactPoint contact = collision.contacts[0];
        Vector3 spawnPos = contact.point + contact.normal * 0.01f;

        GameObject hole = Instantiate(
            bulletImpactPrefab,
            spawnPos,
            Quaternion.LookRotation(contact.normal)
        );

        // Optional: Parent to hit object only if needed (e.g., moving enemy)
        if (!collision.gameObject.CompareTag("Wall"))
            hole.transform.SetParent(collision.transform);
    }
}
