using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DamageIndicatorController : MonoBehaviour
{
    public static DamageIndicatorController Instance { get; private set; }

    [Header("Indicator Setup")]
    [SerializeField] private DamageIndicatorElement indicatorPrefab;
    [SerializeField] private RectTransform indicatorContainer;
    [SerializeField] private float indicatorLifetime = 1.35f;
    [SerializeField] private float maxDistanceForScale = 30f;
    [SerializeField] private AnimationCurve distanceScaleCurve = AnimationCurve.Linear(0f, 1.1f, 1f, 0.55f);

    [Header("Colors")]
    [SerializeField] private Color playerDamageColor = new Color(1f, 0.45f, 0.1f);
    [SerializeField] private Color strongholdDamageColor = new Color(0.2f, 0.65f, 1f);

    [Header("Tracking")]
    [SerializeField] private Transform playerTransform;
    [SerializeField] private Camera playerCamera;
    [SerializeField] private bool persistBetweenScenes = true;

    private readonly List<IndicatorRuntime> activeIndicators = new List<IndicatorRuntime>();
    private readonly Queue<DamageIndicatorElement> pooledIndicators = new Queue<DamageIndicatorElement>();
    private static Sprite fallbackSprite;

    private class IndicatorRuntime
    {
        public DamageIndicatorElement Element;
        public Vector3 WorldPosition;
    }

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        if (persistBetweenScenes)
        {
            DontDestroyOnLoad(gameObject);
        }

        if (indicatorLifetime <= 0f)
        {
            indicatorLifetime = 1f;
        }

        if (playerCamera == null)
        {
            playerCamera = Camera.main;
        }

        if (playerTransform == null)
        {
            Status existingStatus = FindFirstObjectByType<Status>();
            if (existingStatus != null)
            {
                playerTransform = existingStatus.transform;
            }
        }

        EnsureContainer();
        EnsurePrefab();
    }

    void Update()
    {
        if (activeIndicators.Count == 0)
        {
            return;
        }

        Transform observer = GetObserver();
        if (observer == null)
        {
            return;
        }

        for (int i = activeIndicators.Count - 1; i >= 0; i--)
        {
            IndicatorRuntime runtime = activeIndicators[i];
            runtime.Element.SetDirection(CalculateAngle(observer, runtime.WorldPosition));
            runtime.Element.SetScale(CalculateScale(observer, runtime.WorldPosition));

            if (runtime.Element.Tick(Time.deltaTime))
            {
                Recycle(runtime.Element);
                activeIndicators.RemoveAt(i);
            }
        }
    }

    public void RegisterPlayer(Transform player)
    {
        playerTransform = player;
    }

    public void RegisterCamera(Camera camera)
    {
        playerCamera = camera;
    }

    public void ReportDamage(Vector3 worldSourcePosition, DamageIndicatorType type)
    {
        Transform observer = GetObserver();
        if (observer == null || indicatorContainer == null)
        {
            return;
        }

        DamageIndicatorElement element = GetInstance();
        element.Prime(indicatorLifetime, ResolveColor(type));

        var runtime = new IndicatorRuntime
        {
            Element = element,
            WorldPosition = worldSourcePosition
        };

        runtime.Element.SetDirection(CalculateAngle(observer, runtime.WorldPosition));
        runtime.Element.SetScale(CalculateScale(observer, runtime.WorldPosition));
        activeIndicators.Add(runtime);
    }

    Transform GetObserver()
    {
        if (playerTransform != null)
        {
            return playerTransform;
        }

        if (playerCamera == null)
        {
            playerCamera = Camera.main;
        }

        return playerCamera != null ? playerCamera.transform : null;
    }

    float CalculateAngle(Transform observer, Vector3 sourcePosition)
    {
        Vector3 direction = sourcePosition - observer.position;
        if (direction.sqrMagnitude < 0.0001f)
        {
            direction = observer.forward;
        }

        return DamageIndicatorMath.CalculateSignedAngle(observer.forward, direction, observer.up);
    }

    float CalculateScale(Transform observer, Vector3 sourcePosition)
    {
        Vector3 direction = sourcePosition - observer.position;
        float normalized = DamageIndicatorMath.NormalizeDistance(direction.magnitude, maxDistanceForScale);

        if (distanceScaleCurve == null)
        {
            return Mathf.Lerp(1f, 0.35f, normalized);
        }

        return distanceScaleCurve.Evaluate(normalized);
    }

    DamageIndicatorElement GetInstance()
    {
        DamageIndicatorElement element;

        if (pooledIndicators.Count > 0)
        {
            element = pooledIndicators.Dequeue();
        }
        else
        {
            element = Instantiate(indicatorPrefab, indicatorContainer);
        }

        RectTransform rect = element.GetComponent<RectTransform>();
        rect.SetParent(indicatorContainer, false);
        rect.anchoredPosition = Vector2.zero;
        rect.localScale = Vector3.one;
        element.gameObject.SetActive(true);
        return element;
    }

    void Recycle(DamageIndicatorElement element)
    {
        if (element == null)
        {
            return;
        }

        element.gameObject.SetActive(false);
        pooledIndicators.Enqueue(element);
    }

    void EnsureContainer()
    {
        if (indicatorContainer != null)
        {
            return;
        }

        Canvas canvas = FindFirstObjectByType<Canvas>();
        if (canvas == null)
        {
            GameObject canvasObj = new GameObject("DamageIndicatorCanvas", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            canvas = canvasObj.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;

            CanvasScaler scaler = canvasObj.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920f, 1080f);
        }

        GameObject containerObj = new GameObject("DamageIndicators", typeof(RectTransform));
        containerObj.transform.SetParent(canvas.transform, false);
        indicatorContainer = containerObj.GetComponent<RectTransform>();
        indicatorContainer.anchorMin = indicatorContainer.anchorMax = new Vector2(0.5f, 0.5f);
        indicatorContainer.pivot = new Vector2(0.5f, 0.5f);
        indicatorContainer.anchoredPosition = Vector2.zero;
        indicatorContainer.sizeDelta = Vector2.zero;
    }

    void EnsurePrefab()
    {
        if (indicatorPrefab != null)
        {
            return;
        }

        GameObject template = new GameObject("DamageIndicatorTemplate", typeof(RectTransform), typeof(CanvasGroup), typeof(Image));
        template.transform.SetParent(transform, false);

        RectTransform rect = template.GetComponent<RectTransform>();
        rect.sizeDelta = new Vector2(90f, 12f);

        Image icon = template.GetComponent<Image>();
        icon.raycastTarget = false;
        icon.sprite = GetFallbackSprite();

        indicatorPrefab = template.AddComponent<DamageIndicatorElement>();
        template.SetActive(false);
    }

    Sprite GetFallbackSprite()
    {
        if (fallbackSprite != null)
        {
            return fallbackSprite;
        }

        Texture2D texture = new Texture2D(2, 2)
        {
            filterMode = FilterMode.Bilinear,
            wrapMode = TextureWrapMode.Clamp
        };

        for (int x = 0; x < texture.width; x++)
        {
            for (int y = 0; y < texture.height; y++)
            {
                texture.SetPixel(x, y, Color.white);
            }
        }

        texture.Apply();
        fallbackSprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0.5f, 0.5f));
        return fallbackSprite;
    }

    Color ResolveColor(DamageIndicatorType type)
    {
        return type == DamageIndicatorType.Stronghold ? strongholdDamageColor : playerDamageColor;
    }
}
