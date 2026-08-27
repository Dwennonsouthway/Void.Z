using UnityEngine;
using TMPro;
using System.Collections;

public class WelcomeSequence : MonoBehaviour
{
    public TextMeshProUGUI welcomeText;
    public GameObject nameSection;    // 只有名字輸入框
    public GameObject birthdaySection; // 只有生日輸入框
    public float fadeInDuration = 1.5f;

    private string[] lines = {
        "welcome.",
        "we have been waiting for you.",
        "before we begin...",
        "what is your name?"
    };

    void Start()
    {
        nameSection.SetActive(false);
        birthdaySection.SetActive(false);
        StartCoroutine(PlaySequence());
    }

    IEnumerator PlaySequence()
    {
        yield return new WaitForSeconds(1f);

        foreach (string line in lines)
        {
            welcomeText.text = "";
            foreach (char c in line)
            {
                welcomeText.text += c;
                yield return new WaitForSeconds(0.05f);
            }
            yield return new WaitForSeconds(1.5f);
            welcomeText.text = "";
        }

        // 淡入名字輸入框
        yield return StartCoroutine(FadeIn(nameSection));
    }

    public IEnumerator ShowBirthdaySection()
    {
        nameSection.SetActive(false);

        welcomeText.text = "";
        string transition = "and your date of birth?";
        foreach (char c in transition)
        {
            welcomeText.text += c;
            yield return new WaitForSeconds(0.05f);
        }

        // 打字機結束後直接淡入輸入框（不清空文字）
        yield return StartCoroutine(FadeIn(birthdaySection));
    }

    public void ClearWelcomeText()
    {
        welcomeText.text = "";
    }

    IEnumerator FadeIn(GameObject obj)
    {
        obj.SetActive(true);
        CanvasGroup cg = obj.GetComponent<CanvasGroup>();
        if (cg == null) cg = obj.AddComponent<CanvasGroup>();
        cg.alpha = 0f;

        float elapsed = 0f;
        while (elapsed < fadeInDuration)
        {
            elapsed += Time.deltaTime;
            cg.alpha = elapsed / fadeInDuration;
            yield return null;
        }
        cg.alpha = 1f;

        // 自動 focus 輸入框
        TMP_InputField input = obj.GetComponentInChildren<TMP_InputField>();
        if (input != null)
        {
            input.ActivateInputField();
            input.Select();
        }
    }
}