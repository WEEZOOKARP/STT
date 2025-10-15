using UnityEngine;
using TMPro;

public class warningCuscene : MonoBehaviour
{
    public static warningCuscene Instance;

    [Header("UI References")]
    public GameObject warningCusceneUI;
    public CanvasGroup canvasGroup;
    public TMP_Text warningText;

    [Header("Animation Settings")]
    public float showDuration = 2.5f;
    public float hideSpeed = 3f; // How fast it shrinks
    public float showScale = 1f;
    public float hideScaleY = 0f;

    [Header("Audio Settings")]
    public AudioSource warningSoundSource;
    public AudioClip warningSound;

    private bool isHiding = false;
    private Vector3 originalScale;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    private void Start()
    {
        if (warningCusceneUI)
        {
            warningCusceneUI.SetActive(false);
            originalScale = warningCusceneUI.transform.localScale;
        }
        if (warningText) warningText.gameObject.SetActive(false);
        if (canvasGroup) canvasGroup.alpha = 0;
    }

    public void showWarningCuscene()
    {
        if (!warningCusceneUI || !canvasGroup || !warningText)
            return;

        StopAllCoroutines(); // stop any previous animation
        isHiding = false;

        warningCusceneUI.SetActive(true);
        warningText.gameObject.SetActive(true);
        warningCusceneUI.transform.localScale = new Vector3(originalScale.x, showScale, originalScale.z);
        canvasGroup.alpha = 1;

        // Play warning sound
        if (warningSoundSource && warningSound)
        {
          warningSoundSource.PlayOneShot(warningSound);
        }

        CancelInvoke(nameof(StartHideAnimation));
        Invoke(nameof(StartHideAnimation), showDuration);
    }

    private void StartHideAnimation()
    {
        if (!isHiding)
            StartCoroutine(HideAnimation());
    }

    private System.Collections.IEnumerator HideAnimation()
    {
        isHiding = true;
        float t = 0f;
        Vector3 startScale = warningCusceneUI.transform.localScale;
        Vector3 endScale = new Vector3(startScale.x, hideScaleY, startScale.z);

        while (t < 1f)
        {
            t += Time.deltaTime * hideSpeed;
            warningCusceneUI.transform.localScale = Vector3.Lerp(startScale, endScale, t);
            canvasGroup.alpha = Mathf.Lerp(1f, 0f, t);
            yield return null;
        }

        canvasGroup.alpha = 0;
        warningText.gameObject.SetActive(false);
        warningCusceneUI.SetActive(false);

        // reset scale for next show
        warningCusceneUI.transform.localScale = originalScale;
        isHiding = false;
    }
}
