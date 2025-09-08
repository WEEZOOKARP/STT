using UnityEngine;
using UnityEngine.UI;

public class CrosshairController : MonoBehaviour
{
    [Header("Crosshair Settings")]
    public Image crosshairImage;
    public Color defaultColor = Color.white;
    public Color targetColor = Color.yellow;

    [Header("Scale Settings")]
    public float shrinkSize = 0.7f;    // shrink when firing
    public float expandSize = 1.3f;    // expand when aiming at enemy
    public float scaleSpeed = 8f;

    [Header("Raycast Settings")]
    public float detectionRange = 1000f;
    public LayerMask enemyLayer;

    private Vector3 originalScale;
    private bool isFiring = false;
    private bool isOnEnemy = false;

    void Start()
    {
        if (crosshairImage != null)
            originalScale = crosshairImage.rectTransform.localScale;
    }

    void Update()
    {
        if (!isFiring)  // only check enemies if not firing
            DetectEnemy();

        UpdateCrosshair();
    }

    void DetectEnemy()
    {
        Ray ray = new Ray(Camera.main.transform.position, Camera.main.transform.forward);
        RaycastHit hit;

        isOnEnemy = Physics.Raycast(ray, out hit, detectionRange, enemyLayer);
    }

    void UpdateCrosshair()
    {
        Vector3 targetScale = originalScale;
        Color targetColor = defaultColor;

        if (isOnEnemy)
        {
            targetColor = this.targetColor;
            targetScale = originalScale * expandSize;
        }

        crosshairImage.color = Color.Lerp(
            crosshairImage.color,
            targetColor,
            Time.deltaTime * scaleSpeed
        );

        crosshairImage.rectTransform.localScale = Vector3.Lerp(
            crosshairImage.rectTransform.localScale,
            targetScale,
            Time.deltaTime * scaleSpeed
        );
    }

}



