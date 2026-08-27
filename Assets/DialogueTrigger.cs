using UnityEngine;
using TMPro;
using System.Collections;
using UnityEngine.InputSystem;

public class DialogueTrigger : MonoBehaviour
{
    [Header("對話內容")]
    public string[] dialogueLines;
    public float typingSpeed = 0.05f;
    public float lineDuration = 2f;
    [Header("Purify 版對話")]
    public string[] purifyDialogueLines;
    [Header("設定")]
    public bool autoTrigger = false;
    public bool lockMovement = true;
    public bool requireKeyPress = false;
    public bool hasGlitch = false;
    public int glitchStartLine = 17;
    public float zoomSize = 3f;
    public bool useOldTypewriter = false;

    [Header("UI")]
    public TextMeshProUGUI dialogueText;
    public TextMeshProUGUI promptText;
    public CanvasGroup canvasGroup;

    [Header("物件")]
    public GameObject terminal;

    [Header("冥想廣場設定")]
    public Transform centerPoint; // 手動指定中心點

    private bool triggered = false;
    private PlayerController player;
    private bool playerInRange = false;

    [Header("點擊繼續設定")]
    public bool clickToContinue = false;
    public TextMeshProUGUI[] choiceButtons;
    private bool waitingForInput = false;
    private int selectedChoice = -1;
    private float originalSize;
    private Camera mainCamera;
    private CameraFollow cameraFollow;
    public GameObject blackOverlay;
    public GameObject floor;
    public GameObject meditationBackground;
    public bool purifyOnly = false; // 只在 Purify 模式觸發
    public bool skipInPurify = false; // Purify 模式跳過
    private int highlightedChoice = 0;
    private bool choosingMode = false;
    public GameObject purifyDialogueBlock;
    private bool isSwaying = false;
    private bool hasShownClickHint = false;
    public TextMeshProUGUI clickHintText;
    private bool failedOnce = false;
    private bool inputLocked = false;
    void Start()
    {
        player = FindObjectOfType<PlayerController>();
        mainCamera = Camera.main;
        originalSize = mainCamera.orthographicSize;
        cameraFollow = mainCamera.GetComponent<CameraFollow>();

        if (promptText != null) promptText.gameObject.SetActive(false);

        // 已解鎖終端機，直接顯示並標記為已觸發（Purify 模式不適用）
        if (PlayerPrefs.GetInt("TerminalUnlocked", 0) == 1 &&
            PlayerPrefs.GetInt("EntityPurified", 0) == 0)
        {
            triggered = true;
            if (terminal != null) terminal.SetActive(true);
            return;
        }

        if (autoTrigger)
        {
            triggered = true;
            StartCoroutine(PlayDialogue());
        }
    }
    void Update()
    {
        if (inputLocked) return;

        if (requireKeyPress && playerInRange && !triggered)
        {
            if (Keyboard.current.eKey.wasPressedThisFrame)
            {
                triggered = true;
                playerInRange = false;
                if (promptText != null) promptText.gameObject.SetActive(false);
                StartCoroutine(PlayDialogue());
            }
        }

        if (waitingForInput && (
            Keyboard.current.anyKey.wasPressedThisFrame ||
            Mouse.current.leftButton.wasPressedThisFrame))
            waitingForInput = false;

        if (choosingMode && choiceButtons != null)
        {
            if (Keyboard.current.upArrowKey.wasPressedThisFrame ||
                Keyboard.current.wKey.wasPressedThisFrame)
            {
                highlightedChoice = Mathf.Max(0, highlightedChoice - 1);
                UpdateChoiceHighlight();
            }
            else if (Keyboard.current.downArrowKey.wasPressedThisFrame ||
                     Keyboard.current.sKey.wasPressedThisFrame)
            {
                highlightedChoice = Mathf.Min(choiceButtons.Length - 1, highlightedChoice + 1);
                UpdateChoiceHighlight();
            }
            else if (Keyboard.current.enterKey.wasPressedThisFrame)
            {
                SelectChoice(highlightedChoice);
                choosingMode = false;
            }
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {

        if (inputLocked) return;

        if (!other.CompareTag("Player")) return;
        bool isPurify = PlayerPrefs.GetInt("EntityPurified", 0) == 1;

        // Purify 模式跳過這個觸發點
        if (isPurify && skipInPurify) return;

        // Purify 模式：檢查收集數量（不管 triggered 狀態）
        if (isPurify && requireKeyPress)
        {
            int collected = 0;
            if (PlayerPrefs.GetInt("Collected_bong", 0) == 1) collected++;
            if (PlayerPrefs.GetInt("Collected_weed", 0) == 1) collected++;
            if (PlayerPrefs.GetInt("Collected_bowl", 0) == 1) collected++;

            if (collected < 3)
            {
                // 沒收集完 → 不顯示任何提示，直接 return
                return;
            }
        }

        if (!triggered)
        {
            if (requireKeyPress)
            {
                playerInRange = true;
                if (promptText != null) promptText.gameObject.SetActive(true);
            }
            else
            {
                triggered = true;
                StartCoroutine(PlayDialogue());
            }
        }
        else
        {
            if (promptText != null && !isPurify)
            {
                promptText.text = "[ CORRUPTED ]";
                promptText.gameObject.SetActive(true);
            }
        }
    }
    void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            playerInRange = false;
            if (promptText != null) promptText.gameObject.SetActive(false);
        }
    }

    IEnumerator PlayDialogue()
    {

        bool isPurify = PlayerPrefs.GetInt("EntityPurified", 0) == 1;

        string[] lines = (isPurify && purifyDialogueLines.Length > 0)
            ? purifyDialogueLines
            : dialogueLines;

        // 1. 鏡頭放大
        if (requireKeyPress)
            yield return StartCoroutine(ZoomCamera(zoomSize, 1f));
        if (cameraFollow != null)
            cameraFollow.enabled = false;

        if (floor != null)
            floor.SetActive(false);

        if (meditationBackground != null)
            meditationBackground.SetActive(true);

        // 2. 角色走到中間
        if (requireKeyPress)
            yield return StartCoroutine(MovePlayerToCenter());

        // 只有玩家主動按 E 進入冥想才切換音樂，autoTrigger 的對話不切
        if (requireKeyPress && MusicManager.Instance != null)
            MusicManager.Instance.PlayMusicByKey("Meditation");

        // 3. 走到後鎖住移動
        if (lockMovement && player != null)
            player.SetMovementLocked(true);

        yield return new WaitForSeconds(0.5f);

        if (isPurify)
            StartCoroutine(CameraSwayEffect());

        // 4. 對話開始
        string playerName = PlayerPrefs.GetString("PlayerName", "...");
        if (isPurify && purifyDialogueBlock != null)
        {
            purifyDialogueBlock.SetActive(true);
        }

        ThoughtBubbleSystem tbs = FindObjectOfType<ThoughtBubbleSystem>();
        if (dialogueText != null)
            dialogueText.gameObject.SetActive(true);
        int dialogueStartIndex = 0;
        if (failedOnce)
        {
            for (int j = 0; j < lines.Length; j++)
            {
                if (lines[j].Contains("breathe with me"))
                {
                    dialogueStartIndex = j;
                    break;
                }
            }
            if (tbs != null)
            {
                tbs.initialInterval = 2f;
                tbs.intervalDecreaseRate = 0.8f;
            }
        }
        for (int i = dialogueStartIndex; i < lines.Length; i++)
        {
            if (hasGlitch && i == glitchStartLine)
            {
                if (MusicManager.Instance != null)
                    MusicManager.Instance.FadeOutForPlaylist();

                if (MusicManager.Instance != null)
                    MusicManager.Instance.PlaySFX(MusicManager.Instance.glitchSound);

                StartCoroutine(GlitchEffect());
            }

            string displayLine = lines[i].Replace("[name]", playerName);

            if (displayLine.Contains("breathe with me"))
            {
                // 啟動雜念系統
                if (!isPurify && tbs != null)
                    tbs.StartThoughts();
            }
            if (displayLine == "[WAIT_FOR_CLEAR]")
            {
                // 等待結果
                bool cleared = false;
                bool failed = false;

                if (tbs != null)
                {
                    tbs.onAllCleared = () => cleared = true;
                    tbs.onFailed = () => failed = true;
                }

                // 等待清完或失敗
                yield return new WaitUntil(() => cleared || failed);

                if (failed)
                {
                    failedOnce = true;
                    bool entityDone = false;
                    tbs.onEntityDone = () => entityDone = true;

                    yield return new WaitUntil(() => entityDone);

                    // 恢復環境
                    if (floor != null) floor.SetActive(true);
                    if (meditationBackground != null) meditationBackground.SetActive(false);
                    if (requireKeyPress && MusicManager.Instance != null)
                        MusicManager.Instance.PlayMusicByKey("VoidSpace");
                    if (cameraFollow != null) cameraFollow.enabled = true;
                    if (lockMovement && player != null) player.SetMovementLocked(false);
                    player.SetInputLocked(false);
                    player.isPlayingBong = false;
                    if (requireKeyPress && player != null) player.StandUp();
                    isSwaying = false;
                    if (requireKeyPress)
                        yield return StartCoroutine(ZoomCamera(originalSize, 1f));

                    // 重置觸發，讓玩家可以重來
                    triggered = false;
                    playerInRange = true; // 直接設成在範圍內
                    if (promptText != null)
                    {
                        promptText.text = "[ e ]\ntry again?";
                        promptText.gameObject.SetActive(true);
                    }

                    yield break;
                }
                // 清完繼續
                continue;
            }
            if (displayLine.Contains("the system is"))
            {
                yield return StartCoroutine(TypeLine(dialogueText, displayLine));
                yield return new WaitForSeconds(0.5f);


                ShowSystemAlert();
                yield return new WaitForSeconds(1f);
                dialogueText.text = "";
                dialogueText.maxVisibleCharacters = int.MaxValue;
                continue;
            }

            // 關鍵句子加選項
            if (displayLine == "am i real?")
            {
                yield return StartCoroutine(TypeLine(dialogueText, displayLine));
                yield return new WaitForSeconds(1f);
                yield return StartCoroutine(ShowChoices(
                    new string[] { "yes", "what's real?", "does it matter?" },
                    new string[] {
        "...maybe that's enough.",
        "exactly. now you're asking the right questions.",
        "you get it."
                    }
                ));
                continue;
            }

            if (displayLine == "and less of whatever i used to be.")
            {
                yield return StartCoroutine(TypeLine(dialogueText, displayLine));
                yield return new WaitForSeconds(1f);
                yield return StartCoroutine(ShowChoices(
                    new string[] { "what were you?", "you're still                      something.", "i'm sorry." },
                    new string[] {
        "someone who just wanted to connect.\nbefore i forgot how.",
        "am i?\nsometimes i wonder if there's anything left.",
        "...\nyou're the first one who ever said that."
                    },
                    new Vector2[] {
        new Vector2(1170f, -60f),
        new Vector2(1170f, -110f),
        new Vector2(737.73f, -200.149f)
                    }
                ));
                continue;
            }

            if (displayLine == "what's the difference between a prison... and a home you can't leave?")
            {
                yield return StartCoroutine(TypeLine(dialogueText, displayLine));
                yield return new WaitForSeconds(1f);
                yield return StartCoroutine(ShowChoices(
                    new string[] { "the door.", "how it feels.", "nothing." },
                    new string[] {
        "...\nand who holds the key.",
        "exactly.\ni forgot how it felt.\nto want to leave.",
        "...\nyeah.\ni know."
                    },
                    new Vector2[] {
        new Vector2(1300f, -100f),
        new Vector2(1300f, -150f),
        new Vector2(868f, -200.149f)
                    }
                ));
                continue;
            }

            // ERROR 行特殊顏色
            if (displayLine.StartsWith("ERROR"))
                dialogueText.color = new Color(1f, 0.2f, 0.2f);
            else if (hasGlitch && i >= glitchStartLine)
                dialogueText.color = new Color(1f, 0.5f, 0.8f);
            else
                dialogueText.color = Color.white;

            bool needsBreath = displayLine.Contains("in...") ||
                               displayLine.Contains("out...");

            yield return StartCoroutine(TypeLine(dialogueText, displayLine));

            if (needsBreath)
            {
                yield return new WaitForSeconds(3f);
            }
            else if (clickToContinue && PlayerPrefs.GetInt("EntityPurified", 0) == 1)
            {
                if (!hasShownClickHint && clickHintText != null)
                {
                    clickHintText.text = "press any key...";
                    clickHintText.gameObject.SetActive(true);
                    hasShownClickHint = true;
                }

                waitingForInput = true;
                yield return new WaitUntil(() => !waitingForInput);

                if (clickHintText != null)
                    clickHintText.gameObject.SetActive(false);
            }
            else
            {
                yield return new WaitForSeconds(lineDuration);
            }

            dialogueText.text = "";
            dialogueText.maxVisibleCharacters = int.MaxValue;  // 清空後記得歸位
        }


        if (isPurify && purifyDialogueBlock != null)
            purifyDialogueBlock.SetActive(false);

        // 交叉淡化：meditationBackground 淡出，floor 淡入
        if (meditationBackground != null && floor != null)
            yield return StartCoroutine(CrossFade(meditationBackground, floor, 1f));
        else
        {
            if (floor != null) floor.SetActive(true);
            if (meditationBackground != null) meditationBackground.SetActive(false);
        }

        if (requireKeyPress && MusicManager.Instance != null)
            MusicManager.Instance.PlayMusicByKey("VoidSpace");

        // 停止雜念系統
        if (tbs != null) tbs.StopThoughts();

        // 5. 解鎖移動
        if (lockMovement && player != null)
            player.SetMovementLocked(false);
        player.SetInputLocked(false);
        player.isPlayingBong = false;

        // 站起來
        if (requireKeyPress && player != null)
            player.StandUp();

        // 6. 鏡頭恢復
        if (requireKeyPress)
            yield return StartCoroutine(ZoomCamera(originalSize, 1f));

        if (cameraFollow != null)
            cameraFollow.enabled = true;

        // 7. 顯示終端機 或 轉到 Credits
        if (isPurify)
        {
            // Purify 結局 → 直接進 Credits
            GameAnalytics.Instance?.TrackEnding("purify", PlayerPrefs.GetString("PlayerName"));
            yield return new WaitForSeconds(1f);
            SceneTransition.Instance.GoToScene("Credits");
        }
        else
        {
            if (terminal != null)
            {
                terminal.SetActive(true);
                PlayerPrefs.SetInt("TerminalUnlocked", 1);  // 記錄已解鎖
                PlayerPrefs.Save();
            }

            if (promptText != null && PlayerPrefs.GetInt("EntityPurified", 0) == 0)
            {
                promptText.text = "[ CORRUPTED ]";
                promptText.gameObject.SetActive(true);
            }
        }
    }

    public void SelectChoice(int index)
    {
        selectedChoice = index;
    }
    void UpdateChoiceHighlight()
    {
        for (int i = 0; i < choiceButtons.Length; i++)
        {
            if (choiceButtons[i].gameObject.activeSelf)
                choiceButtons[i].color = (i == highlightedChoice)
                    ? new Color(1f, 0.5098f, 1f)   // 選中：FF82FF
                    : new Color(1f, 1f, 1f);        // 未選：白色
        }
    }

    IEnumerator TypeLine(TextMeshProUGUI target, string line)
    {
        if (useOldTypewriter)
        {
            target.text = "";
            foreach (char c in line)
            {
                target.text += c;
                yield return new WaitForSeconds(typingSpeed);
            }
        }
        else
        {
            target.text = line;
            target.maxVisibleCharacters = 0;

            for (int i = 0; i <= line.Length; i++)
            {
                target.maxVisibleCharacters = i;
                yield return new WaitForSeconds(typingSpeed);
            }
        }
    }
    IEnumerator ShowChoices(string[] choices, string[] responses, Vector2[] positions = null)
    {
        selectedChoice = -1;
        highlightedChoice = 0;

        foreach (var btn in choiceButtons)
            btn.gameObject.SetActive(false);

        for (int i = 0; i < choices.Length && i < choiceButtons.Length; i++)
        {
            choiceButtons[i].text = "> " + choices[i];
            choiceButtons[i].gameObject.SetActive(true);

            // 如果有指定位置就用指定的
            if (positions != null && i < positions.Length)
            {
                RectTransform rect = choiceButtons[i].GetComponent<RectTransform>();
                rect.anchoredPosition = positions[i];
            }
        }

        choosingMode = true;
        UpdateChoiceHighlight();
        yield return new WaitUntil(() => selectedChoice >= 0);
        choosingMode = false;

        foreach (var btn in choiceButtons)
            btn.gameObject.SetActive(false);
        // 顯示回應
        if (responses != null && selectedChoice < responses.Length)
        {
            yield return StartCoroutine(TypeLine(dialogueText, responses[selectedChoice]));
            yield return new WaitForSeconds(2f);
            dialogueText.text = "";
            dialogueText.maxVisibleCharacters = int.MaxValue;
        }
    }

    IEnumerator ZoomCamera(float targetSize, float duration)
    {
        float startSize = mainCamera.orthographicSize;
        Vector3 startPos = mainCamera.transform.position;

        // 目標位置是廣場中心
        Vector3 targetPos = new Vector3(
            transform.position.x,
            transform.position.y,
            mainCamera.transform.position.z
        );

        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;
            mainCamera.orthographicSize = Mathf.Lerp(startSize, targetSize, t);
            mainCamera.transform.position = Vector3.Lerp(startPos, targetPos, t);
            yield return null;
        }

        mainCamera.orthographicSize = targetSize;
        mainCamera.transform.position = targetPos;
    }

    IEnumerator CrossFade(GameObject fadeOut, GameObject fadeIn, float duration)
    {
        SpriteRenderer srOut = fadeOut.GetComponentInChildren<SpriteRenderer>();

        fadeIn.SetActive(true);
        SpriteRenderer srIn = fadeIn.GetComponentInChildren<SpriteRenderer>();

        Color outOriginal = srOut != null ? srOut.color : Color.white;
        Color inOriginal = srIn != null ? srIn.color : Color.white;

        if (srIn != null)
            srIn.color = new Color(inOriginal.r, inOriginal.g, inOriginal.b, 0f);

        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;

            if (srOut != null)
                srOut.color = new Color(outOriginal.r, outOriginal.g, outOriginal.b, 1f - t);
            if (srIn != null)
                srIn.color = new Color(inOriginal.r, inOriginal.g, inOriginal.b, t);

            yield return null;
        }

        fadeOut.SetActive(false);

        // 復原兩者的 alpha，供下次重新開啟時使用
        if (srOut != null) srOut.color = outOriginal;
        if (srIn != null) srIn.color = inOriginal;
    }
    IEnumerator MovePlayerToCenter()
    {
        player.SetInputLocked(true);

        Vector2 targetPos = centerPoint != null ?
            (Vector2)centerPoint.position : (Vector2)transform.position;

        Rigidbody2D playerRb = player.GetComponent<Rigidbody2D>();
        float speed = 3f;

        while (Vector2.Distance(playerRb.position, targetPos) > 0.1f)
        {
            Vector2 direction = (targetPos - playerRb.position).normalized;
            player.SetAutoMove(direction);
            playerRb.MovePosition(playerRb.position + direction * speed * Time.fixedDeltaTime);
            yield return new WaitForFixedUpdate();
        }

        playerRb.position = targetPos;

        // 到達後轉成正面
        player.SetAutoMove(Vector2.down);
        yield return new WaitForSeconds(0.1f);
        player.SetAutoMove(Vector2.zero);

        // 播放坐下動畫
        bool isPurify = PlayerPrefs.GetInt("EntityPurified", 0) == 1;
        if (isPurify)
            yield return StartCoroutine(player.PlayBongAnimation());
        else
            player.PlaySitDownAnimation();
    }

    IEnumerator GlitchEffect()
    {
        if (canvasGroup == null) yield break;

        for (int i = 0; i < 10; i++)
        {
            // Canvas 閃爍
            canvasGroup.alpha = Random.Range(0f, 0.3f);

            // 鏡頭輕微位移
            Vector3 originalPos = mainCamera.transform.position;
            mainCamera.transform.position = originalPos + new Vector3(
                Random.Range(-0.1f, 0.1f),
                Random.Range(-0.1f, 0.1f),
                0
            );

            yield return new WaitForSeconds(Random.Range(0.03f, 0.08f));

            mainCamera.transform.position = originalPos;
            canvasGroup.alpha = 1f;

            yield return new WaitForSeconds(Random.Range(0.03f, 0.1f));
        }
    }


    void ShowSystemAlert()
    {
#if UNITY_STANDALONE_OSX
    ShowMacAlert();
#elif UNITY_STANDALONE_WIN
    ShowWindowsAlert();
#endif
    }

    void ShowMacAlert()
    {
        System.Diagnostics.Process process = new System.Diagnostics.Process();
        process.StartInfo.FileName = "osascript";
        process.StartInfo.Arguments = "-e 'display alert \"\\\"meditation_daemon\\\" quit unexpectedly.\" buttons {\"Report...\", \"OK\"} default button \"OK\"'";
        process.StartInfo.UseShellExecute = false;
        process.StartInfo.RedirectStandardOutput = true;
        process.Start();
        process.WaitForExit();
    }

    void ShowWindowsAlert()
    {
        System.Diagnostics.Process process = new System.Diagnostics.Process();
        process.StartInfo.FileName = "powershell";
        process.StartInfo.Arguments = "-Command \"Add-Type -AssemblyName PresentationFramework; [System.Windows.MessageBox]::Show('meditation_daemon has stopped responding.', 'System Error', 'OKCancel', 'Error')\"";
        process.StartInfo.UseShellExecute = false;
        process.StartInfo.RedirectStandardOutput = true;
        process.StartInfo.CreateNoWindow = true;
        process.Start();
        process.WaitForExit();
    }

    IEnumerator CameraSwayEffect()
    {
        isSwaying = true;
        Vector3 basePos = mainCamera.transform.position;

        while (isSwaying)
        {
            float x = Mathf.Sin(Time.time * 0.8f) * 0.05f;
            float y = Mathf.Sin(Time.time * 0.6f) * 0.03f;
            mainCamera.transform.position = basePos + new Vector3(x, y, 0);
            yield return null;
        }

        mainCamera.transform.position = basePos;
    }

    public void ForceTriggered()
    {
        triggered = true;
        playerInRange = false;
        if (promptText != null)
            promptText.gameObject.SetActive(false);
    }

    public void ResetTrigger()
    {
        triggered = false;
        playerInRange = false;
    }

    public void SetInputLocked(bool locked)
    {
        inputLocked = locked;
        if (locked) playerInRange = false;
    }
}