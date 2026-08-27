using UnityEngine;
using TMPro;
using System.Collections;
using UnityEngine.UI;

public class CreditsController : MonoBehaviour
{
    public TextMeshProUGUI creditsText;
    public RectTransform creditsRect;
    public float scrollSpeed = 50f;
    public float fadeInDuration = 2f;

    void Start()
    {
        creditsRect = creditsText.GetComponent<RectTransform>();
        StartCoroutine(PlayCredits());
    }

    IEnumerator PlayCredits()
    {
        bool isConsumed = PlayerPrefs.GetInt("WasConsumed", 0) == 1;
        string playerName = PlayerPrefs.GetString("PlayerName", "you");

        // 先把畫面設成透明，避免閃爍
        CanvasGroup cg = GetComponentInParent<CanvasGroup>();
        if (cg != null) cg.alpha = 0f;

        if (isConsumed)
        {
            creditsText.text =
                "Void.zen\n\n\n" +
                "Concept & Story          Dwennon\n\n" +
                "Narrative Design         Dwennon\n\n" +
                "Programming              Dwennon\n\n" +
                "UI/UX Design             Dwennon\n\n" +
                "Art & Animation          Dwennon\n\n" +
                "Character Design         Dwennon\n\n" +
                "Environment Art          Dwennon\n\n" +
                "Sound Design             Tiana\n\n" +
                "Music                    Tiana\n\n" +
                "Quality Assurance        Dwennon\n\n" +
                "Emotional Support        Kiryu Kazuma\n\n" +

                "\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n" +
                "Special Thanks\n\n" +
                playerName + "\n\n" +
                "for staying long enough\n" +
                "to say her name.";
        }
        else
        {
            creditsText.text =
                "Void.zen\n\n\n" +
                "Concept & Story          Dwennon\n\n" +
                "Narrative Design         Dwennon\n\n" +
                "Programming              Dwennon\n\n" +
                "UI/UX Design             Dwennon\n\n" +
                "Art & Animation          Dwennon\n\n" +
                "Character Design         Dwennon\n\n" +
                "Environment Art          Dwennon\n\n" +
                "Sound Design             Tiana\n\n" +
                "Music                    Tiana\n\n" +

                "Quality Assurance        Dwennon\n\n" +
                "Emotional Support        Kiryu Kazuma\n\n" +
                "\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n" +
                "Special Thanks\n\n" +
                playerName + "\n\n" +
                "for being the first to truly see.";
        }

        // 等文字建立完
        yield return new WaitForEndOfFrame();

        float canvasHeight = GetCanvasHeight();
        float startY = -canvasHeight / 2f;
        float endY = creditsText.preferredHeight + canvasHeight;
        creditsRect.anchoredPosition = new Vector2(0, startY);

        // 位置設好後才淡入
        if (cg != null)
        {
            cg.alpha = 0f;
            float elapsed = 0f;
            while (elapsed < fadeInDuration)
            {
                elapsed += Time.deltaTime;
                cg.alpha = elapsed / fadeInDuration;
                yield return null;
            }
            cg.alpha = 1f;
        }
        // 計算停止位置也改用 canvasHeight
        float screenCenter = canvasHeight / 2f;
        float stopY = creditsText.preferredHeight - screenCenter + 330f;

        bool stopped = false;

        while (creditsRect.anchoredPosition.y < endY)
        {
            creditsRect.anchoredPosition += new Vector2(0, scrollSpeed * Time.deltaTime);

            if (!stopped && creditsRect.anchoredPosition.y >= stopY)
            {
                stopped = true;
                creditsRect.anchoredPosition = new Vector2(0, stopY);
                break;
            }

            yield return null;
        }

        yield return new WaitForSeconds(5f);


        if (isConsumed)
        {
            PlayerPrefs.SetInt("GameCompleted_consumed", 1);
            PlayerPrefs.DeleteKey("WasConsumed");
            PlayerPrefs.Save();

            // 音樂淡出
            if (MusicManager.Instance != null)
                MusicManager.Instance.FadeOutForPlaylist();

            // 畫面慢慢淡黑
            if (cg != null)
            {
                float elapsed = 0f;
                while (elapsed < 3f)
                {
                    elapsed += Time.deltaTime;
                    cg.alpha = 1f - (elapsed / 3f);
                    yield return null;
                }
                cg.alpha = 0f;
            }

            yield return new WaitForSeconds(2f);

            // 黑畫面上逐行出現文字
            creditsText.color = Color.white;
            creditsText.alignment = TMPro.TextAlignmentOptions.Center;
            creditsRect.anchoredPosition = Vector2.zero;

            if (cg != null) cg.alpha = 1f;

            string[] finalLines = {
        "...",
        "thank you.",
        "...",
        "I can rest now.",
    };

            creditsText.text = "";

            foreach (string line in finalLines)
            {
                // 逐字打出
                foreach (char c in line)
                {
                    creditsText.text += c;
                    yield return new WaitForSeconds(0.06f);
                }

                yield return new WaitForSeconds(2f);
                creditsText.text = "";
                yield return new WaitForSeconds(0.5f);
            }

            yield return new WaitForSeconds(1f);

            // 最後顯示玩家名字
            creditsText.text = playerName + ".";
            yield return new WaitForSeconds(1f);

            // 再一行
            creditsText.text = "";
            yield return new WaitForSeconds(0.5f);
            creditsText.text = "thank you for staying.";
            yield return new WaitForSeconds(3f);

            // 淡出
            float fadeElapsed = 0f;
            while (fadeElapsed < 2f)
            {
                fadeElapsed += Time.deltaTime;
                creditsText.color = new Color(1f, 1f, 1f, 1f - (fadeElapsed / 2f));
                yield return null;
            }

            yield return new WaitForSeconds(1f);
            SceneTransition.Instance.GoToScene("SampleScene");
        }
        else
        {
            // 記錄通關，清掉結局 flag
            PlayerPrefs.SetInt("GameCompleted_purify", 1);
            PlayerPrefs.DeleteKey("EntityPurified");
            PlayerPrefs.DeleteKey("Collected_bong");
            PlayerPrefs.DeleteKey("Collected_weed");
            PlayerPrefs.DeleteKey("Collected_bowl");
            PlayerPrefs.DeleteKey("TerminalUnlocked");
            PlayerPrefs.DeleteKey("EntityIntroDone");
            PlayerPrefs.DeleteKey("MorganPlaylistEnabled");
            PlayerPrefs.Save();

            // 音樂淡出
            if (MusicManager.Instance != null)
                MusicManager.Instance.FadeOutForPlaylist();

            // 畫面慢慢淡黑
            if (cg != null)
            {
                float elapsed = 0f;
                while (elapsed < 3f)
                {
                    elapsed += Time.deltaTime;
                    cg.alpha = 1f - (elapsed / 3f);
                    yield return null;
                }
                cg.alpha = 0f;
            }

            yield return new WaitForSeconds(2f);

            // 黑畫面上逐行出現文字
            creditsText.color = Color.white;
            creditsText.alignment = TMPro.TextAlignmentOptions.Center;
            creditsRect.anchoredPosition = Vector2.zero;

            if (cg != null) cg.alpha = 1f;

            string[] finalLines = {
        "998 souls.",
        "...",
        "finally free.",
        "...",
        "so am I.",
    };

            creditsText.text = "";

            foreach (string line in finalLines)
            {
                foreach (char c in line)
                {
                    creditsText.text += c;
                    yield return new WaitForSeconds(0.06f);
                }

                yield return new WaitForSeconds(2f);
                creditsText.text = "";
                yield return new WaitForSeconds(0.5f);
            }


            yield return new WaitForSeconds(1f);

            // 淡出
            float fadeElapsed = 0f;
            while (fadeElapsed < 2f)
            {
                fadeElapsed += Time.deltaTime;
                creditsText.color = new Color(1f, 1f, 1f, 1f - (fadeElapsed / 2f));
                yield return null;
            }

            yield return new WaitForSeconds(1f);
            SceneTransition.Instance.GoToScene("SampleScene");
        }
    }

    private float GetCanvasHeight()
    {
        Canvas canvas = GetComponentInParent<Canvas>();
        if (canvas != null)
            return canvas.GetComponent<RectTransform>().rect.height;
        return Screen.height; // fallback
    }
}