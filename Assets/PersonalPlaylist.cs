using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using UnityEngine.InputSystem;

public class PersonalPlaylist : MonoBehaviour
{
    public static PersonalPlaylist Instance;

    [Header("歌單")]
    public AudioClip[] tracks;
    public string[] trackNames;
    public Sprite[] albumCovers; // 對應每首歌的封面，可以不填（會隱藏）

    [Header("播放器 UI")]
    public GameObject playerUI;
    public TextMeshProUGUI trackNameText;
    public Button nextButton;
    public Button prevButton;
    public Button pauseButton;
    public TextMeshProUGUI pauseButtonText;
    public Image albumCoverImage; // 封面圖片元件

    [Header("音量")]
    [Range(0f, 1f)] public float volume = 0.8f;

    [Header("顯示設定")]
    public float holdDuration = 0.3f;  // 按住多久才算「長按」顯示

    private bool isHolding = false;
    private float holdTimer = 0f;
    private bool uiVisible = false;

    private AudioSource audioSource;
    private int currentTrackIndex = 0;
    private bool isActive = false;
    private bool isPausedByGame = false;
    private bool isPausedByUser = false;
    private float pausedTime = 0f;
    private Vector2 originalPosition;
    private bool wasActiveBeforeYield = false;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        audioSource = gameObject.AddComponent<AudioSource>();
        audioSource.loop = false;
        audioSource.playOnAwake = false;
        audioSource.volume = volume;

    }

    void Start()
    {
        if (playerUI != null) playerUI.SetActive(false);
        if (nextButton != null) nextButton.onClick.AddListener(NextTrack);
        if (prevButton != null) prevButton.onClick.AddListener(PreviousTrack);
        if (pauseButton != null) pauseButton.onClick.AddListener(TogglePause);

        UnityEngine.SceneManagement.SceneManager.sceneLoaded += OnSceneLoaded;

    }

    void OnSceneLoaded(UnityEngine.SceneManagement.Scene scene, UnityEngine.SceneManagement.LoadSceneMode mode)
    {
        string[] hideScenes = { "SampleScene", "Credits", "CharacterSelect" };
        bool isHideScene = false;

        foreach (string s in hideScenes)
        {
            if (scene.name == s)
            {
                isHideScene = true;
                break;
            }
        }

        if (isHideScene)
        {
            if (isActive || wasActiveBeforeYield)
                YieldToMusicManager(scene.name);
            return;
        }

        // 非隱藏場景
        if (wasActiveBeforeYield)
        {
            // 從 yield 狀態恢復
            RecoverFromYield();
        }
        else if (!isActive && PlayerPrefs.GetInt("MorganPlaylistEnabled", 0) == 1)
        {
            // 重開遊戲後第一次進非隱藏場景才啟動
            StartPlaylist();
        }
    }

    void OnDestroy()
    {
        UnityEngine.SceneManagement.SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    void Update()
    {
        if (!isActive) return;

        // 偵測 Tab 長按
        if (Keyboard.current.tabKey.isPressed)
        {
            holdTimer += Time.deltaTime;

            if (!uiVisible && holdTimer >= holdDuration)
            {
                uiVisible = true;
                if (playerUI != null) playerUI.SetActive(true);
            }
        }
        else
        {
            // 放開 Tab → 隱藏
            if (uiVisible)
            {
                uiVisible = false;
                if (playerUI != null) playerUI.SetActive(false);
            }
            holdTimer = 0f;
        }

        // 自動播下一首
        if (!isPausedByGame && !isPausedByUser &&
            audioSource != null && !audioSource.isPlaying)
        {
            NextTrack();
        }
    }

    // ── 對外介面 ──────────────────────────────────────────

    public void StartPlaylist()
    {
        isActive = true;
        currentTrackIndex = 0;
        isPausedByUser = false;
        uiVisible = false;
        if (playerUI != null) playerUI.SetActive(false);

        if (MusicManager.Instance != null)
            MusicManager.Instance.FadeOutForPlaylist();

        PlayCurrentTrack();
    }

    public void TogglePause()
    {
        if (!isActive) return;

        if (isPausedByUser)
        {
            audioSource.Play();
            isPausedByUser = false;
        }
        else
        {
            audioSource.Pause();
            isPausedByUser = true;
        }

    }

    public void PauseForGame()
    {
        if (!isActive || isPausedByGame) return;
        pausedTime = audioSource.time;
        audioSource.Pause();
        isPausedByGame = true;
    }

    public void ResumeAfterGame()
    {
        if (!isActive || !isPausedByGame) return;

        if (MusicManager.Instance != null)
            MusicManager.Instance.FadeOutForPlaylist();

        isPausedByGame = false;

        if (!isPausedByUser)
        {
            audioSource.time = pausedTime;
            audioSource.Play();
        }
    }

    public void NextTrack()
    {
        currentTrackIndex = (currentTrackIndex + 1) % tracks.Length;
        isPausedByUser = false;
        PlayCurrentTrack();
    }

    public void PreviousTrack()
    {
        if (audioSource.time > 3f)
        {
            audioSource.time = 0f;
            UpdateTrackNameUI();
            return;
        }

        currentTrackIndex = (currentTrackIndex - 1 + tracks.Length) % tracks.Length;
        isPausedByUser = false;
        PlayCurrentTrack();
    }

    public bool IsActive() => isActive;

    // ── 內部 ──────────────────────────────────────────────

    void PlayCurrentTrack()
    {
        if (tracks == null || tracks.Length == 0) return;
        if (currentTrackIndex >= tracks.Length) return;

        audioSource.clip = tracks[currentTrackIndex];
        audioSource.time = 0f;
        audioSource.Play();

        UpdateTrackNameUI();
        UpdateAlbumCover();
    }

    void UpdateTrackNameUI()
    {
        if (trackNameText == null) return;
        string name = (trackNames != null && currentTrackIndex < trackNames.Length)
            ? trackNames[currentTrackIndex]
            : tracks[currentTrackIndex].name;

        // 如果有掛 MarqueeText 就用它設文字，否則直接設
        MarqueeText marquee = trackNameText.GetComponent<MarqueeText>();
        if (marquee != null)
            marquee.SetText("♪ " + name);
        else
            trackNameText.text = "♪ " + name;
    }

    void UpdateAlbumCover()
    {
        if (albumCoverImage == null) return;

        if (albumCovers != null && currentTrackIndex < albumCovers.Length
            && albumCovers[currentTrackIndex] != null)
        {
            albumCoverImage.sprite = albumCovers[currentTrackIndex];
            albumCoverImage.gameObject.SetActive(true);
        }
        else
        {
            // 這首沒有封面就隱藏圖片元件
            albumCoverImage.gameObject.SetActive(false);
        }
    }

    public void SetVolume(float volume)
    {
        if (audioSource != null)
            audioSource.volume = volume;
        this.volume = volume;
    }

    public void YieldToMusicManager(string sceneKey)
    {
        if (!isActive) return;

        if (playerUI != null) playerUI.SetActive(false);

        wasActiveBeforeYield = true;
        isActive = false;

        // 用 Coroutine 淡出再暫停
        StartCoroutine(FadeOutAndYield(sceneKey));
    }

    public void RecoverFromYield()
    {
        if (!wasActiveBeforeYield) return;

        wasActiveBeforeYield = false;
        isActive = true;

        if (MusicManager.Instance != null)
            MusicManager.Instance.FadeOutForPlaylist();

        if (!isPausedByUser)
        {
            audioSource.time = pausedTime;
            audioSource.Play();
        }
        isPausedByGame = false;
    }

    IEnumerator FadeOutAndYield(string sceneKey)
    {
        float elapsed = 0f;
        float fadeDuration = 1.5f;
        float startVolume = audioSource.volume;

        while (elapsed < fadeDuration)
        {
            elapsed += Time.deltaTime;
            audioSource.volume = Mathf.Lerp(startVolume, 0f, elapsed / fadeDuration);
            yield return null;
        }

        audioSource.volume = 0f;
        pausedTime = audioSource.time;
        audioSource.Pause();
        isPausedByGame = true;

        // 淡出完才讓 MusicManager 接管
        audioSource.volume = volume; // 恢復音量設定，下次播放才正常
        if (MusicManager.Instance != null)
            MusicManager.Instance.PlayMusicByKey(sceneKey);
    }
}