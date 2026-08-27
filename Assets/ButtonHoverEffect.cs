using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;

public class ButtonHoverEffect : MonoBehaviour
{
    public Color normalColor = Color.white;
    public Color hoverColor = new Color(1f, 0.5f, 1f, 1f);

    [Header("色差設定")]
    public float chromaOffsetX = 3f;
    public Color chromaColorR = new Color(1f, 0f, 0f, 0.5f);
    public Color chromaColorB = new Color(0f, 0.5f, 1f, 0.5f);

    void Start()
    {
        Button[] buttons = GetComponentsInChildren<Button>();

        foreach (Button btn in buttons)
        {
            TextMeshProUGUI tmp = btn.GetComponentInChildren<TextMeshProUGUI>();
            if (tmp == null) continue;

            EventTrigger trigger = btn.gameObject.GetComponent<EventTrigger>();
            if (trigger == null)
                trigger = btn.gameObject.AddComponent<EventTrigger>();

            trigger.triggers.Clear();

            TextMeshProUGUI capturedTmp = tmp;

            EventTrigger.Entry enter = new EventTrigger.Entry();
            enter.eventID = EventTriggerType.PointerEnter;
            enter.callback.AddListener((data) =>
            {
                capturedTmp.color = hoverColor;
                AddChromaLayers(capturedTmp);
            });
            trigger.triggers.Add(enter);

            EventTrigger.Entry exit = new EventTrigger.Entry();
            exit.eventID = EventTriggerType.PointerExit;
            exit.callback.AddListener((data) =>
            {
                capturedTmp.color = normalColor;
                RemoveChromaLayers(capturedTmp);
            });
            trigger.triggers.Add(exit);
        }
    }

    void AddChromaLayers(TextMeshProUGUI tmp)
    {
        // 避免重複建立
        RemoveChromaLayers(tmp);

        CreateChromaLayer(tmp, "ChromaR", chromaColorR, -chromaOffsetX);
        CreateChromaLayer(tmp, "ChromaB", chromaColorB, chromaOffsetX);
    }

    void RemoveChromaLayers(TextMeshProUGUI tmp)
    {
        Transform parent = tmp.transform.parent;
        if (parent == null) return;

        Transform r = parent.Find("ChromaR");
        Transform b = parent.Find("ChromaB");

        if (r != null) Destroy(r.gameObject);
        if (b != null) Destroy(b.gameObject);
    }

    void CreateChromaLayer(TextMeshProUGUI source, string layerName, Color color, float xOffset)
    {
        GameObject obj = new GameObject(layerName);
        obj.transform.SetParent(source.transform.parent, false);
        obj.transform.SetSiblingIndex(source.transform.GetSiblingIndex());

        RectTransform rect = obj.AddComponent<RectTransform>();
        RectTransform srcRect = source.GetComponent<RectTransform>();

        rect.anchorMin = srcRect.anchorMin;
        rect.anchorMax = srcRect.anchorMax;
        rect.pivot = srcRect.pivot;
        rect.anchoredPosition = srcRect.anchoredPosition + new Vector2(xOffset, 0);
        rect.sizeDelta = srcRect.sizeDelta;

        TextMeshProUGUI tmp = obj.AddComponent<TextMeshProUGUI>();
        tmp.text = source.text;
        tmp.font = source.font;
        tmp.fontSize = source.fontSize;
        tmp.fontStyle = source.fontStyle;
        tmp.alignment = source.alignment;
        tmp.color = color;
    }
}