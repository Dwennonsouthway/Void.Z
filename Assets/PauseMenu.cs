using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem;

public class PauseMenu : MonoBehaviour
{
    public GameObject pauseMenuUI;
    private bool isPaused = false;

    void Update()
    {
        int sessionCount = PlayerPrefs.GetInt("SessionCount", 0);

        if (Keyboard.current.escapeKey.wasPressedThisFrame)
        {
            if (sessionCount == 0)
            {
                // 第一次：正常暫停
                if (isPaused) Resume();
                else Pause();
            }
            else
            {
                // 第二次之後：沒反應
                StartCoroutine(EscapeResponse());
            }
        }
    }

    public void Resume()
    {
        pauseMenuUI.SetActive(false);
        Time.timeScale = 1f;
        isPaused = false;
    }

    public void Pause()
    {
        pauseMenuUI.SetActive(true);
        Time.timeScale = 0f;
        isPaused = true;
    }

    public void QuitToMenu()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene("SampleScene");
    }

    System.Collections.IEnumerator EscapeResponse()
    {
        // 第二次按 Escape 沒反應，之後可以加詭異反應
        yield return null;
    }
}