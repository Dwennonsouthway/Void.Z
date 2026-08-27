using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem;
using TMPro;
using System.Collections;

public class NameInputController : MonoBehaviour
{
    public TMP_InputField nameInputField;
    public TextMeshProUGUI analysisText;
    public GameObject birthdaySection;

    private string[] traits = {
        "a natural leader. independent. you prefer to walk alone.",
        "deeply sensitive. you feel what others cannot.",
        "creative and expressive. but easily distracted.",
        "disciplined. methodical. you build walls without realizing.",
        "restless. always searching. never quite arriving.",
        "a caretaker. you give more than you receive.",
        "a seeker of truth. often misunderstood.",
        "driven by power. afraid of losing control.",
        "old soul. you carry grief that isn't yours."
    };

    void Update()
    {
        if (Keyboard.current.enterKey.wasPressedThisFrame ||
            Keyboard.current.numpadEnterKey.wasPressedThisFrame)
        {
            if (nameInputField.text.Length > 0)
            {
                ConfirmName();
            }
        }
    }

    public void ConfirmName()
    {
        string playerName = nameInputField.text;
        if (playerName.Length > 0)
        {
            PlayerPrefs.SetString("PlayerName", playerName);
            PlayerPrefs.Save();
            WelcomeSequence ws = FindObjectOfType<WelcomeSequence>();
            ws.StartCoroutine(ws.ShowBirthdaySection());
        }
    }
    public void ConfirmBirthday()
    {
        TerminalBirthday tb = FindObjectOfType<TerminalBirthday>();
        string birthday = tb.GetBirthday();
        string playerName = PlayerPrefs.GetString("PlayerName", "");

        GameObject bd = GameObject.Find("BirthdaySection");
        bd.SetActive(false);

        WelcomeSequence ws2 = FindObjectOfType<WelcomeSequence>();
        ws2.ClearWelcomeText();
        ws2.StartCoroutine(AnalysisSequence(playerName, birthday));
    }
    IEnumerator AnalysisSequence(string playerName, string birthday)
    {


        int lifeNumber = CalculateLifeNumber(birthday);
        string trait = traits[lifeNumber - 1];

        string[] lines = {
            playerName + ".",
            "calculating your soul frequency...",
            "analysis complete.",
            trait,
            "we have been looking for someone like you.",
            "let us begin."
        };

        foreach (string line in lines)
        {
            analysisText.text = "";
            foreach (char c in line)
            {
                analysisText.text += c;
                yield return new WaitForSeconds(0.05f);
            }
            yield return new WaitForSeconds(2f);
            analysisText.text = "";
        }

        SceneManager.LoadScene("MeditationScene");
    }

    int CalculateLifeNumber(string birthday)
    {
        int sum = 0;
        foreach (char c in birthday)
        {
            if (char.IsDigit(c))
                sum += int.Parse(c.ToString());
        }

        while (sum > 9)
        {
            int temp = 0;
            while (sum > 0)
            {
                temp += sum % 10;
                sum /= 10;
            }
            sum = temp;
        }

        return sum == 0 ? 9 : sum;
    }
}