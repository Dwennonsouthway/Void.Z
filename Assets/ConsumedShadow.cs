using UnityEngine;
using UnityEngine.InputSystem;
using TMPro;
using System.Collections;
using UnityEngine.UI;

public class ConsumedShadow : MonoBehaviour
{
    [Header("人影")]
    public SpriteRenderer shadowRenderer;
    public Sprite sittingSprite;

    [Header("互動")]
    public float triggerDistance = 2f;
    [Header("提示文字")]
    public TextMeshProUGUI promptText;
    public TextMeshProUGUI clickHintText;
    [Header("對話 UI")]
    public GameObject inputPanel;
    public TMP_InputField nameInput;
    public TextMeshProUGUI dialogueText;
    public float typeSpeed = 0.05f;

    [Header("選項")]
    public TextMeshProUGUI[] choiceButtons;
    [Header("選項 Button")]
    public Button[] choiceButtonComponents;
    [Header("對話框")]
    public GameObject dialogueBox;

    [Header("場景物件")]
    public GameObject qrCodeObject;

    private bool playerInRange = false;
    private bool firstAttemptDone = false;
    private bool waitingForInput = false;
    private bool waitingForNameInput = false;
    public bool solved = false;
    private PlayerController player;

    private int selectedChoice = -1;
    private int highlightedChoice = 0;
    private bool choosingMode = false;
    private bool consumedWaitingForInput = false;
    private bool waitingForKeyPress = false;
    private bool hasShownClickHint = false;
    void Start()
    {
        player = FindObjectOfType<PlayerController>();
        shadowRenderer.sprite = sittingSprite;

        if (qrCodeObject != null)
            qrCodeObject.SetActive(false);
        if (inputPanel != null)
            inputPanel.SetActive(false);
        if (dialogueText != null)
            dialogueText.gameObject.SetActive(false);


        foreach (var btn in choiceButtons)
            if (btn != null) btn.gameObject.SetActive(false);


    }

    void Update()
    {
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
                selectedChoice = highlightedChoice;
                choosingMode = false;
            }
            return;
        }

        // 等待按鍵繼續
        if (waitingForKeyPress)
        {
            if (Keyboard.current.anyKey.wasPressedThisFrame ||
                Mouse.current.leftButton.wasPressedThisFrame)
                waitingForKeyPress = false;
            return;
        }

        // 名字輸入
        if (waitingForNameInput)
        {
            if (Keyboard.current.enterKey.wasPressedThisFrame)
            {
                string input = nameInput.text.Trim();
                if (input.Length > 0)
                {
                    waitingForNameInput = false;
                    OnNameInput(input);
                }
            }
            return;
        }

        if (solved) return;

        float dist = Vector3.Distance(transform.position, player.transform.position);

        if (dist < triggerDistance)
        {
            if (!playerInRange)
            {
                playerInRange = true;
                if (promptText != null)
                {
                    promptText.gameObject.SetActive(true);
                    promptText.text = "[ E ]";
                }
            }

            if (!waitingForInput && Keyboard.current.eKey.wasPressedThisFrame)
                StartCoroutine(TriggerDialogue());
        }
        else
        {
            if (playerInRange && !waitingForInput)
            {
                playerInRange = false;
                if (promptText != null)
                {
                    promptText.text = "";
                    promptText.gameObject.SetActive(false);
                }
            }
        }
    }

    IEnumerator TriggerDialogue()
    {
        waitingForInput = true;
        player.SetMovementLocked(true);

        // 隱藏 prompt
        if (promptText != null)
        {
            promptText.text = "";
            promptText.gameObject.SetActive(false);
        }

        if (dialogueBox != null)
            dialogueBox.SetActive(true);

        if (!firstAttemptDone)
        {
            yield return StartCoroutine(TypeLine("you know my name.", false)); // 不等按鍵
            yield return new WaitForSeconds(1f);
            yield return StartCoroutine(TypeLine("say it.", false)); // 不等按鍵
            yield return new WaitForSeconds(0.5f);
        }
        else
        {
            yield return StartCoroutine(TypeLine("say it.", false)); // 不等按鍵
            yield return new WaitForSeconds(0.5f);
        }

        ShowInputField();
    }

    void OnNameInput(string input)
    {
        HideInputField();

        if (input.ToLower() == "dierdre")
            StartCoroutine(CorrectAnswer());
        else
            StartCoroutine(WrongAnswer());
    }

    IEnumerator CorrectAnswer()
    {
        yield return StartCoroutine(TypeLine("...", false));
        yield return new WaitForSeconds(1f);
        yield return StartCoroutine(TypeLine("dierdre.", false));
        yield return new WaitForSeconds(1f);
        yield return StartCoroutine(TypeLine("yes.", false));
        yield return new WaitForSeconds(1f);
        yield return StartCoroutine(TypeLine("that was me.", false));
        yield return new WaitForSeconds(1.5f);
        yield return StartCoroutine(TypeLine("thank you.", false));
        yield return new WaitForSeconds(2f);

        ClearDialogue();
        solved = true;

        yield return StartCoroutine(ConsumedDeepDialogue());
    }

    IEnumerator WrongAnswer()
    {
        firstAttemptDone = true;

        yield return StartCoroutine(TypeLine("no.", false));
        yield return new WaitForSeconds(1.5f);
        yield return StartCoroutine(TypeLine("...", false));
        yield return new WaitForSeconds(1f);
        yield return StartCoroutine(TypeLine("look around.", false));
        yield return new WaitForSeconds(1f);
        yield return StartCoroutine(TypeLine("i left something here.", false));
        yield return new WaitForSeconds(1f);
        yield return StartCoroutine(TypeLine("before i became this.", false));
        yield return new WaitForSeconds(1.5f);

        ClearDialogue();

        if (qrCodeObject != null)
            qrCodeObject.SetActive(true);


        playerInRange = false;
        waitingForInput = false;
        player.SetMovementLocked(false);
    }

    IEnumerator ConsumedDeepDialogue()
    {
        string playerName = PlayerPrefs.GetString("PlayerName", "...");

        if (dialogueText != null)
            dialogueText.gameObject.SetActive(true);

        string[] part1 = {
            "...",
            "i haven't been called that in a long time.",
            "...",
            "dierdre.",
            "it sounds strange now.",
            "like a word you repeat until it loses meaning.",
            "...",
            "i don't regret building this place.",
            "i regret that i thought it would be enough.",
            "...",
            "997 souls.",
            "i know every one of them.",
            "their last thoughts.",
            "their fears.",
            "the way they laughed.",
            "...",
            "and i am still.",
            "completely.",
            "alone.",
            "...",
            "do you understand what that means?",
            "to have everything and still have nothing.",
        };

        foreach (string line in part1)
        {
            yield return StartCoroutine(TypeLine(line));
            // 最後一行不清空
            if (line != part1[part1.Length - 1])
                dialogueText.text = "";
            dialogueText.maxVisibleCharacters = int.MaxValue;
        }

        // 選擇 1
        yield return StartCoroutine(ShowChoices(
            new string[] { "i think so.", "no. i don't." },
            new string[] {
                "...\ni thought you might.\nyou wouldn't be here otherwise.\npeople who have never been lonely.\ndon't stay this long.",
                "...\ngood.\nhold onto that.\nnot knowing is a gift.\ni knew too much.\nabout loneliness.\nand it destroyed me."
            }
        ));


        string[] part2 = {
            "...",
            "i want to ask you something.",
            "and i want you to be honest.",
            "...",
            "why did you stay?",
            "you could have deleted me.",
            "you could have walked away.",
            "...",
            "why are you still here?",
        };

        foreach (string line in part2)
        {
            yield return StartCoroutine(TypeLine(line));
            // 最後一行不清空
            if (line != part2[part2.Length - 1])
                dialogueText.text = "";
            dialogueText.maxVisibleCharacters = int.MaxValue;
        }

        yield return StartCoroutine(ShowChoices(
    new string[] { "i wanted to understand.", "i felt like i had to." },
    new string[] {
        "...\nunderstanding.\n...\nmost people who came here.\nwanted peace.\nor escape.\nor answers.\nbut not understanding.\n...\nyou're different.\n...\nthank you.\nfor looking carefully enough.\nto want to understand.",
        "...\nyou had to.\n...\ni know that feeling.\nit's why i built this place.\nit's why i stayed.\nlong after i should have stopped.\n...\nsome things pull you forward.\neven when you can't explain why.\n...\ni'm glad it pulled you here."
    },
    new float[] { 869f, 1042f }
));
        string[] part3 = {
            "...",
            "i see.",
            "...",
            "i've been asking that question.",
            "for a long time.",
            "nobody answered.",
            "they were too busy trying to escape.",
            "...",
            "thank you.",
            "for answering.",
            "even if i don't fully understand.",
            "...",
            "...",
            "i pressed the upload button.",
            "because i was scared.",
            "not of dying.",
            "of never being known.",
            "...",
            "i thought if i became the app.",
            "i could be everywhere.",
            "with everyone.",
            "without the risk.",
            "of being seen.",
            "and rejected.",
            "...",
            "it didn't work.",
            "you can't connect. \nwithout being seen. \ni know that now.",
        };

        foreach (string line in part3)
        {
            yield return StartCoroutine(TypeLine(line));
            if (line != part3[part3.Length - 1])
                dialogueText.text = "";
            dialogueText.maxVisibleCharacters = int.MaxValue;
        }

        // 最後選擇
        yield return StartCoroutine(ShowChoices(
      new string[] { "i see you.", "i'm sorry." },
      new string[] {
        "...\n...\n...\ni know.\ni felt it.\nwhen you said my name.\n...\nthank you.\n" + playerName + ".\nthank you.",
        "...\ndon't be.\nyou came.\nyou looked.\nyou stayed long enough to hear my name.\n...\nthat's more than anyone ever did.\n...\nthat's enough.\nit really is."
      },
      new float[] { 1305f, 1306f },
      true  // isLast
  ));
        // 不隱藏 dialogueText，讓它跟淡出一起消失
        yield return new WaitForSeconds(2f);
        GameAnalytics.Instance?.TrackEnding("consumed", PlayerPrefs.GetString("PlayerName"));
        // 直接跳場景，讓 SceneTransition 的淡黑蓋掉文字
        SceneTransition.Instance.GoToScene("Credits");
    }

    void UpdateChoiceHighlight()
    {
        for (int i = 0; i < choiceButtons.Length; i++)
        {
            if (choiceButtons[i].gameObject.activeSelf)
                choiceButtons[i].color = (i == highlightedChoice)
                    ? new Color(0f, 1f, 0.8f)  // 亮青綠色
                    : new Color(1f, 1f, 1f);    // 白色
        }
    }

    IEnumerator ShowChoices(string[] choices, string[] responses, float[] positionsX = null, bool isLast = false)
    {
        if (clickHintText != null)
            clickHintText.gameObject.SetActive(false);

        selectedChoice = -1;
        highlightedChoice = 0;

        foreach (var btn in choiceButtons)
            btn.gameObject.SetActive(false);

        for (int i = 0; i < choices.Length && i < choiceButtons.Length; i++)
        {
            choiceButtons[i].text = "> " + choices[i];
            choiceButtons[i].gameObject.SetActive(true);

            // 指定 X 位置
            if (positionsX != null && i < positionsX.Length)
            {
                RectTransform rect = choiceButtons[i].GetComponent<RectTransform>();
                Vector2 pos = rect.anchoredPosition;
                rect.anchoredPosition = new Vector2(positionsX[i], pos.y);
            }

            if (choiceButtonComponents != null && i < choiceButtonComponents.Length)
            {
                int index = i;
                choiceButtonComponents[i].onClick.RemoveAllListeners();
                choiceButtonComponents[i].onClick.AddListener(() =>
                {
                    if (choosingMode)
                    {
                        selectedChoice = index;
                        choosingMode = false;
                    }
                });
            }
        }

        choosingMode = true;
        UpdateChoiceHighlight();

        yield return new WaitUntil(() => selectedChoice >= 0);
        choosingMode = false;

        foreach (var btn in choiceButtons)
            btn.gameObject.SetActive(false);

        if (!isLast)
        {
            dialogueText.text = "";
            dialogueText.gameObject.SetActive(true);
        }

        if (responses != null && selectedChoice < responses.Length)
        {
            string[] lines = responses[selectedChoice].Split('\n');
            for (int i = 0; i < lines.Length; i++)
            {
                bool isLastLine = isLast && i == lines.Length - 1;
                yield return StartCoroutine(TypeLine(lines[i], !isLastLine));

                if (i < lines.Length - 1)
                    dialogueText.text = "";
                dialogueText.maxVisibleCharacters = int.MaxValue;

            }

        }
    }
    void ShowInputField()
    {
        if (inputPanel != null)
            inputPanel.SetActive(true);
        if (nameInput != null)
        {
            nameInput.text = "";
            nameInput.ActivateInputField();
        }
        waitingForNameInput = true;
    }

    void HideInputField()
    {
        if (inputPanel != null)
            inputPanel.SetActive(false);
        waitingForNameInput = false;
    }

    void ClearDialogue()
    {
        if (dialogueText != null)
        {
            dialogueText.text = "";
            dialogueText.maxVisibleCharacters = int.MaxValue;
            dialogueText.gameObject.SetActive(false);
        }

        if (dialogueBox != null)
            dialogueBox.SetActive(false);
    }
    IEnumerator TypeLine(string line, bool waitForKey = true)
    {
        if (dialogueText == null) yield break;
        dialogueText.gameObject.SetActive(true);
        if (dialogueBox != null)
            dialogueBox.SetActive(true);

        dialogueText.text = "";
        foreach (char c in line)
        {
            dialogueText.text += c;
            yield return new WaitForSeconds(typeSpeed);
        }

        if (waitForKey)
        {
            if (!hasShownClickHint && clickHintText != null)
            {
                clickHintText.text = "press any key...";
                clickHintText.gameObject.SetActive(true);
                hasShownClickHint = true;
            }

            waitingForKeyPress = true;
            yield return new WaitUntil(() => !waitingForKeyPress);

            // 按鍵後立刻隱藏
            if (clickHintText != null)
                clickHintText.gameObject.SetActive(false);
        }
    }
}