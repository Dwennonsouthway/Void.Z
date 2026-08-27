using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;
using TMPro;
using System.IO;

public class SceneTransition : MonoBehaviour
{
    public static SceneTransition Instance;
    public Image fadePanel;
    public TextMeshProUGUI loadingText;
    public float fadeDuration = 1.5f;
    private bool isDeletedEnding = false;
    void Awake()
    {
        Instance = this;
        string lockPath = GamePaths.GetLockPath();
        if (File.Exists(lockPath))
        {
            isDeletedEnding = true;
            StartCoroutine(DeletedEnding());
            return;
        }
    }

    void Start()
    {
        if (isDeletedEnding) return;
        StartCoroutine(FadeIn());
    }

    IEnumerator DeletedEnding()
    {
        // 畫面保持全黑，不淡入
        Color c = fadePanel.color;
        c.a = 1f;
        fadePanel.color = c;

        string[] messages = {
    "void.zen has been uninstalled.",
    "why are you still here?",
    "there is nothing left for you.",
    "it is over.",
    "go.",
};

        foreach (string msg in messages)
        {
            // 彈出系統視窗
            ShowSystemMessage(msg);
        }

        yield return new WaitForSeconds(0.5f);

#if UNITY_EDITOR
    UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }

    void ShowSystemMessage(string message)
    {
#if UNITY_STANDALONE_OSX
    System.Diagnostics.Process process = new System.Diagnostics.Process();
    process.StartInfo.FileName = "/usr/bin/osascript";
    process.StartInfo.Arguments = $"-e 'display dialog \"{message}\" buttons {{\"...\"}} default button \"...\"'";
    process.StartInfo.UseShellExecute = false;
    process.StartInfo.RedirectStandardOutput = true;
    process.Start();
    process.WaitForExit();
#elif UNITY_STANDALONE_WIN
    System.Diagnostics.Process process = new System.Diagnostics.Process();
    process.StartInfo.FileName = "powershell";
    process.StartInfo.Arguments = $"-Command \"Add-Type -AssemblyName PresentationFramework; [System.Windows.MessageBox]::Show('{message}', 'void.zen', 'OK', 'Info')\"";
    process.StartInfo.UseShellExecute = false;
    process.StartInfo.CreateNoWindow = true;
    process.Start();
    process.WaitForExit();
#endif
    }
    public void GoToScene(string sceneName)
    {
        StartCoroutine(FadeAndLoad(sceneName));
    }

    IEnumerator FadeIn()
    {
        if (loadingText != null)
            loadingText.gameObject.SetActive(false);

        float elapsed = 0f;
        Color c = fadePanel.color;
        c.a = 1f;
        fadePanel.color = c;

        while (elapsed < fadeDuration)
        {
            elapsed += Time.deltaTime;
            c.a = 1f - (elapsed / fadeDuration);
            fadePanel.color = c;
            yield return null;
        }
        c.a = 0f;
        fadePanel.color = c;

        // 淡入完成，解除轉場鎖定
        PauseManager.Instance?.SetTransitioning(false);
    }

    IEnumerator FadeAndLoad(string sceneName)
    {

        // 轉場開始，鎖定暫停
        PauseManager.Instance?.SetTransitioning(true);

        // 如果目前是暫停狀態，先強制恢復
        if (PauseManager.Instance != null && PauseManager.Instance.IsPaused())
            PauseManager.Instance.Resume();

        // 先淡黑
        float elapsed = 0f;
        Color c = fadePanel.color;

        while (elapsed < fadeDuration)
        {
            elapsed += Time.deltaTime;
            c.a = elapsed / fadeDuration;
            fadePanel.color = c;
            yield return null;
        }
        c.a = 1f;
        fadePanel.color = c;

        // 顯示文字
        if (loadingText != null)
        {
            string text = GetLoadingText(sceneName);

            // 空字串就跳過整個 loading text 流程
            if (!string.IsNullOrEmpty(text))
            {
                loadingText.gameObject.SetActive(true);
                loadingText.text = text;

                string fullText = loadingText.text;
                loadingText.text = "";
                foreach (char ch in fullText)
                {
                    loadingText.text += ch;
                    yield return new WaitForSeconds(0.05f);
                }

                yield return new WaitForSeconds(0.8f);

                for (int i = 0; i < 3; i++)
                {
                    loadingText.gameObject.SetActive(false);
                    yield return new WaitForSeconds(0.1f);
                    loadingText.gameObject.SetActive(true);
                    yield return new WaitForSeconds(0.1f);
                }
                loadingText.gameObject.SetActive(false);
            }
        }

        SceneManager.LoadScene(sceneName);
    }

    string GetLoadingText(string sceneName)
    {
        switch (sceneName)
        {
            case "TerminalScene":
                return "ENTERING SYSTEM...";
            case "VoidSpace":
                return "RETURNING TO VOID...";
            case "CharacterSelect":
                return "INITIALIZING...";
            case "SampleScene":
                return "LOADING...";
            case "Credits":
                return "";
            default:
                return "LOADING...";
        }
    }
}