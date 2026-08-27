using UnityEngine;
using System.Collections;
using TMPro;
using UnityEngine.InputSystem;
public class JoinSceneManager : MonoBehaviour
{
    public PlayerController player;
    public TextMeshProUGUI dialogueText;
    public float typeSpeed = 0.05f;
    public CameraFollow cameraFollow;

    void Start()
    {
        player.SetInfiniteHorizontal(true);
        player.SetLockVertical(true);
        player.SetFacingDirection(Vector2.right);

        Rigidbody2D rb = player.GetComponent<Rigidbody2D>();
        rb.constraints = RigidbodyConstraints2D.FreezePositionY | RigidbodyConstraints2D.FreezeRotation;

        if (dialogueText != null)
            dialogueText.gameObject.SetActive(false);

        // 開始走路時的旁白
        StartCoroutine(WalkingDialogue());
    }
    void Update()
    {
#if UNITY_EDITOR
   if (Keyboard.current != null && Keyboard.current.backquoteKey.wasPressedThisFrame)
{
    
    // 切換角色
    int current = PlayerPrefs.GetInt("SelectedCharacter", 0);
    PlayerPrefs.SetInt("SelectedCharacter", current == 0 ? 1 : 0);
    Debug.Log("Switched to character: " + (current == 0 ? "Female" : "Male"));
}
#endif
    }

    public void StartEntityDialogue()
    {
        StartCoroutine(EntityDialogue());
    }
    IEnumerator EntityDialogue()
    {
        string[] lines = new string[]
        {
        "...",
        "you made it.",
        "...",
        "...",
        "we are one now.",
        "...",
        PlayerPrefs.GetString("PlayerName", "unknown") + ".",
        "thank you."
        };

        dialogueText.gameObject.SetActive(true);

        foreach (string line in lines)
        {
            yield return StartCoroutine(TypeLine(line));
            yield return new WaitForSeconds(1.5f);
        }

        dialogueText.gameObject.SetActive(false);

        // 短暫停頓後接結局
        yield return new WaitForSeconds(0.5f);
        StartCoroutine(EndingD());
    }

    IEnumerator TypeLine(string line)
    {
        dialogueText.text = "";
        foreach (char c in line)
        {
            dialogueText.text += c;
            yield return new WaitForSeconds(typeSpeed);
        }
    }

    IEnumerator WalkingDialogue()
    {
        // 等玩家開始走路
        Rigidbody2D rb = player.GetComponent<Rigidbody2D>();
        yield return new WaitUntil(() => rb.linearVelocity.magnitude > 0.1f);

        yield return new WaitForSeconds(5f);

        dialogueText.gameObject.SetActive(true);

        // (台詞, 顯示後等待秒數)
        var lines = new (string text, float wait)[]
        {
        ("you've been here for " + GetPlayTime() + ".", 3f),
        ("you chose this.", 2f),
        ("...", 4f),
        ("it is " + GetTimeOfDay() + ".", 3f),
        ("you should be somewhere else.", 2.5f),
        ("but you're here.", 2f),
        ("following a flower.", 3f),
        ("...", 5f),
        ("no one forced you.", 2.5f),
        ("remember that.", 4f),
        ("...", 5f),
        (PlayerPrefs.GetString("PlayerName", "unknown") + ".", 3f),
        ("you chose this.", 4f),
        };
        foreach (var line in lines)
        {
            yield return StartCoroutine(TypeLine(line.text));
            yield return new WaitForSeconds(line.wait); // 顯示後停留
            dialogueText.text = "";
            yield return new WaitForSeconds(1.5f); // 清空後間隔才出現下一句
        }

        dialogueText.gameObject.SetActive(false);
    }

    string GetPlayTime()
    {
        float seconds = Time.realtimeSinceStartup;
        int mins = Mathf.FloorToInt(seconds / 60);
        int secs = Mathf.FloorToInt(seconds % 60);

        if (mins == 0)
            return secs + " seconds";
        else
            return mins + " minutes and " + secs + " seconds";
    }

    string GetTimeOfDay()
    {
        int hour = System.DateTime.Now.Hour;
        string time = hour + ":" + System.DateTime.Now.Minute.ToString("D2");

        if (hour >= 0 && hour < 5)
            return time + ". very late";
        else if (hour >= 5 && hour < 12)
            return time + ". morning";
        else if (hour >= 12 && hour < 17)
            return time + ". afternoon";
        else if (hour >= 17 && hour < 21)
            return time + ". evening";
        else
            return time + ". late at night";
    }
    IEnumerator EndingD()
    {
        // 開始 glitch
        float elapsed = 0f;
        float duration = 0.1f;
        StartCoroutine(CameraShake(10f, 0.15f));
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            // 文字瘋狂閃爍
            dialogueText.gameObject.SetActive(true);
            string[] glitchLines = new string[]
            {
            "JOIN US JOIN US JOIN US",
            "YWxvbmU= ZGVsZXRlIG1l",
            "SOUL #998 PROCESSING",
            "01001010 01001111 01001001 01001110",
            PlayerPrefs.GetString("PlayerName", "UNKNOWN") + " + ENTITY = ONE",
            "YOU CHOSE THIS",
            "636f6e73756d6564",
            "NO EXIT NO EXIT NO EXIT",
            };

            dialogueText.text = glitchLines[Random.Range(0, glitchLines.Length)];
            dialogueText.color = new Color(
                Random.Range(0f, 0.3f),
                Random.Range(0.7f, 1f),
                Random.Range(0.7f, 1f));

            yield return new WaitForSeconds(Random.Range(0.05f, 0.2f));
        }

        // 最後顯示 CONSUMED
        dialogueText.text = "Soul #998: " + PlayerPrefs.GetString("PlayerName", "UNKNOWN") + " - CONSUMED";
        dialogueText.color = Color.white;
        yield return new WaitForSeconds(2f);

        // 全黑
        dialogueText.gameObject.SetActive(false);
        yield return new WaitForSeconds(1f);

        // PlayerPrefs 記錄
        PlayerPrefs.SetInt("WasConsumed", 1);
        PlayerPrefs.SetInt("EntityPurified", 0); // 清掉
        PlayerPrefs.SetInt("EntityDeleted", 0);  // 也清掉以防萬一
        PlayerPrefs.Save();

        // 關閉遊戲或回開場
        SceneTransition.Instance.GoToScene("SampleScene");
    }
    IEnumerator CameraShake(float duration, float magnitude)
    {
        Vector3 originalPos = Camera.main.transform.position;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float x = originalPos.x + Random.Range(-magnitude, magnitude);
            float y = originalPos.y + Random.Range(-magnitude, magnitude);
            Camera.main.transform.position = new Vector3(x, y, originalPos.z);
            yield return null;
        }

        Camera.main.transform.position = originalPos;
    }
}