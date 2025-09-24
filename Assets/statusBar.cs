using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Collections.Generic;

public class statusBar : MonoBehaviour
{
    public TMP_Text statTextIntro;
    public bool enableScroll = true;
    public float scrollSpeed = 50f;
    public healthBar hpBar;

    [Header("Status icon")]
    public Image defenceIcon;
    public Image regenIcon;
    public Image powerIcon;

    private RectTransform textRectTransform;
    private RectTransform parentRectTransform;
    private float textWidth;
    private float parentWidth;
    private string displayText;
    private string lastDisplayText;
    private string newDisplayText;
    private Color dangerColor = Color.red;
    private Color normalColor = Color.white;

    private float iconFillTime = 60f;

    private class StatusTimer
    {
        public Image icon;
        public float timer;
        public StatusTimer(Image icon)
        {
            this.icon = icon;
            this.timer = 0f;
        }
    }

    private List<StatusTimer> activeStatusTimers = new List<StatusTimer>();

    void Start()
    {
        textRectTransform = statTextIntro.GetComponent<RectTransform>();
        parentRectTransform = statTextIntro.transform.parent.GetComponent<RectTransform>();

        InitIcon(defenceIcon);
        InitIcon(regenIcon);
        InitIcon(powerIcon);

        UpdateStatUI();
        SetupScrolling();
    }

void Update()
{
    // --- Testing hotkeys ---
    if (Input.GetKeyDown(KeyCode.D))  ActivateDefence();
    if (Input.GetKeyDown(KeyCode.R))  ActivateRegen();
    if (Input.GetKeyDown(KeyCode.P))  ActivatePower();

    // --- Update active icons ---
    if (activeStatusTimers.Count > 0)
    {
        HandleStatusIcons();
        statTextIntro.text = "";
    }
    else
    {
        if (string.IsNullOrEmpty(statTextIntro.text))
        {
            statTextIntro.text = lastDisplayText;
            SetupScrolling();
        }

        UpdateStatUI();

        if (enableScroll)
            ScrollText();
    }
}


    void UpdateStatUI()
    {
        if (hpBar != null && hpBar.isInDanger())
        {
            newDisplayText = "DANGER! DANGER!";
            statTextIntro.color = dangerColor;
        }
        else
        {
            newDisplayText = "STATUS BOARD";
            statTextIntro.color = normalColor;
        }

        if (newDisplayText != lastDisplayText)
        {
            displayText = newDisplayText;
            statTextIntro.text = displayText;
            SetupScrolling();
            lastDisplayText = newDisplayText;
        }
    }

    void SetupScrolling()
    {
        statTextIntro.ForceMeshUpdate();
        textWidth = statTextIntro.textBounds.size.x;
        parentWidth = parentRectTransform.rect.width;

        float startOffset = 100f;
        textRectTransform.anchoredPosition = new Vector2(parentWidth - startOffset, textRectTransform.anchoredPosition.y);
    }

    void ScrollText()
    {
        Vector2 currentPos = textRectTransform.anchoredPosition;
        currentPos.x -= scrollSpeed * Time.deltaTime;

        if (currentPos.x < -textWidth * 0.5f)
        {
            currentPos.x = parentWidth;
        }

        textRectTransform.anchoredPosition = currentPos;
    }

    void InitIcon(Image icon)
    {
        if (icon != null)
            icon.fillAmount = 0f;
    }

    void HandleStatusIcons()
    {
        List<StatusTimer> finishedTimers = new List<StatusTimer>();

        foreach (var timer in activeStatusTimers)
        {
            timer.timer += Time.deltaTime;
            float progress = Mathf.Clamp01(timer.timer / iconFillTime);
            timer.icon.fillAmount = 1f - progress;

            if (progress >= 1f)
                finishedTimers.Add(timer);
        }

        foreach (var timer in finishedTimers)
        {
            timer.icon.fillAmount = 0f;
            activeStatusTimers.Remove(timer);
        }
    }

    public void ActivateDefence() => ActivateStatus(defenceIcon);
    public void ActivateRegen() => ActivateStatus(regenIcon);
    public void ActivatePower() => ActivateStatus(powerIcon);

    private void ActivateStatus(Image icon)
    {
        if (icon == null) return;

        icon.fillAmount = 1f;

        activeStatusTimers.Add(new StatusTimer(icon));
    }
}

