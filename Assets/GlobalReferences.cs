using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class GlobalReferences : MonoBehaviour
{
    public static GlobalReferences Instance { get; set; }
    public GameObject bulletImpactPrefab;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
        }
        else
        {
            Instance = this;
        }
    }
}
