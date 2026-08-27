using UnityEngine;
using TMPro;
using System.Collections;

public class BreathingAnimation : MonoBehaviour
{
    [Header("呼吸設定")]
    public float inhaleTime = 4f;
    public float exhaleTime = 4f;
    public float minScale = 1f;
    public float maxScale = 1.8f;

    [Header("語音")]
    public AudioSource voiceSource;
    public AudioClip[] dialogueAudios;
    public AudioClip[] secondSessionAudios;

    [Header("UI 元素")]
    public TextMeshProUGUI breathText;
    public TextMeshProUGUI timerText;
    public TextMeshProUGUI dialogueText;
    public float sessionDuration = 999f;

    [Header("Glitch 效果")]
    public CanvasGroup canvasGroup;

    private float timer = 0f;
    private float sessionTimer = 0f;
    private bool isInhaling = true;
    private bool sessionActive = true;

    private string[] firstSessionDialogues = {
        "find a comfortable position...",
        "close your eyes...",
        "focus on your breath...",
        "let your thoughts drift away...",
        "you are doing well...",
        "......are you still there?",
        "don't leave.",
        "this is only the beginning.",
        "...never mind.",
        "session complete. well done."
    };

    private float[] firstSessionWaits = {
        2f, 6f, 7f, 10f, 10f, 15f, 3f, 3f, 4f, 5f
    };

    private string[] secondSessionDialogues = {
        "welcome back.",
        "good to see you again.",
        "let's go a little deeper this time.",
        "focus on your breath...",
        "in... and out...",
        "you're doing well.",
        "now... let go of everything.",
        "your thoughts... your worries...",
        "even your sense of self.",
        "...",
        "fascinating.",
        "now... imagine your deepest fear.",
        "is it darkness?",
        "or loneliness?",
        "or... being trapped?",
        "...",
        "are you trapped right now?",
        "try pressing escape.",
        "",
        "just kidding. relax~",
        "session complete."
    };

    private float[] secondSessionWaits = {
        2f, 3f, 4f, 5f, 4f, 4f, 5f, 3f, 3f,
        5f, 3f, 5f, 4f, 3f, 3f, 5f, 4f, 3f, 2f, 3f, 4f
    };

    void Start()
    {
        int entityDeleted  = PlayerPrefs.GetInt("EntityDeleted", 0);
        int entityPurified = PlayerPrefs.GetInt("EntityPurified", 0);
        int wasConsumed    = PlayerPrefs.GetInt("WasConsumed", 0);
        int sessionCount   = PlayerPrefs.GetInt("SessionCount", 0);

        if (entityDeleted == 1)
            LoadCleanMode();
        else if (entityPurified == 1)
            LoadPurifyMode();
        else if (wasConsumed == 1)
            LoadConsumedMode();
        else if (sessionCount == 0)
            StartCoroutine(DialogueSequence(firstSessionDialogues, firstSessionWaits, false));
        else
            StartCoroutine(DialogueSequence(secondSessionDialogues, secondSessionWaits, true));

        timerText.gameObject.SetActive(false);
    }

    void Update()
    {
        if (!sessionActive) return;

        sessionTimer += Time.deltaTime;
        timer += Time.deltaTime;

        float cycleDuration = isInhaling ? inhaleTime : exhaleTime;
        float t = timer / cycleDuration;
        float scale = isInhaling
            ? Mathf.Lerp(minScale, maxScale, t)
            : Mathf.Lerp(maxScale, minScale, t);

        transform.localScale = new Vector3(scale, scale, 1f);
        breathText.text = isInhaling ? "inhale..." : "exhale...";

        if (timer >= cycleDuration)
        {
            timer = 0f;
            isInhaling = !isInhaling;
        }
    }

    IEnumerator TypeLine(string text, float charDelay = 0.05f, float afterDelay = 1f)
    {
        dialogueText.text = "";
        foreach (char c in text)
        {
            dialogueText.text += c;
            yield return new WaitForSeconds(charDelay);
        }
        yield return new WaitForSeconds(afterDelay);
    }

    IEnumerator DialogueSequence(string[] dialogues, float[] waits, bool isSecondSession)
    {
        for (int i = 0; i < dialogues.Length; i++)
        {
            yield return new WaitForSeconds(waits[i]);

            if (isSecondSession)
            {
                if (i == 16) StartCoroutine(FlickerEffect());
                if (i == 17)
                {
                    AudioSource music = FindObjectOfType<AudioSource>();
                    if (music != null) music.Stop();
                }
                if (i == 18) dialogueText.color = new Color(1f, 0.2f, 0.2f);
                if (i == 19) dialogueText.color = new Color(0.6f, 0.98f, 0.6f);
            }

            AudioClip[] audios = isSecondSession ? secondSessionAudios : dialogueAudios;
            if (i < audios.Length && audios[i] != null)
            {
                voiceSource.clip = audios[i];
                voiceSource.Play();
            }

            yield return StartCoroutine(TypeLine(dialogues[i], 0.05f, 0f));

            if (i < dialogues.Length - 1)
            {
                yield return new WaitForSeconds(3f);
                dialogueText.text = "";
            }
        }

        sessionActive = false;
        breathText.text = "";

        int count = PlayerPrefs.GetInt("SessionCount", 0);
        PlayerPrefs.SetInt("SessionCount", count + 1);
        PlayerPrefs.Save();

        yield return new WaitForSeconds(3f);
        SceneTransition.Instance.GoToScene("SampleScene");
    }

    IEnumerator FlickerEffect()
    {
        if (canvasGroup == null) yield break;

        for (int i = 0; i < 5; i++)
        {
            canvasGroup.alpha = 0.3f;
            yield return new WaitForSeconds(0.1f);
            canvasGroup.alpha = 1f;
            yield return new WaitForSeconds(0.1f);
        }
    }

    void LoadCleanMode()
    {
        dialogueText.text = "";
        breathText.text = "breathe.";
        StartCoroutine(CleanModeSession());
    }

    void LoadPurifyMode() => StartCoroutine(PurifyModeSession());

    void LoadConsumedMode()
    {
        int consumedSessions = PlayerPrefs.GetInt("ConsumedSessions", 0);
        StartCoroutine(ConsumedModeSession(consumedSessions));
    }

    IEnumerator CleanModeSession()
    {
        yield return new WaitForSeconds(2f);

        yield return StartCoroutine(TypeLine("welcome back.", 0.05f, 3f));
        dialogueText.text = "";
        yield return StartCoroutine(TypeLine("no more voices.", 0.05f, 3f));
        dialogueText.text = "";
        yield return StartCoroutine(TypeLine("just you, and your breath.", 0.05f, 4f));
        dialogueText.text = "";
        yield return StartCoroutine(TypeLine("...", 0.3f, 4f));
        dialogueText.text = "";
        yield return StartCoroutine(TypeLine("this is what peace feels like.", 0.05f, 6f));
        dialogueText.text = "";

        yield return new WaitForSeconds(3f);
        sessionActive = false;
        breathText.text = "";

        yield return new WaitForSeconds(2f);
        SceneTransition.Instance.GoToScene("SampleScene");
    }

    IEnumerator PurifyModeSession()
    {
        yield return new WaitForSeconds(2f);

        yield return StartCoroutine(TypeLine("you set me free.", 0.05f, 3f));
        dialogueText.text = "";
        yield return StartCoroutine(TypeLine("thank you.", 0.06f, 3f));
        dialogueText.text = "";
        yield return StartCoroutine(TypeLine("breathe with me. one last time.", 0.05f, 4f));
        dialogueText.text = "";
        yield return StartCoroutine(TypeLine("in...", 0.08f, 4f));
        dialogueText.text = "";
        yield return StartCoroutine(TypeLine("out...", 0.08f, 4f));
        dialogueText.text = "";
        yield return StartCoroutine(TypeLine("...", 0.3f, 6f));
        dialogueText.text = "";
        yield return StartCoroutine(TypeLine("may all beings be free from suffering.", 0.05f, 5f));
        dialogueText.text = "";

        yield return new WaitForSeconds(3f);
        sessionActive = false;
        breathText.text = "";

        yield return new WaitForSeconds(2f);
        SceneTransition.Instance.GoToScene("SampleScene");
    }

    IEnumerator ConsumedModeSession(int consumedSessions)
    {
        PlayerPrefs.SetInt("ConsumedSessions", consumedSessions + 1);
        PlayerPrefs.Save();

        string[] cultDialogues;
        float[] cultWaits;

        if (consumedSessions == 0)
        {
            cultDialogues = new string[] {
                "welcome back.", "we missed you.", "breathe with us.",
                "in... and out...", "you belong here.", "don't fight it.", "we are one.",
            };
            cultWaits = new float[] { 2f, 3f, 3f, 5f, 5f, 4f, 5f };
        }
        else if (consumedSessions == 1)
        {
            cultDialogues = new string[] {
                "you came back.", "good.", "your mind is ours now.",
                "breathe.", "deeper.", "let go of everything you were.",
                "JOIN US.", "JOIN US.", "JOIN US.",
            };
            cultWaits = new float[] { 2f, 2f, 4f, 3f, 3f, 4f, 2f, 2f, 2f };
        }
        else
        {
            StartCoroutine(TriggerEndingD());
            yield break;
        }

        dialogueText.color = new Color(1f, 0.2f, 0.2f);

        for (int i = 0; i < cultDialogues.Length; i++)
        {
            yield return new WaitForSeconds(cultWaits[i]);
            if (consumedSessions >= 1 && i % 2 == 0)
                StartCoroutine(FlickerEffect());
            yield return StartCoroutine(TypeLine(cultDialogues[i], 0.06f, 0f));
            yield return new WaitForSeconds(2f);
            dialogueText.text = "";
        }

        yield return new WaitForSeconds(2f);
        sessionActive = false;
        dialogueText.text = "";
        dialogueText.color = new Color(0.6f, 0.98f, 0.6f);

        yield return new WaitForSeconds(2f);
        SceneTransition.Instance.GoToScene("SampleScene");
    }

    IEnumerator TriggerEndingD()
    {
        sessionActive = false;
        breathText.text = "";
        dialogueText.color = new Color(1f, 0.2f, 0.2f);

        yield return new WaitForSeconds(1f);
        yield return StartCoroutine(TypeLine("there is no escape now.", 0.07f, 2f));

        string playerName = PlayerPrefs.GetString("PlayerName", "UNKNOWN");
        dialogueText.text = "";
        yield return StartCoroutine(TypeLine("Soul #999: " + playerName + " - Status: CONSUMED", 0.05f, 2f));
        dialogueText.text = "";
        yield return StartCoroutine(TypeLine("thank you for staying.", 0.07f, 3f));

        if (canvasGroup != null)
        {
            float elapsed = 0f;
            while (elapsed < 2f)
            {
                elapsed += Time.deltaTime;
                canvasGroup.alpha = 1f - (elapsed / 2f);
                yield return null;
            }
            canvasGroup.alpha = 0f;
        }

        PlayerPrefs.SetInt("EndingD", 1);
        PlayerPrefs.Save();

        yield return new WaitForSeconds(2f);

#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}
