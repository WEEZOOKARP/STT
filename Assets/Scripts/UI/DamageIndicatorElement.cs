using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(RectTransform))]
public class DamageIndicatorElement : MonoBehaviour
{
    [SerializeField] private Image icon;
    [SerializeField] private CanvasGroup canvasGroup;

    private RectTransform rectTransform;
    private float lifetime;
    private float remainingLifetime;

    void Awake()
    {
        rectTransform = GetComponent<RectTransform>();

        if (icon == null)
        {
            icon = GetComponent<Image>();
            if (icon == null)
            {
                icon = gameObject.AddComponent<Image>();
            }
        }

        if (canvasGroup == null)
        {
            canvasGroup = GetComponent<CanvasGroup>();
            if (canvasGroup == null)
            {
                canvasGroup = gameObject.AddComponent<CanvasGroup>();
            }
        }

        rectTransform.anchorMin = rectTransform.anchorMax = new Vector2(0.5f, 0.5f);
        rectTransform.pivot = new Vector2(0.5f, 0.5f);
    }

    public void Prime(float lifetimeSeconds, Color tint)
    {
        lifetime = Mathf.Max(0.05f, lifetimeSeconds);
        remainingLifetime = lifetime;
        SetColor(tint);
        canvasGroup.alpha = 1f;
        rectTransform.anchoredPosition = Vector2.zero;
        rectTransform.localScale = Vector3.one;
        gameObject.SetActive(true);
    }

    public bool Tick(float deltaTime)
    {
        remainingLifetime -= deltaTime;
        float normalized = 1f - Mathf.Clamp01(remainingLifetime / lifetime);
        canvasGroup.alpha = 1f - normalized;
        return remainingLifetime <= 0f;
    }

    public void SetDirection(float signedAngle)
    {
        rectTransform.localEulerAngles = new Vector3(0f, 0f, -signedAngle);
    }

    public void SetScale(float scale)
    {
        rectTransform.localScale = Vector3.one * Mathf.Max(0.2f, scale);
    }

    public void SetColor(Color color)
    {
        if (icon != null)
        {
            icon.color = color;
        }
    }
}
