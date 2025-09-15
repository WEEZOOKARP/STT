using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Bullet : MonoBehaviour
{
<<<<<<< Updated upstream
    private void OnCollisionEnter(Collision collision)
=======
    public GameObject bulletImpactPrefab, bloodMistPrefab;
    public int damage=10;

    void OnCollisionEnter(Collision c)
>>>>>>> Stashed changes
    {
        if (collision.gameObject.CompareTag("Enemy"))
        {
            print("hit " + collision.gameObject.name + "!");
            createBulletEffect(collision);
            Destroy(gameObject);
        }
        if (collision.gameObject.CompareTag("Wall"))
        {
            print("hit a wall");
            createBulletEffect(collision);
            Destroy(gameObject);
        }
    }

    void createBulletEffect(Collision collision)
    {
        ContactPoint contact = collision.contacts[0];
        GameObject hole = Instantiate(
            GlobalReferences.Instance.bulletImpactPrefab,
            contact.point,
            Quaternion.LookRotation(contact.normal)
        );
        hole.transform.SetParent(collision.gameObject.transform);
    }
}


