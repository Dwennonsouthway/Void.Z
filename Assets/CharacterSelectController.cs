using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using TMPro;
using System.Collections;

public class CharacterSelectController : MonoBehaviour
{
    [Header("角色圖片")]
    public Image maleImage;
    public Image femaleImage;

    [Header("文字")]
    public TextMeshProUGUI titleText;
    public TextMeshProUGUI subtitleText;

    [Header("名字輸入")]
    public TMP_InputField nameInputField;

    [Header("發光顏色")]
    public Color normalColor = Color.white;
    public Color hoverColor = new Color(1f, 0.5f, 1f, 1f);
    public Color selectedColor = new Color(1f, 0.2f, 1f, 1f);

    private int selectedCharacter = -1;
    private bool acceptingSelection = false;
    private bool acceptingName = false;

    void Start()
    {
        maleImage.color = new Color(1f, 1f, 1f, 0f);
        femaleImage.color = new Color(1f, 1f, 1f, 0f);
        titleText.text = "";
        subtitleText.text = "";
        nameInputField.gameObject.SetActive(false);

        StartCoroutine(IntroSequence());
    }

    void Update()
    {
        // 選角階段
        if (acceptingSelection)
        {
            Vector2 mousePos = Mouse.current.position.ReadValue();

            bool hoverMale = RectTransformUtility.RectangleContainsScreenPoint(
                maleImage.rectTransform, mousePos);
            bool hoverFemale = RectTransformUtility.RectangleContainsScreenPoint(
                femaleImage.rectTransform, mousePos);

            if (selectedCharacter != 0)
                maleImage.color = hoverMale ? hoverColor : normalColor;
            if (selectedCharacter != 1)
                femaleImage.color = hoverFemale ? hoverColor : normalColor;

            if (Mouse.current.leftButton.wasPressedThisFrame)
            {
                if (hoverMale) StartCoroutine(OnCharacterSelected(0));
                else if (hoverFemale) StartCoroutine(OnCharacterSelected(1));
            }
        }

        // 名字輸入階段
        if (acceptingName)
        {
            if (Keyboard.current.enterKey.wasPressedThisFrame)
            {
                string name = nameInputField.text.Trim();
                if (name.Length > 0)
                    StartCoroutine(ConfirmName(name));
            }
        }
    }

    // ── 開場序列 ────────────────────────────────────────────

    IEnumerator IntroSequence()
    {
        yield return new WaitForSeconds(1f);

        yield return StartCoroutine(TypeText(titleText, "YOUR CONSCIOUSNESS NEEDS A FORM.", 0.08f));
        yield return new WaitForSeconds(0.5f);
        yield return StartCoroutine(TypeText(subtitleText, "CHOOSE ONE.", 0.12f));
        yield return new WaitForSeconds(0.8f);

        yield return StartCoroutine(FadeInCharacters());

        acceptingSelection = true;
    }

    // ── 選角 ────────────────────────────────────────────────

    IEnumerator OnCharacterSelected(int index)
    {
        acceptingSelection = false;
        selectedCharacter = index;

        Image selected = selectedCharacter == 0 ? maleImage : femaleImage;
        Image other = selectedCharacter == 0 ? femaleImage : maleImage;

        // 另一個角色淡出
        StartCoroutine(FadeOutImage(other));

        // 閃爍選中角色
        for (int i = 0; i < 3; i++)
        {
            selected.color = Color.white;
            yield return new WaitForSeconds(0.1f);
            selected.color = selectedColor;
            yield return new WaitForSeconds(0.1f);
        }

        yield return new WaitForSeconds(0.3f);

        // 顯示確認提示
        subtitleText.text = "You sure?  > Y / N";
        yield return new WaitForSeconds(0.1f);

        // 等玩家確認或反悔
        bool decided = false;
        bool confirmed = false;

        while (!decided)
        {
            if (Keyboard.current.yKey.wasPressedThisFrame)
            {
                confirmed = true;
                decided = true;
            }
            else if (Keyboard.current.nKey.wasPressedThisFrame)
            {
                confirmed = false;
                decided = true;
            }
            yield return null;
        }

        subtitleText.text = "";

        if (!confirmed)
        {
            // 反悔：恢復兩個角色
            selectedCharacter = -1;
            yield return StartCoroutine(FadeInCharacters());
            acceptingSelection = true;
            yield break;
        }

        // 確認選角
        PlayerPrefs.SetInt("SelectedCharacter", selectedCharacter);
        PlayerPrefs.Save();

        yield return StartCoroutine(TransitionToNameInput());
    }

    IEnumerator FadeOutImage(Image img)
    {
        float elapsed = 0f;
        Color startColor = img.color;

        while (elapsed < 0.5f)
        {
            elapsed += Time.deltaTime;
            float alpha = 1f - (elapsed / 0.5f);
            img.color = new Color(startColor.r, startColor.g, startColor.b, alpha);
            yield return null;
        }

        img.color = new Color(startColor.r, startColor.g, startColor.b, 0f);
    }

    // ── 名字輸入 ─────────────────────────────────────────────

    IEnumerator TransitionToNameInput()
    {
        // 文字換成問名字
        titleText.text = "";
        subtitleText.text = "";

        yield return new WaitForSeconds(0.3f);

        yield return StartCoroutine(TypeText(titleText, "WHAT IS YOUR NAME?", 0.08f));
        yield return new WaitForSeconds(0.3f);

        // 輸入框出現
        nameInputField.gameObject.SetActive(true);
        nameInputField.ActivateInputField();
        acceptingName = true;
    }

    IEnumerator ConfirmName(string playerName)
    {
        acceptingName = false;
        nameInputField.gameObject.SetActive(false);
        PlayerPrefs.SetString("PlayerName", playerName);
        PlayerPrefs.SetInt("SessionCount", 1);
        PlayerPrefs.Save();

        yield return StartCoroutine(TypeText(subtitleText, "hello, " + playerName + ".", 0.07f));
        yield return new WaitForSeconds(1f);

        // Morgan 彩蛋：問要不要聽歌單
        if (playerName.Trim().ToLower() == "morgan")
        {
            yield return StartCoroutine(MorganPlaylistPrompt(playerName));
        }
        else
        {
            GameAnalytics.Instance?.TrackNameInput(playerName);
            SceneTransition.Instance.GoToScene("VoidSpace");
        }
    }

    // ── Helper ──────────────────────────────────────────────

    IEnumerator TypeText(TextMeshProUGUI tmp, string text, float charDelay)
    {
        tmp.text = "";
        foreach (char c in text)
        {
            tmp.text += c;
            yield return new WaitForSeconds(charDelay);
        }
    }

    IEnumerator FadeInCharacters()
    {
        float elapsed = 0f;

        while (elapsed < 1f)
        {
            elapsed += Time.deltaTime;
            float alpha = elapsed / 1f;
            maleImage.color = new Color(1f, 1f, 1f, alpha);
            femaleImage.color = new Color(1f, 1f, 1f, alpha);
            yield return null;
        }

        maleImage.color = Color.white;
        femaleImage.color = Color.white;
    }

    IEnumerator MorganPlaylistPrompt(string playerName)
    {
        // 清掉舊字，顯示特殊訊息
        titleText.text = "";
        subtitleText.text = "";
        yield return new WaitForSeconds(0.3f);

        yield return StartCoroutine(TypeText(titleText, "one moment, " + playerName + ".", 0.08f));
        yield return new WaitForSeconds(0.5f);
        yield return StartCoroutine(TypeText(subtitleText, "i made something for you.", 0.07f));
        yield return new WaitForSeconds(1f);

        subtitleText.text = "";
        yield return new WaitForSeconds(0.3f);

        // 顯示選項：用兩行文字模擬選項
        titleText.text = "want to listen?";
        subtitleText.text = "> Y / N";
        yield return new WaitForSeconds(0.3f);

        // 等玩家按 Y 或 N
        bool chosen = false;
        bool wantsPlaylist = false;

        while (!chosen)
        {
            if (Keyboard.current.yKey.wasPressedThisFrame)
            {
                wantsPlaylist = true;
                chosen = true;
            }
            else if (Keyboard.current.nKey.wasPressedThisFrame)
            {
                wantsPlaylist = false;
                chosen = true;
            }
            yield return null;
        }

        titleText.text = "";
        subtitleText.text = "";

        if (wantsPlaylist)
        {
            PlayerPrefs.SetInt("MorganPlaylistEnabled", 1);
            PlayerPrefs.Save();

            if (PersonalPlaylist.Instance != null)
                PersonalPlaylist.Instance.StartPlaylist();

            yield return new WaitForSeconds(0.5f);
            yield return StartCoroutine(TypeText(subtitleText, "this one's yours.", 0.07f));
            yield return new WaitForSeconds(1.5f);

            // 提示長按 Tab
            subtitleText.text = "";
            yield return new WaitForSeconds(0.3f);
            yield return StartCoroutine(TypeText(titleText, "hold [ TAB ] to open the player.", 0.07f));

            // 等玩家真的長按 Tab 超過 holdDuration
            float holdTimer = 0f;
            float holdDuration = 0.3f;
            bool tabShown = false;

            while (!tabShown)
            {
                if (Keyboard.current.tabKey.isPressed)
                {
                    holdTimer += Time.deltaTime;
                    if (holdTimer >= holdDuration)
                        tabShown = true;
                }
                else
                {
                    holdTimer = 0f;
                }
                yield return null;
            }

            // 玩家做到了，給回饋再轉場
            titleText.text = "";
            yield return StartCoroutine(TypeText(subtitleText, "good.", 0.07f));
            yield return new WaitForSeconds(1f);
        }
        else
        {
            PlayerPrefs.SetInt("MorganPlaylistEnabled", 0);
            PlayerPrefs.Save();
            yield return StartCoroutine(TypeText(subtitleText, "okay.", 0.07f));
            yield return new WaitForSeconds(1f);
        }

        GameAnalytics.Instance?.TrackNameInput(playerName);
        SceneTransition.Instance.GoToScene("VoidSpace");
    }
}