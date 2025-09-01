using UnityEngine;
using TMPro;

public class statusBar : MonoBehaviour
{
    public TMP_Text statTextIntro;
    public bool enableScroll = true;
    public float scrollSpeed = 50f;
    public healthBar hpBar;

    private RectTransform textRectTransform;
    private RectTransform parentRectTransform;
    private float textWidth;
    private float parentWidth;
    private string displayText;
    private string lastDisplayText;
    private string newDisplayText;
    private Color dangerColor = Color.red;
    private Color normalColor = Color.white;    

    void Start()
    {
        textRectTransform = statTextIntro.GetComponent<RectTransform>();
        parentRectTransform = statTextIntro.transform.parent.GetComponent<RectTransform>();

        UpdateStatUI();
        SetupScrolling();
    }

    void Update()
    {
        UpdateStatUI();
        if (enableScroll)
        {
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

        if (currentPos.x < -textWidth * 0.6)
        {
            currentPos.x = parentWidth;
        }

        textRectTransform.anchoredPosition = currentPos;
    }
}
