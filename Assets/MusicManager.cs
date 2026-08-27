using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;
using System.Collections.Generic;

public class MusicManager : MonoBehaviour
{
    public static MusicManager Instance;

    [System.Serializable]
    public class SceneMusic
    {
        public string sceneName;
        public AudioClip clip;
        [Range(0f, 1f)] public float volume = 1f;
    }

    [Header("場景對應音樂清單")]
    public List<SceneMusic> sceneMusicList;

    [Header("Cross-fade 設定")]
    public float fadeDuration = 2f;

    private AudioSource sourceA;
    private AudioSource sourceB;
    private AudioSource activeSource;
    private AudioSource inactiveSource;
    private Coroutine fadeCoroutine;
    private AudioSource sfxSource;
    [Header("音效")]
    public AudioClip glitchSound;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        sourceA = gameObject.AddComponent<AudioSource>();
        sourceB = gameObject.AddComponent<AudioSource>();
        sourceA.loop = true;
        sourceB.loop = true;
        sourceA.playOnAwake = false;
        sourceB.playOnAwake = false;

        activeSource = sourceA;
        inactiveSource = sourceB;

        SceneManager.sceneLoaded += OnSceneLoaded;

        sfxSource = gameObject.AddComponent<AudioSource>();
        sfxSource.loop = false;
        sfxSource.playOnAwake = false;
    }

    void Start()
    {
        PlayMusicForScene(SceneManager.GetActiveScene().name, fadeIn: false);
    }

    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (scene.name == "VoidSpace") return;

        PlayMusicForScene(scene.name, fadeIn: true);
    }

    /// <summary>
    /// 給場景內互動使用（例如按 E 進入冥想），用「自訂 key」而非場景名稱切換音樂。
    /// key 對應 sceneMusicList 裡的 sceneName 欄位（可以填非場景名稱的自訂標籤）。
    /// </summary>
    public void PlayMusicByKey(string key)
    {
        PlayMusicForScene(key, fadeIn: true);
    }

    void PlayMusicForScene(string sceneName, bool fadeIn)
    {
        Debug.Log("PlayMusicForScene: " + sceneName + ", fadeIn: " + fadeIn);

        if (PersonalPlaylist.Instance != null && PersonalPlaylist.Instance.IsActive())
        {
            Debug.Log("Playlist active, skipping");
            return;
        }
        // 歌單啟用中，場景音樂不自動接管
        if (PersonalPlaylist.Instance != null && PersonalPlaylist.Instance.IsActive())
            return;

        SceneMusic target = sceneMusicList.Find(s => s.sceneName == sceneName);

        if (target == null)
        {
            if (fadeCoroutine != null) StopCoroutine(fadeCoroutine);
            fadeCoroutine = StartCoroutine(FadeOutCurrent());
            return;
        }

        if (activeSource.clip == target.clip && activeSource.isPlaying)
            return;

        if (fadeCoroutine != null) StopCoroutine(fadeCoroutine);

        if (fadeIn)
        {
            fadeCoroutine = StartCoroutine(CrossFadeTo(target));
        }
        else
        {
            activeSource.clip = target.clip;
            activeSource.volume = target.volume;
            activeSource.Play();
        }
    }

    IEnumerator CrossFadeTo(SceneMusic target)
    {
        inactiveSource.clip = target.clip;
        inactiveSource.volume = 0f;
        inactiveSource.Play();

        float elapsed = 0f;
        float startVolumeActive = activeSource.volume;
        float targetVolumeInactive = target.volume;

        while (elapsed < fadeDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / fadeDuration;

            activeSource.volume = Mathf.Lerp(startVolumeActive, 0f, t);
            inactiveSource.volume = Mathf.Lerp(0f, targetVolumeInactive, t);

            yield return null;
        }

        activeSource.Stop();
        activeSource.volume = 0f;

        AudioSource temp = activeSource;
        activeSource = inactiveSource;
        inactiveSource = temp;
    }

    IEnumerator FadeOutCurrent()
    {
        float elapsed = 0f;
        float startVolume = activeSource.volume;

        while (elapsed < fadeDuration)
        {
            elapsed += Time.deltaTime;
            activeSource.volume = Mathf.Lerp(startVolume, 0f, elapsed / fadeDuration);
            yield return null;
        }

        activeSource.Stop();
        activeSource.volume = 0f;
    }

    /// <summary>
    /// 給 PersonalPlaylist 呼叫，把目前背景音樂淡出並停止，
    /// 讓歌單接管音訊輸出。
    /// </summary>
    public void FadeOutForPlaylist()
    {
        if (fadeCoroutine != null) StopCoroutine(fadeCoroutine);
        fadeCoroutine = StartCoroutine(FadeOutCurrent());
    }

    void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    public void PlaySFX(AudioClip clip)
    {
        if (clip == null || sfxSource == null) return;
        sfxSource.PlayOneShot(clip);
    }

    public void SetVolume(float bgmVolume, float sfxVolume)
    {
        if (activeSource != null)
            activeSource.volume = bgmVolume;
        if (inactiveSource != null)
            inactiveSource.volume = bgmVolume;
        if (sfxSource != null)
            sfxSource.volume = sfxVolume;
    }

    public void SetBGMVolume(float volume)
    {
        if (activeSource != null) activeSource.volume = volume;
        if (inactiveSource != null) inactiveSource.volume = volume;
    }
}