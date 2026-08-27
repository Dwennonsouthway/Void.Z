using UnityEngine;
using TMPro;
using System.Collections;

[RequireComponent(typeof(TextMeshProUGUI))]
public class MarqueeText : MonoBehaviour
{
    [Header("跑馬燈設定")]
    public float scrollSpeed = 50f;       // 滾動速度（像素/秒）
    public float pauseDuration = 2f;      // 開始滾動前停頓幾秒
    public float resetPauseDuration = 1f; // 滾完回到開頭前停頓幾秒
    public float overflowThreshold = 5f;  // 超出幾像素才觸發跑馬燈

    private TextMeshProUGUI tmp;
    private RectTransform rectTransform;
    private RectTransform parentRect;
    private Coroutine marqueeCoroutine;
    private string currentText = "";
    private Vector2 originalPosition;

    void Awake()
    {
        tmp = GetComponent<TextMeshProUGUI>();
        rectTransform = GetComponent<RectTransform>();
        parentRect = transform.parent?.GetComponent<RectTransform>();
        originalPosition = rectTransform.anchoredPosition; // 記錄你在 Inspector 設定的位置
    }

    public void SetText(string text)
    {
        if (currentText == text) return;
        currentText = text;
        tmp.text = text;

        if (marqueeCoroutine != null)
        {
            StopCoroutine(marqueeCoroutine);
            marqueeCoroutine = null;
        }

        // 改成用 originalPosition 而不是 Vector2.zero
        rectTransform.anchoredPosition = originalPosition;

        StartCoroutine(CheckOverflowNextFrame());
    }

    IEnumerator CheckOverflowNextFrame()
    {
        yield return null; // 等 TMP 更新 preferredWidth

        float textWidth = tmp.preferredWidth;
        float containerWidth = parentRect != null
            ? parentRect.rect.width
            : rectTransform.rect.width;

        if (textWidth > containerWidth + overflowThreshold)
        {
            marqueeCoroutine = StartCoroutine(MarqueeLoop(textWidth, containerWidth));
        }
    }

    IEnumerator MarqueeLoop(float textWidth, float containerWidth)
    {
        float scrollDistance = textWidth - containerWidth;

        while (true)
        {
            // 停在原始位置
            rectTransform.anchoredPosition = originalPosition;
            yield return new WaitForSeconds(pauseDuration);

            float elapsed = 0f;
            float duration = scrollDistance / scrollSpeed;

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float x = Mathf.Lerp(originalPosition.x, originalPosition.x - scrollDistance, elapsed / duration);
                rectTransform.anchoredPosition = new Vector2(x, originalPosition.y);
                yield return null;
            }

            rectTransform.anchoredPosition = new Vector2(originalPosition.x - scrollDistance, originalPosition.y);
            yield return new WaitForSeconds(resetPauseDuration);
        }
    }


    void OnDisable()
    {
        if (marqueeCoroutine != null)
        {
            StopCoroutine(marqueeCoroutine);
            marqueeCoroutine = null;
        }
        // 改成用 originalPosition
        rectTransform.anchoredPosition = originalPosition;
    }
}