using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;
using UnityEngine.UI;
using System.Diagnostics;
using System.Collections;

public class MenuController : MonoBehaviour
{
    [Header("一般 UI")]
    public TextMeshProUGUI btnStartText;
    public TextMeshProUGUI btnSettingsText;
    public GameObject mainUI;

    [Header("結局 D 專用")]
    public GameObject endingDPanel;
    public TextMeshProUGUI endingDText;

    [Header("Glitch 效果")]
    public CanvasGroup canvasGroup;

    private bool alertShown = false;

    void Start()
    {
        if (PlayerPrefs.GetInt("EndingD", 0) == 1)
        {
            mainUI.SetActive(false);
            endingDPanel.SetActive(true);
            endingDText.text = "";
            StartCoroutine(EndingDScreen());
            return;
        }

        if (PlayerPrefs.GetInt("WasConsumed", 0) == 1)
        {
            btnStartText.text = "return to us.";
            btnSettingsText.text = "[CORRUPTED]";
            StartCoroutine(GlitchLoop());
            return;
        }

        int sessionCount = PlayerPrefs.GetInt("SessionCount", 0);

        if (sessionCount == 1)
        {
            btnStartText.text = "RETURN TO VOID";
            btnSettingsText.text = "SETTINGS [CORRUPTED]";
        }
        else if (sessionCount >= 2)
        {
            btnStartText.text = "you know you want to.";
            btnSettingsText.text = "[ERROR]";
            if (PlayerPrefs.GetInt("EntityDeleted", 0) == 0)
                StartCoroutine(ShowErrorAfterDelay());
        }
    }

    void Update()
    {
#if UNITY_EDITOR
        if (UnityEngine.InputSystem.Keyboard.current.backquoteKey.wasPressedThisFrame)
        {
            PlayerPrefs.DeleteAll();
            PlayerPrefs.Save();

            string lockPath = System.IO.Path.Combine(Application.dataPath, "..", "deleted.lock");
            if (System.IO.File.Exists(lockPath))
                System.IO.File.Delete(lockPath);

            SceneTransition.Instance.GoToScene("SampleScene");
            UnityEngine.Debug.Log("Reset!");
        }
#endif
    }

    IEnumerator EndingDScreen()
    {
        yield return new WaitForSeconds(2f);

        string message = "there is no escape now.";
        foreach (char c in message)
        {
            endingDText.text += c;
            yield return new WaitForSeconds(0.08f);
        }

        yield return new WaitForSeconds(3f);
        endingDText.text += "\n\nyou are soul #999.";
        yield return new WaitForSeconds(3f);
        endingDText.text += "\n\nthank you for staying.";
    }

    IEnumerator GlitchLoop()
    {
        if (canvasGroup == null) yield break;

        for (int i = 0; i < 8; i++)
        {
            canvasGroup.alpha = Random.Range(0.1f, 0.4f);
            yield return new WaitForSeconds(Random.Range(0.05f, 0.15f));
            canvasGroup.alpha = 1f;
            yield return new WaitForSeconds(Random.Range(0.05f, 0.2f));
        }

        while (true)
        {
            yield return new WaitForSeconds(Random.Range(2f, 6f));

            int flickers = Random.Range(2, 6);
            for (int i = 0; i < flickers; i++)
            {
                canvasGroup.alpha = Random.Range(0f, 0.5f);
                yield return new WaitForSeconds(Random.Range(0.03f, 0.1f));
                canvasGroup.alpha = 1f;
                yield return new WaitForSeconds(Random.Range(0.03f, 0.1f));
            }

            string[] glitchTexts = {
                "return to us.",
                "r̷e̷t̷u̷r̷n̷ ̷t̷o̷ ̷u̷s̷.",
                "you can't leave.",
                "we are waiting.",
                "JOIN US",
            };
            btnStartText.text = glitchTexts[Random.Range(0, glitchTexts.Length)];
        }
    }

    IEnumerator ShowErrorAfterDelay()
    {
        yield return new WaitForSeconds(3f);
        if (!alertShown)
        {
            alertShown = true;
#if UNITY_STANDALONE_OSX
            ShowMacAlert();
#elif UNITY_STANDALONE_WIN
            ShowWindowsAlert();
#endif
        }
    }

    void ShowMacAlert()
    {
        Process process = new Process();
        process.StartInfo.FileName = "osascript";
        process.StartInfo.Arguments = "-e 'display alert \"\\\"meditation_daemon\\\" quit unexpectedly.\" buttons {\"Report...\", \"OK\"} default button \"OK\"'";
        process.StartInfo.UseShellExecute = false;
        process.StartInfo.RedirectStandardOutput = true;
        process.Start();

        string result = process.StandardOutput.ReadToEnd();
        process.WaitForExit();

        if (result.Contains("OK"))
            SceneTransition.Instance.GoToScene("TerminalScene");
    }

    void ShowWindowsAlert()
    {
        Process process = new Process();
        process.StartInfo.FileName = "powershell";
        process.StartInfo.Arguments = "-Command \"Add-Type -AssemblyName PresentationFramework; $result = [System.Windows.MessageBox]::Show('meditation_daemon has stopped responding.', 'System Error', 'OKCancel', 'Error'); Write-Output $result\"";
        process.StartInfo.UseShellExecute = false;
        process.StartInfo.RedirectStandardOutput = true;
        process.StartInfo.CreateNoWindow = true;
        process.Start();

        string result = process.StandardOutput.ReadToEnd();
        process.WaitForExit();

        if (result.Contains("OK"))
            SceneTransition.Instance.GoToScene("TerminalScene");
    }

    public void StartGame()
    {
        int sessionCount   = PlayerPrefs.GetInt("SessionCount", 0);
        int entityDeleted  = PlayerPrefs.GetInt("EntityDeleted", 0);
        int entityPurified = PlayerPrefs.GetInt("EntityPurified", 0);
        int wasConsumed    = PlayerPrefs.GetInt("WasConsumed", 0);

        if (entityDeleted == 1 || entityPurified == 1 || wasConsumed == 1)
        {
            SceneTransition.Instance.GoToScene("VoidSpace");
            return;
        }

        if (sessionCount == 0)
            SceneTransition.Instance.GoToScene("CharacterSelect");
        else
            SceneTransition.Instance.GoToScene("VoidSpace");
    }

    public void QuitGame() => Application.Quit();
}
