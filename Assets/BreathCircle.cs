using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;
using TMPro;
public class BreathCircle : MonoBehaviour
{
    [Header("呼吸設定")]
    public float minScale = 1.5f;
    public float maxScale = 3f;
    public float inhaleTime = 4f;
    public float exhaleTime = 4f;

    [Header("顏色")]
    public Color normalColor = new Color(0f, 0.96f, 1f, 0.6f);
    public Color glitchColor = new Color(1f, 0.2f, 0.2f, 0.6f);

    [Header("粒子")]
    public ParticleSystem breathParticles;

    private SpriteRenderer sr;
    private bool isBreathing = false;
    private bool isInhaling = true;
    private bool isGlitching = false;
    private float timer = 0f;

    public bool breathedCorrectly = false;
    private float noBreathTimer = 0f;
    private float noBreathThreshold = 8f;
    private int noBreathCount = 0;

    public float ringScale = 1f; // 光環的 Scale，跟 BreathRing 的 Scale 一樣
    public float tolerance = 0.3f; // 判定範圍
    public SpriteRenderer breathRingOuter; // 外光環
    public SpriteRenderer breathRingInner; // 內光環
    public TextMeshProUGUI breathFeedbackText;
    private bool countingNoBreath = false;
    void Awake()
    {
        sr = GetComponent<SpriteRenderer>();
    }

    void Update()
    {
        if (!isBreathing) return;

        timer += Time.deltaTime;
        float cycleDuration = isInhaling ? inhaleTime : exhaleTime;
        float t = timer / cycleDuration;

        float scale = isInhaling
            ? Mathf.Lerp(minScale, maxScale, t)
            : Mathf.Lerp(maxScale, minScale, t);
        transform.localScale = new Vector3(scale, scale, 1f);

        bool nearOuter = isInhaling && Mathf.Abs(scale - ringScale) < tolerance;
        bool nearInner = !isInhaling && Mathf.Abs(scale - minScale) < tolerance;
        bool nearRing = nearOuter || nearInner;

        if (nearRing)
        {
            float pulse = Mathf.Sin(Time.time * 30f) * 0.5f + 0.5f;

            if (nearOuter && breathRingOuter != null)
                breathRingOuter.color = new Color(0.7f, 0f, 1f, pulse);

            if (nearInner && breathRingInner != null)
                breathRingInner.color = new Color(0.7f, 0f, 1f, pulse);

        }
        else
        {
            if (breathRingOuter != null)
                breathRingOuter.color = new Color(1f, 1f, 1f, 0.3f);

            if (breathRingInner != null)
                breathRingInner.color = new Color(1f, 1f, 1f, 0.3f);

            if (sr != null && !isGlitching)
                sr.color = normalColor;

        }

        if (timer >= cycleDuration)
        {
            timer = 0f;
            isInhaling = !isInhaling;
        }

        if (countingNoBreath && !Keyboard.current.spaceKey.wasPressedThisFrame && !isGlitching)
        {
            noBreathTimer += Time.deltaTime;
            if (noBreathTimer >= noBreathThreshold)
            {
                noBreathTimer = 0f;
                StartCoroutine(NoBreathComment());
            }
        }

        if (Keyboard.current.spaceKey.wasPressedThisFrame && !isGlitching)
        {
            noBreathTimer = 0f;
            if (nearRing)
                StartCoroutine(GoodBreath());
            else
                StartCoroutine(BadBreath());
        }
    }
    public void StartBreathing()
    {
        sr = GetComponent<SpriteRenderer>();
        isBreathing = true;
        isGlitching = false;
        gameObject.SetActive(true);

        if (sr != null)
            sr.color = normalColor;
        if (breathRingOuter != null)
            breathRingOuter.gameObject.SetActive(true);
        if (breathRingInner != null)
            breathRingInner.gameObject.SetActive(true);

    }

    public void StartCounting()
    {
        countingNoBreath = true;
        noBreathTimer = 0f;
        noBreathCount = 0;
    }

    public void StopBreathing()
    {
        isBreathing = false;
        gameObject.SetActive(false);
        if (breathRingOuter != null)
            breathRingOuter.gameObject.SetActive(false);
        if (breathRingInner != null)
            breathRingInner.gameObject.SetActive(false);

    }

    public void StartGlitch()
    {
        isGlitching = true;
        noBreathTimer = 0f;
        StartCoroutine(GlitchLoop());
    }

    IEnumerator GoodBreath()
    {
        breathedCorrectly = true;
        Color bright = new Color(normalColor.r, normalColor.g, normalColor.b, 1f);
        sr.color = bright;

        if (breathParticles != null)
            breathParticles.Emit(10);

        yield return new WaitForSeconds(0.2f);
        sr.color = normalColor;
    }

    IEnumerator BadBreath()
    {
        Color original = sr.color;
        string[] badLines = {
        "not yet.",
        "wait for it.",
        "you're rushing.",
        "feel the rhythm.",
        "...focus.",
        "that wasn't right.",
    };
        if (breathFeedbackText != null)
            breathFeedbackText.text = badLines[Random.Range(0, badLines.Length)];

        for (int i = 0; i < 4; i++)
        {
            sr.color = new Color(1f, 0f, 0.5f, 0.8f);
            transform.localScale *= Random.Range(0.9f, 1.1f);
            yield return new WaitForSeconds(0.05f);
            sr.color = original;
            yield return new WaitForSeconds(0.05f);
        }

        sr.color = original;
        yield return new WaitForSeconds(0.5f);
        if (breathFeedbackText != null)
            breathFeedbackText.text = "";

        transform.localScale = new Vector3(
            Mathf.Lerp(minScale, maxScale, timer / (isInhaling ? inhaleTime : exhaleTime)),
            Mathf.Lerp(minScale, maxScale, timer / (isInhaling ? inhaleTime : exhaleTime)),
            1f
        );
    }
    IEnumerator NoBreathComment()
    {
        noBreathCount++;

        string[] lines;

        if (noBreathCount == 1)
        {
            lines = new string[] {
            "breathe.",
            "are you still there?",
            "don't forget to breathe.",
            "follow my rhythm.",
        };
        }
        else if (noBreathCount == 2)
        {
            lines = new string[] {
            "[name].",
            "i need you to breathe with me.",
            "please.",
            "you're not trying.",
        };
        }
        else
        {
            lines = new string[] {
            "fine.",
            "suit yourself.",
            "i'll wait.",
            "...",
        };
        }

        string playerName = PlayerPrefs.GetString("PlayerName", "...");
        string line = lines[Random.Range(0, lines.Length)].Replace("[name]", playerName);

        if (breathFeedbackText != null)
        {
            breathFeedbackText.text = line;
            yield return new WaitForSeconds(2f);
            breathFeedbackText.text = "";
        }
    }

    IEnumerator GlitchLoop()
    {
        while (isGlitching)
        {
            sr.color = Color.Lerp(normalColor, glitchColor, Random.Range(0f, 1f));
            float randomScale = Random.Range(minScale, maxScale);
            transform.localScale = new Vector3(randomScale, randomScale, 1f);
            yield return new WaitForSeconds(Random.Range(0.05f, 0.2f));
        }
    }

    public bool HasBreathedCorrectly()
    {
        return breathedCorrectly;
    }

}