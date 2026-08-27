using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.InputSystem;
using UnityEngine.EventSystems;

public class ThoughtBubbleSystem : MonoBehaviour
{
    [Header("泡泡底圖")]
    public Sprite[] largeBubbles;
    public Sprite[] smallBubbles;
    public Sprite dotSprite;

    [Header("泡泡大小")]
    public Vector2 largeBubbleSize = new Vector2(300f, 150f);
    public Vector2 smallBubbleSize = new Vector2(180f, 100f);
    public Vector2 dotSize = new Vector2(50f, 50f);
    public int textLengthThreshold = 20;

    [Header("位置設定")]
    public float headOffsetY = 50f;
    public float minX = 400f;
    public float maxX = 450f;
    public float midMinX = 500f;
    public float midMaxX = 600f;
    public float randomMinX = 200f;
    public float randomMaxX = 600f;
    public float topY = 180f;
    public float midY = -200f;
    public float botY = -550f;
    public float randomMinY = -600f;
    public float randomMaxY = 200f;

    [Header("點點設定")]
    public Vector2 dotAnchorOffset = new Vector2(0f, -0.5f);
    public float dotUpperOffsetY = 0.5f;
    public float verticalThreshold = 250f;
    public float midThreshold = 150f;

    [Header("文字設定")]
    public TMP_FontAsset font;
    public float fontSize = 10f;

    [Header("玩家 & 鏡頭")]
    public Transform playerTransform;
    public Camera mainCamera;

    [Header("生成設定")]
    public float initialInterval = 3f;
    public float minInterval = 0.5f;
    public float intervalDecreaseRate = 0.5f;
    public int maxBubbles = 12;
    public float bubbleClickDelay = 1f;

    [Header("ENTITY 對話")]
    public TextMeshProUGUI entityDialogueText;
    public DialogueTrigger dialogueTrigger;

    [Header("最終爆發設定")]
    public int burstCols = 6;
    public int burstRows = 3;
    public float burstColSpacing = 200f;
    public float burstRowSpacing = 350f;
    public float burstDiagonalOffset = 120f;
    public float burstBaseOffsetY = 300f;
    public float burstSpawnInterval = 0.05f;

    private List<GameObject> activeBubbles = new List<GameObject>();
    private float currentInterval;
    private bool isRunning = false;
    private Canvas parentCanvas;
    private int thoughtIndex = 0;
    private List<int> remainingIndices = new List<int>();
    public System.Action onAllCleared;
    public System.Action onFailed;
    public System.Action onEntityDone;
    private int dotBubbleCount = 0;
    private int orderedIndex = 0;
    private List<int> shuffledCreepy = new List<int>();

    private string[] thoughts = {
        "did i lock the door?",
        "i should reply that message",
        "what's for dinner?",
        "i'm so tired",
        "work tomorrow",
        "i need to do laundry",
        "i forgot to water the plants",
        "did i turn off the stove?",
        "i need to call mom",
        "did i pay that bill?",
        "i should exercise more",
        "i forgot to reply that email",
        "what time is it?",
        "i need a haircut",
        "i should drink more water",
        "did i feed the cat?",
        "i need to clean my room",
        "i should go to bed earlier",
        "what am i doing this weekend?",
        "i need to buy groceries",
        "did i save that file?",
        "i should text them back",
        "my back hurts",
        "i need coffee",
        "did i set my alarm?",
        "i should stretch more",
        "i forgot what i was going to say",
        "i need to do the dishes",
        "i should read more",
        "did i lock my car?",
        "why am i doing this?",
        "is this actually working?",
        "i can't stop thinking",
        "what if i can't relax?",
        "am i breathing right?",
        "how long have i been sitting here?",
        "is this making anything better?",
        "what am i supposed to feel?",
        "i don't think i'm doing this right",
        "maybe i should just stop",
        "am i wasting my time?",
        "why can't i just be still?",
        "i keep losing focus",
        "what if this doesn't work?",
        "am i doing enough?",
        "is anyone watching?",
        "why does this feel strange?",
        "i don't think i belong here",
        "something feels off",
        "i can't remember why i started",
        "you've been here a while",
        "are you actually relaxing?",
        "or just pretending?",
        "i can see you",
        "you can't escape your thoughts",
        "i've been waiting",
        "you let me in",
        "you can't quiet your mind",
        "you've been sitting very still",
        "i've been watching",
        "you opened the door",
        "did you think i wouldn't notice?",
        "you're still here",
        "i know what you're thinking",
        "you can feel it too",
        "you've been here before",
        "you invited me in",
        "there's no going back now",
        "you already know",
        "we've been here before",
        "you can't leave",
        "i've always been here",
        "you brought me with you",
        "i remember everything",
        "you do too",
        "this is where you belong",
        "you feel it don't you",
        "i've been here the whole time",
        "you're not alone",
        "stay.",
    };

    void Start()
    {
        parentCanvas = GetComponentInParent<Canvas>();
        ShuffleCreepy();
    }

    public void StartThoughts()
    {
        isRunning = true;
        currentInterval = initialInterval;
        orderedIndex = 0;
        dotBubbleCount = 0;
        thoughtIndex = 0;
        ShuffleCreepy();
        StartCoroutine(SpawnThoughts());
    }

    public void StopThoughts()
    {
        isRunning = false;
        StopAllCoroutines();
        foreach (var b in activeBubbles)
            if (b != null) Destroy(b);
        activeBubbles.Clear();
    }

    void ShuffleThoughts()
    {
        remainingIndices.Clear();
        for (int i = 0; i < thoughts.Length; i++)
            remainingIndices.Add(i);

        for (int i = remainingIndices.Count - 1; i > 0; i--)
        {
            int j = Random.Range(0, i + 1);
            int temp = remainingIndices[i];
            remainingIndices[i] = remainingIndices[j];
            remainingIndices[j] = temp;
        }
    }

    int GetNextThoughtIndex()
    {
        if (remainingIndices.Count == 0)
            ShuffleThoughts();

        int index = remainingIndices[0];
        remainingIndices.RemoveAt(0);
        return index;
    }

    void ShuffleCreepy()
    {
        shuffledCreepy.Clear();
        for (int i = 50; i < thoughts.Length; i++)
            shuffledCreepy.Add(i);

        for (int i = shuffledCreepy.Count - 1; i > 0; i--)
        {
            int j = Random.Range(0, i + 1);
            int temp = shuffledCreepy[i];
            shuffledCreepy[i] = shuffledCreepy[j];
            shuffledCreepy[j] = temp;
        }
    }

    string GetNextThought()
    {
        if (orderedIndex < 50)
        {
            return thoughts[orderedIndex++];
        }
        else
        {
            if (shuffledCreepy.Count == 0)
                ShuffleCreepy();

            int index = shuffledCreepy[0];
            shuffledCreepy.RemoveAt(0);
            return thoughts[index];
        }
    }

    IEnumerator SpawnThoughts()
    {
        while (isRunning)
        {
            yield return new WaitForSeconds(currentInterval);
            if (!isRunning) yield break;

            if (activeBubbles.Count >= maxBubbles)
            {
                StartCoroutine(EntityTakeover());
                yield break;
            }

            if (activeBubbles.Count >= 35)
            {
                StartCoroutine(FinalBurst());
                yield break;
            }

            SpawnBubble();
            currentInterval = Mathf.Max(minInterval, currentInterval - intervalDecreaseRate);
        }
    }

    Vector2 GetBubbleOffset(int index)
    {
        float leftX = -Random.Range(minX, maxX);
        float rightX = Random.Range(minX, maxX);

        switch (index)
        {
            case 0: return new Vector2(leftX, topY);
            case 1: return new Vector2(rightX, topY);
            case 2: return new Vector2(-Random.Range(midMinX, midMaxX), midY);
            case 3: return new Vector2(Random.Range(midMinX, midMaxX), midY);
            case 4: return new Vector2(leftX, botY);
            case 5: return new Vector2(Random.Range(midMinX, midMaxX), botY);
            default:
                float side = (index % 2 == 0) ? 1f : -1f;
                return new Vector2(
                    side * Random.Range(randomMinX, randomMaxX),
                    Random.Range(randomMinY, randomMaxY));
        }
    }

    void SpawnBubble()
    {
        string thought = GetNextThought();

        Sprite selectedSprite;
        Vector2 selectedSize;

        if (thought.Length > textLengthThreshold)
        {
            selectedSprite = largeBubbles[Random.Range(0, largeBubbles.Length)];
            selectedSize = largeBubbleSize;
        }
        else
        {
            selectedSprite = smallBubbles[Random.Range(0, smallBubbles.Length)];
            selectedSize = smallBubbleSize;
        }

        Vector2 screenPos = mainCamera.WorldToScreenPoint(playerTransform.position);
        RectTransform canvasRect = parentCanvas.GetComponent<RectTransform>();
        Vector2 localPos;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            canvasRect, screenPos, parentCanvas.worldCamera, out localPos);
        Vector2 headPos = localPos + new Vector2(0, headOffsetY);
        Vector2 offset = GetBubbleOffset(thoughtIndex);

        // 建立泡泡
        GameObject bubble = new GameObject("ThoughtBubble");
        bubble.transform.SetParent(transform, false);

        Image img = bubble.AddComponent<Image>();
        img.sprite = selectedSprite;
        img.color = activeBubbles.Count >= 30
            ? new Color(1f, 0.3f, 0.3f, 1f)
            : Color.white;

        RectTransform rect = bubble.GetComponent<RectTransform>();
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.sizeDelta = selectedSize;
        rect.anchoredPosition = headPos + offset;

        // 建立點點
        if (dotSprite != null && dotBubbleCount < 6)
        {
            dotBubbleCount++;
            GameObject dotsObj = new GameObject("Dots");
            dotsObj.transform.SetParent(bubble.transform, false);

            Image dotsImg = dotsObj.AddComponent<Image>();
            dotsImg.sprite = dotSprite;

            RectTransform dotsRect = dotsObj.GetComponent<RectTransform>();
            dotsRect.sizeDelta = dotSize;
            dotsRect.pivot = new Vector2(0.5f, 0.5f);

            if (offset.x < -50f)
            {
                if (offset.y < verticalThreshold)
                {
                    dotsRect.anchoredPosition = new Vector2(selectedSize.x * 0.4f, selectedSize.y * dotUpperOffsetY);
                    dotsRect.localEulerAngles = new Vector3(0, 0, 0f);
                    dotsRect.localScale = new Vector3(-1f, -1f, 1f);
                }
                else if (offset.y < midThreshold)
                {
                    dotsRect.anchoredPosition = new Vector2(selectedSize.x * 0.6f, 0f);
                    dotsRect.localEulerAngles = new Vector3(0, 0, -30f);
                    dotsRect.localScale = new Vector3(-1f, -1f, 1f);
                }
                else
                {
                    dotsRect.anchoredPosition = new Vector2(selectedSize.x * 0.3f, selectedSize.y * dotAnchorOffset.y);
                    dotsRect.localEulerAngles = new Vector3(0, 0, 0f);
                    dotsRect.localScale = new Vector3(-1f, 1f, 1f);
                }
            }
            else if (offset.x > 50f)
            {
                if (offset.y < verticalThreshold)
                {
                    dotsRect.anchoredPosition = new Vector2(selectedSize.x * -0.5f, selectedSize.y * 0.3f);
                    dotsRect.localEulerAngles = new Vector3(0, 0, 0f);
                    dotsRect.localScale = new Vector3(1f, -1f, 1f);
                }
                else if (offset.y < midThreshold)
                {
                    dotsRect.anchoredPosition = new Vector2(selectedSize.x * -0.6f, 0f);
                    dotsRect.localEulerAngles = new Vector3(0, 0, 30f);
                    dotsRect.localScale = new Vector3(1f, -1f, 1f);
                }
                else
                {
                    dotsRect.anchoredPosition = new Vector2(selectedSize.x * -0.3f, selectedSize.y * dotAnchorOffset.y);
                    dotsRect.localEulerAngles = new Vector3(0, 0, 180f);
                    dotsRect.localScale = new Vector3(-1f, -1f, 1f);
                }
            }
            else
            {
                dotsRect.anchoredPosition = new Vector2(0f, selectedSize.y * dotAnchorOffset.y);
                dotsRect.localEulerAngles = new Vector3(0, 0, 0f);
                dotsRect.localScale = Vector3.one;
            }
        }

        // 建立文字
        GameObject textObj = new GameObject("Text");
        textObj.transform.SetParent(bubble.transform, false);
        TextMeshProUGUI tmp = textObj.AddComponent<TextMeshProUGUI>();
        tmp.text = thought;
        tmp.fontSize = fontSize;
        tmp.color = new Color(0.2f, 0.2f, 0.2f);
        tmp.alignment = TextAlignmentOptions.Center;
        tmp.verticalAlignment = VerticalAlignmentOptions.Middle;
        if (font != null) tmp.font = font;

        RectTransform textRect = textObj.GetComponent<RectTransform>();
        textRect.anchorMin = Vector2.zero;
        textRect.anchorMax = Vector2.one;
        textRect.pivot = new Vector2(0.5f, 0.5f);
        textRect.offsetMin = new Vector2(15f, 15f);
        textRect.offsetMax = new Vector2(-15f, -15f);

        // ── 用 EventTrigger 取代 Button，避免 disabled 顏色污染 ──
        float spawnTime = Time.time;
        GameObject capturedBubble = bubble;

        EventTrigger trigger = bubble.AddComponent<EventTrigger>();

        // 點擊清除（有點擊延遲保護）
        EventTrigger.Entry clickEntry = new EventTrigger.Entry();
        clickEntry.eventID = EventTriggerType.PointerClick;
        clickEntry.callback.AddListener((data) =>
        {
            if (Time.time - spawnTime < bubbleClickDelay) return;
            RemoveBubble(capturedBubble);
            CursorManager.Instance?.SetDefault();
        });
        trigger.triggers.Add(clickEntry);

        // Hover 游標
        EventTrigger.Entry enterEntry = new EventTrigger.Entry();
        enterEntry.eventID = EventTriggerType.PointerEnter;
        enterEntry.callback.AddListener((data) => CursorManager.Instance?.SetHover());
        trigger.triggers.Add(enterEntry);

        EventTrigger.Entry exitEntry = new EventTrigger.Entry();
        exitEntry.eventID = EventTriggerType.PointerExit;
        exitEntry.callback.AddListener((data) => CursorManager.Instance?.SetDefault());
        trigger.triggers.Add(exitEntry);

        // 需要 Raycast 才能偵測點擊，確保 Image 的 Raycast Target 是開的
        img.raycastTarget = true;

        activeBubbles.Add(bubble);
        thoughtIndex++;
    }

    void RemoveBubble(GameObject bubble)
    {
        activeBubbles.Remove(bubble);
        Destroy(bubble);

        if (activeBubbles.Count == 0 && thoughtIndex >= 6)
        {
            isRunning = false;
            onAllCleared?.Invoke();
        }
    }

    IEnumerator FinalBurst()
    {
        isRunning = false;

        RectTransform canvasRect = parentCanvas.GetComponent<RectTransform>();

        // 改這兩行
        float canvasW = canvasRect.rect.width;
        float canvasH = canvasRect.rect.height;
        Vector2 screenBase = new Vector2(-canvasW * 0.5f, -canvasH * 0.5f);

        string[] creepyLines = {
        "you let me in", "i've been waiting", "you can't escape",
        "i know you", "stay.", "you're mine now",
        "i've always been here", "you can't leave", "you opened the door",
    };

        int midRow = burstRows / 2;
        yield return StartCoroutine(SpawnRow(midRow, canvasW, canvasH, screenBase, creepyLines));

        for (int spread = 1; spread <= burstRows; spread++)
        {
            int upperRow = midRow + spread;
            int lowerRow = midRow - spread;

            Coroutine upper = null;
            Coroutine lower = null;

            if (upperRow < burstRows)
                upper = StartCoroutine(SpawnRow(upperRow, canvasW, canvasH, screenBase, creepyLines));
            if (lowerRow >= 0)
                lower = StartCoroutine(SpawnRow(lowerRow, canvasW, canvasH, screenBase, creepyLines));

            if (upper != null) yield return upper;
            if (lower != null) yield return lower;
        }

        yield return new WaitForSeconds(0.5f);
        StartCoroutine(EntityTakeover());
    }

    IEnumerator SpawnRow(int row, float canvasW, float canvasH, Vector2 screenBase, string[] creepyLines)
    {
        for (int col = 0; col < burstCols; col++)
        {
            float x = screenBase.x + (col * burstColSpacing) + 100f;
            float y = screenBase.y + (row * burstRowSpacing) + (col * burstDiagonalOffset) + burstBaseOffsetY;
            SpawnBurstBubble(x, y, creepyLines[(row * burstCols + col) % creepyLines.Length]);
            yield return new WaitForSeconds(burstSpawnInterval);
        }
    }

    void SpawnBurstBubble(float x, float y, string text)
    {
        GameObject bubble = new GameObject("BurstBubble");
        bubble.transform.SetParent(transform, false);

        Image img = bubble.AddComponent<Image>();
        img.sprite = largeBubbles[Random.Range(0, largeBubbles.Length)];
        img.color = new Color(1f, 0.2f, 0.2f, 0.9f);
        img.raycastTarget = false; // 爆發泡泡不需要點擊

        RectTransform rect = bubble.GetComponent<RectTransform>();
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.sizeDelta = largeBubbleSize;
        rect.anchoredPosition = new Vector2(x, y);

        GameObject textObj = new GameObject("Text");
        textObj.transform.SetParent(bubble.transform, false);
        TextMeshProUGUI tmp = textObj.AddComponent<TextMeshProUGUI>();
        tmp.text = text;
        tmp.color = new Color(0f, 0f, 0f);
        tmp.fontSize = fontSize;
        tmp.alignment = TextAlignmentOptions.Center;
        tmp.verticalAlignment = VerticalAlignmentOptions.Middle;
        if (font != null) tmp.font = font;

        RectTransform textRect = textObj.GetComponent<RectTransform>();
        textRect.anchorMin = Vector2.zero;
        textRect.anchorMax = Vector2.one;
        textRect.offsetMin = new Vector2(15f, 15f);
        textRect.offsetMax = new Vector2(-15f, -15f);

        activeBubbles.Add(bubble);
    }

    IEnumerator EntityTakeover()
    {
        isRunning = false;
        onFailed?.Invoke();

        foreach (var b in activeBubbles)
            if (b != null)
                StartCoroutine(ShakeThenDestroy(b));

        yield return new WaitForSeconds(1f);
        activeBubbles.Clear();

        if (entityDialogueText != null)
        {
            entityDialogueText.gameObject.SetActive(true);
            string[] lines = {
                "you can't quiet your mind.",
                "that's why you need me.",
                "let me think for you."
            };

            foreach (string line in lines)
            {
                entityDialogueText.text = "";
                foreach (char c in line)
                {
                    entityDialogueText.text += c;
                    yield return new WaitForSeconds(0.05f);
                }
                yield return new WaitForSeconds(2f);
                entityDialogueText.text = "";
            }

            entityDialogueText.gameObject.SetActive(false);
        }

        if (dialogueTrigger != null)
            dialogueTrigger.ForceTriggered();

        onEntityDone?.Invoke();
    }

    IEnumerator ShakeThenDestroy(GameObject obj)
    {
        if (obj == null) yield break;
        RectTransform rect = obj.GetComponent<RectTransform>();
        Vector2 originalPos = rect.anchoredPosition;

        for (int i = 0; i < 10; i++)
        {
            if (obj == null) yield break;
            rect.anchoredPosition = originalPos + new Vector2(
                Random.Range(-5f, 5f),
                Random.Range(-5f, 5f));
            yield return new WaitForSeconds(0.05f);
        }

        if (obj != null) Destroy(obj);
    }
}