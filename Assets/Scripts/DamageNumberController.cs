using UnityEngine;
using TMPro;
using System.Collections;
using System.Collections.Generic;

public class DamageNumberController : MonoBehaviour
{
    [Header("Damage Number Settings")]
    public GameObject damageNumberPrefab;
    public Transform damageNumberParent;
    
    [Header("Animation Settings")]
    public float moveDistance = 2f;
    public float animationDuration = 1f;
    public float fadeStartTime = 0.5f;
    
    [Header("Color Settings")]
    public Color normalDamageColor = Color.white;
    public Color criticalDamageColor = Color.yellow;
    public Color bossDamageColor = Color.red;
    public Color strongholdDamageColor = new Color(1f, 0.5f, 0f, 1f);
    
    private Queue<GameObject> damageNumberPool = new Queue<GameObject>();
    private const int POOL_SIZE = 20;
    
    public static DamageNumberController Instance { get; private set; }
    
    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    void Start()
    {
        InitializePool();
    }
    
    void InitializePool()
    {
        if (damageNumberPrefab == null)
        {
            Debug.LogError("DamageNumberController: No damage number prefab assigned!");
            return;
        }

        if (damageNumberParent == null)
        {
            damageNumberParent = transform;
        }
        
        for (int i = 0; i < POOL_SIZE; i++)
        {
            GameObject damageNumber = Instantiate(damageNumberPrefab);
            damageNumber.SetActive(false);
            damageNumber.transform.SetParent(damageNumberParent);
            damageNumberPool.Enqueue(damageNumber);
        }
    }
    
    public void ShowDamageNumber(Vector3 worldPosition, int damage, bool isBoss = false, bool isCritical = false, bool isStronghold = false)
    {
        GameObject damageNumber = GetPooledDamageNumber();
        if (damageNumber == null) return;
        
        // Set up the damage number - try TextMeshPro first, then TextMesh
        TextMeshProUGUI textProComponent = damageNumber.GetComponent<TextMeshProUGUI>();
        TextMesh textMeshComponent = damageNumber.GetComponent<TextMesh>();
        
        if (textProComponent != null)
        {
            textProComponent.text = damage.ToString();
            
            // Set color based on damage type
            if (isStronghold)
                textProComponent.color = strongholdDamageColor;
            else if (isBoss)
                textProComponent.color = bossDamageColor;
            else if (isCritical)
                textProComponent.color = criticalDamageColor;
            else
                textProComponent.color = normalDamageColor;
        }
        else if (textMeshComponent != null)
        {
            textMeshComponent.text = damage.ToString();
            
            // Set color based on damage type
            if (isStronghold)
                textMeshComponent.color = strongholdDamageColor;
            else if (isBoss)
                textMeshComponent.color = bossDamageColor;
            else if (isCritical)
                textMeshComponent.color = criticalDamageColor;
            else
                textMeshComponent.color = normalDamageColor;
        }
        
        // Position the damage number
        damageNumber.transform.position = worldPosition + Vector3.up * 0.5f;
        damageNumber.SetActive(true);
        
        // Start animation
        StartCoroutine(AnimateDamageNumber(damageNumber));
    }
    
    GameObject GetPooledDamageNumber()
    {
        if (damageNumberPool.Count > 0)
        {
            return damageNumberPool.Dequeue();
        }
        else
        {
            // Pool is empty, create a new one
            GameObject newDamageNumber = Instantiate(damageNumberPrefab);
            newDamageNumber.transform.SetParent(damageNumberParent);
            return newDamageNumber;
        }
    }
    
    void ReturnToPool(GameObject damageNumber)
    {
        damageNumber.SetActive(false);
        damageNumber.transform.SetParent(damageNumberParent);
        damageNumberPool.Enqueue(damageNumber);
    }
    
    IEnumerator AnimateDamageNumber(GameObject damageNumber)
    {
        Vector3 startPosition = damageNumber.transform.position;
        Vector3 endPosition = startPosition + Vector3.up * moveDistance;
        
        TextMeshProUGUI textProComponent = damageNumber.GetComponent<TextMeshProUGUI>();
        TextMesh textMeshComponent = damageNumber.GetComponent<TextMesh>();
        
        Color originalColor = Color.white;
        if (textProComponent != null)
            originalColor = textProComponent.color;
        else if (textMeshComponent != null)
            originalColor = textMeshComponent.color;
        
        float elapsedTime = 0f;
        
        while (elapsedTime < animationDuration)
        {
            elapsedTime += Time.deltaTime;
            float progress = elapsedTime / animationDuration;
            
            // Move upward
            damageNumber.transform.position = Vector3.Lerp(startPosition, endPosition, progress);
            
            // Fade out after fadeStartTime
            if (elapsedTime >= fadeStartTime)
            {
                float fadeProgress = (elapsedTime - fadeStartTime) / (animationDuration - fadeStartTime);
                Color currentColor = originalColor;
                currentColor.a = Mathf.Lerp(1f, 0f, fadeProgress);
                
                if (textProComponent != null)
                    textProComponent.color = currentColor;
                else if (textMeshComponent != null)
                    textMeshComponent.color = currentColor;
            }
            
            yield return null;
        }
        
        // Return to pool
        ReturnToPool(damageNumber);
    }
}
