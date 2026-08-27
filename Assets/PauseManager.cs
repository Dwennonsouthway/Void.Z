using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.InputSystem;

public class PauseManager : MonoBehaviour
{
    public static PauseManager Instance;

    [Header("暫停 UI")]
    public GameObject pausePanel;

    [Header("音量設定")]
    public Slider bgmSlider;
    public Button muteButton;
    public Image muteButtonImage;  // 換成 Image
    public Sprite iconSound;       // 🔊 sprite
    public Sprite iconMute;        // 🔇 sprite

    [Header("不允許暫停的場景")]
    public string[] unpausableScenes = { "Credits", "SampleScene", "CharacterSelect" };

    private bool isPaused = false;
    private bool isMuted = false;
    private float lastVolume = 0.8f;
    private string currentScene = "";
    private bool isTransitioning = false;

    public void SetTransitioning(bool value)
    {
        isTransitioning = value;
    }
    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    void Start()
    {
        if (pausePanel != null)
            pausePanel.SetActive(false);

        // 載入儲存設定
        lastVolume = PlayerPrefs.GetFloat("BGMVolume", 0.8f);
        isMuted = PlayerPrefs.GetInt("BGMMuted", 0) == 1;

        if (bgmSlider != null)
        {
            bgmSlider.value = lastVolume;
            bgmSlider.onValueChanged.AddListener(OnSliderChanged);
        }

        if (muteButton != null)
            muteButton.onClick.AddListener(ToggleMute);

        UpdateMuteUI();
        ApplyVolume();

        UnityEngine.SceneManagement.SceneManager.sceneLoaded += OnSceneLoaded;
    }

    void OnSceneLoaded(UnityEngine.SceneManagement.Scene scene, UnityEngine.SceneManagement.LoadSceneMode mode)
    {
        currentScene = scene.name;
        if (isPaused) Resume();
    }

    void Update()
    {
        if (Keyboard.current == null) return;
        if (isTransitioning) return;  // 轉場中不允許暫停

        if (Keyboard.current.escapeKey.wasPressedThisFrame)
        {
            foreach (string s in unpausableScenes)
                if (currentScene == s) return;

            if (isPaused) Resume();
            else Pause();
        }
    }

    // ── 暫停 / 繼續 ─────────────────────────────────────────

    public void Pause()
    {
        isPaused = true;
        Time.timeScale = 0f;
        if (pausePanel != null) pausePanel.SetActive(true);
    }

    public void Resume()
    {
        isPaused = false;
        Time.timeScale = 1f;
        if (pausePanel != null) pausePanel.SetActive(false);
    }

    public void GoToMainMenu()
    {
        Resume();
        SceneTransition.Instance.GoToScene("SampleScene");
    }

    public bool IsPaused() => isPaused;

    // ── 音量控制 ─────────────────────────────────────────────

    void OnSliderChanged(float value)
    {
        lastVolume = value;
        PlayerPrefs.SetFloat("BGMVolume", value);

        // 拖 slider 時如果是靜音狀態，自動取消靜音
        if (isMuted && value > 0f)
        {
            isMuted = false;
            PlayerPrefs.SetInt("BGMMuted", 0);
            UpdateMuteUI();
        }

        ApplyVolume();
    }

    public void ToggleMute()
    {
        isMuted = !isMuted;
        PlayerPrefs.SetInt("BGMMuted", isMuted ? 1 : 0);

        // 靜音時把 slider 視覺上歸零，取消靜音時恢復
        if (bgmSlider != null)
            bgmSlider.SetValueWithoutNotify(isMuted ? 0f : lastVolume);

        UpdateMuteUI();
        ApplyVolume();
    }

    void UpdateMuteUI()
    {
        if (muteButtonImage != null)
            muteButtonImage.sprite = isMuted ? iconMute : iconSound;
    }

    void ApplyVolume()
    {
        float vol = isMuted ? 0f : lastVolume;
        if (MusicManager.Instance != null)
            MusicManager.Instance.SetBGMVolume(vol);
        if (PersonalPlaylist.Instance != null)
            PersonalPlaylist.Instance.SetVolume(vol);
    }

    void OnDestroy()
    {
        UnityEngine.SceneManagement.SceneManager.sceneLoaded -= OnSceneLoaded;
    }
}