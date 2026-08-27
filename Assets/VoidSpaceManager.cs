using UnityEngine;
using System.Collections;
using TMPro;
using UnityEngine.InputSystem;
public class VoidSpaceManager : MonoBehaviour
{
    [Header("Purify 結局")]
    public GameObject entitySprite;
    public TextMeshProUGUI dialogueText;
    public float entityFadeDuration = 3f;
    public Transform purifyStartPosition;
    private TextMeshProUGUI mainDialogue;
    public TextMeshProUGUI promptText;
    public GameObject promptTextObj;

    public GameObject[] purifyEnvironment;
    public GameObject[] normalEnvironment;
    public GameObject[] consumedEnvironment;
    private Coroutine hidePromptCoroutine;
    private Coroutine disableCameraCoroutine;
    public DialogueTrigger purifyDialogueTrigger;


    void Start()
    {
        int purified = PlayerPrefs.GetInt("EntityPurified", 0);
        int deleted = PlayerPrefs.GetInt("EntityDeleted", 0);
        int consumed = PlayerPrefs.GetInt("WasConsumed", 0);

        Debug.Log("VoidSpaceManager Start - purified: " + purified + " deleted: " + deleted + " consumed: " + consumed);

        if (purified == 1)
        {
            MusicManager.Instance?.PlayMusicByKey("VoidSpace_Purify");
            StartCoroutine(PurifyMode());
        }
        else if (deleted == 1)
        {
            MusicManager.Instance?.PlayMusicByKey("VoidSpace_Normal");
            StartCoroutine(DeleteMode());
        }
        else if (consumed == 1)
        {
            MusicManager.Instance?.PlayMusicByKey("VoidSpace_Consumed");
            StartCoroutine(ConsumedMode());
        }
        else
        {
            Debug.Log("Playing VoidSpace_Normal, MusicManager: " + (MusicManager.Instance != null));
            MusicManager.Instance?.PlayMusicByKey("VoidSpace_Normal");
            Debug.Log("PlayMusicByKey called");
        }
    }

    void Update()
    {
#if UNITY_EDITOR
    // ` 鍵：重置收集物 + 切換角色
    if (Keyboard.current != null && Keyboard.current.backquoteKey.wasPressedThisFrame)
    {
        PlayerPrefs.DeleteKey("Collected_bong");
        PlayerPrefs.DeleteKey("Collected_weed");
        PlayerPrefs.DeleteKey("Collected_bowl");
        PlayerPrefs.Save();
        Debug.Log("Reset collectibles");

        int current = PlayerPrefs.GetInt("SelectedCharacter", 0);
        PlayerPrefs.SetInt("SelectedCharacter", current == 0 ? 1 : 0);
        Debug.Log("Switched to character: " + (current == 0 ? "Female" : "Male"));
    }

     // P 鍵：直接進 Purify 模式
    if (Keyboard.current != null && Keyboard.current.pKey.wasPressedThisFrame)
    {
        PlayerPrefs.SetInt("EntityPurified", 1);
        PlayerPrefs.SetInt("EntityDeleted", 0);
        PlayerPrefs.SetInt("WasConsumed", 0);
        PlayerPrefs.Save();
        StopAllCoroutines();
        StartCoroutine(PurifyMode());
        Debug.Log("Purify Mode activated!");
    }

    // c 鍵：重置 ConsumedMode
    if (Keyboard.current != null && Keyboard.current.cKey.wasPressedThisFrame)
    {
        PlayerPrefs.SetInt("WasConsumed", 1);
        PlayerPrefs.SetInt("EntityPurified", 0);
        PlayerPrefs.SetInt("EntityDeleted", 0);
        PlayerPrefs.Save();

        ConsumedShadow shadow = FindObjectOfType<ConsumedShadow>(true);
        if (shadow != null)
        {
            shadow.solved = false;
            shadow.gameObject.SetActive(false);
        }

        StopAllCoroutines();
        StartCoroutine(ConsumedMode());
        Debug.Log("ConsumedMode Reset!");
    }
#endif
    }

    IEnumerator PurifyMode()
    {
        // 鎖住廣場 trigger 的輸入，防止玩家在 ENTITY 說話時按 E
        DialogueTrigger[] allTriggers = FindObjectsOfType<DialogueTrigger>(true);
        foreach (DialogueTrigger trigger in allTriggers)
        {
            trigger.SetInputLocked(true);
            trigger.enabled = false;
        }

        foreach (var obj in normalEnvironment)
            if (obj != null) obj.SetActive(false);
        foreach (var obj in purifyEnvironment)
            if (obj != null) obj.SetActive(true);

        hidePromptCoroutine = StartCoroutine(ForceHidePrompt());

        // 關閉原本的 dialogueText
        if (allTriggers.Length > 0)
        {
            mainDialogue = allTriggers[0]?.dialogueText;
            if (mainDialogue != null)
                mainDialogue.gameObject.SetActive(false);
        }

        bool entityIntroDone = PlayerPrefs.GetInt("EntityIntroDone", 0) == 1;

        // 把玩家移到廣場旁邊，背對 ENTITY
        PlayerController player = FindObjectOfType<PlayerController>();
        if (player != null && purifyStartPosition != null)
        {
            player.transform.position = purifyStartPosition.position;
            player.SetFacingDirection(Vector2.up);
        }

        Camera mainCamera = Camera.main;
        CameraFollow cameraFollow = mainCamera.GetComponent<CameraFollow>();

        if (!entityIntroDone)
        {
            // 直接把鏡頭設在 ENTITY 位置，不用動畫
            Vector3 entityPos = new Vector3(
                entitySprite.transform.position.x,
                entitySprite.transform.position.y,
                mainCamera.transform.position.z
            );
            mainCamera.transform.position = entityPos;

            if (player != null)
                player.SetMovementLocked(true);

            if (cameraFollow != null)
                cameraFollow.enabled = false;
            disableCameraCoroutine = StartCoroutine(ForceDisableCameraFollow(cameraFollow));

            if (entitySprite != null)
                entitySprite.SetActive(true);

            entityPos = new Vector3(
                entitySprite.transform.position.x,
                entitySprite.transform.position.y,
                mainCamera.transform.position.z
            );
            mainCamera.transform.position = entityPos;

            yield return new WaitForSeconds(0.5f);

            if (dialogueText != null)
                dialogueText.gameObject.SetActive(true);

            string playerName = PlayerPrefs.GetString("PlayerName", "...");
            string[] lines = {
            "you saw me.",
            "not what i did.",
            "not what i became.",
            "me.",
            "...",
            "thank you, " + playerName + ".",
            "i can let go now.",
            "...",
            "before i go.",
            "there is one more thing.",
            "look around this place.",
            "i left something for you.",
            "three offerings.",
            "bring them to the circle.",
            "you'll understand.",
            "...",
            "may all beings be free.",
        };

            foreach (string line in lines)
            {
                if (dialogueText != null)
                {
                    dialogueText.text = "";
                    foreach (char c in line)
                    {
                        dialogueText.text += c;
                        yield return new WaitForSeconds(0.05f);
                    }
                    yield return new WaitForSeconds(2f);
                    dialogueText.text = "";
                }
            }

            if (dialogueText != null)
                dialogueText.gameObject.SetActive(false);

            yield return StartCoroutine(FadeOutEntity());

            // 記錄 ENTITY 已經說完話，下次不再重播
            PlayerPrefs.SetInt("EntityIntroDone", 1);
            PlayerPrefs.Save();

            if (player != null)
            {
                Vector3 backToPlayer = new Vector3(
                    player.transform.position.x,
                    player.transform.position.y,
                    mainCamera.transform.position.z
                );
                yield return StartCoroutine(MoveCamera(mainCamera, backToPlayer, 1f));
            }
        }
        else
        {
            // ENTITY 已經說過話，直接隱藏它，不重播對話
            if (entitySprite != null)
                entitySprite.SetActive(false);
            if (dialogueText != null)
                dialogueText.gameObject.SetActive(false);
        }

        // 恢復
        if (hidePromptCoroutine != null)
        {
            StopCoroutine(hidePromptCoroutine);
            hidePromptCoroutine = null;
        }

        if (disableCameraCoroutine != null)
        {
            StopCoroutine(disableCameraCoroutine);
            disableCameraCoroutine = null;
        }
        if (cameraFollow != null)
            cameraFollow.enabled = true;
        if (player != null)
            player.SetMovementLocked(false);
        if (mainDialogue != null)
            mainDialogue.gameObject.SetActive(true);

        // 直接用名字找，不依賴 Inspector 引用
        DialogueTrigger purifyTrigger = null;
        foreach (DialogueTrigger trigger in FindObjectsOfType<DialogueTrigger>(true))
        {
            if (trigger.gameObject.name == "PurifyMeditation_0")
            {
                purifyTrigger = trigger;
                break;
            }
        }

        if (purifyTrigger != null)
        {
            purifyTrigger.enabled = true;
            purifyTrigger.ResetTrigger();
            purifyTrigger.SetInputLocked(false);
        }

        foreach (DialogueTrigger trigger in FindObjectsOfType<DialogueTrigger>(true))
        {
            if (trigger.requireKeyPress && trigger != purifyTrigger)
                trigger.enabled = true;
        }
    }
    IEnumerator ForceHidePrompt()
    {
        while (true)
        {
            if (promptTextObj != null)
                promptTextObj.SetActive(false);
            yield return null;
        }
    }
    IEnumerator ForceDisableCameraFollow(CameraFollow cf)
    {
        while (cf != null)
        {
            cf.enabled = false;
            yield return null;
        }
    }
    IEnumerator MoveCamera(Camera cam, Vector3 targetPos, float duration)
    {
        Vector3 startPos = cam.transform.position;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            cam.transform.position = Vector3.Lerp(startPos, targetPos, elapsed / duration);
            yield return null;
        }

        cam.transform.position = targetPos;
    }
    IEnumerator FadeOutEntity()
    {
        if (entitySprite == null) yield break;

        SpriteRenderer sr = entitySprite.GetComponent<SpriteRenderer>();
        if (sr == null)
            sr = entitySprite.GetComponentInChildren<SpriteRenderer>();

        if (sr == null)
        {
            yield break;
        }

        float elapsed = 0f;
        Color original = sr.color;

        while (elapsed < entityFadeDuration)
        {
            elapsed += Time.deltaTime;
            float alpha = Mathf.Lerp(1f, 0f, elapsed / entityFadeDuration);
            sr.color = new Color(original.r, original.g, original.b, alpha);
            yield return null;
        }

        entitySprite.SetActive(false);
    }

    IEnumerator DeleteMode()
    {
        // 之後做
        yield break;
    }

    IEnumerator ConsumedMode()
    {
        PlayerController player = FindObjectOfType<PlayerController>();
        PlayerPrefs.SetInt("WasConsumed", 1);
        PlayerPrefs.SetInt("EntityPurified", 0); // 清掉
        PlayerPrefs.SetInt("EntityDeleted", 0);  // 也清掉以防萬一
        PlayerPrefs.Save();
        // 關閉 PurifyEnvironment，確保不會顯示
        foreach (var obj in purifyEnvironment)
            if (obj != null) obj.SetActive(false);

        // 正常環境
        foreach (var obj in normalEnvironment)
            if (obj != null) obj.SetActive(false);

        // 啟用 Consumed 環境
        foreach (var obj in consumedEnvironment)
            if (obj != null) obj.SetActive(true);

        // 停用所有 DialogueTrigger
        DialogueTrigger[] allTriggers = FindObjectsOfType<DialogueTrigger>();
        foreach (DialogueTrigger trigger in allTriggers)
            trigger.enabled = false;

        // 隱藏 promptText
        if (promptTextObj != null)
            promptTextObj.SetActive(false);

        // 持續隱藏
        hidePromptCoroutine = StartCoroutine(ForceHidePrompt());

        SpriteRenderer playerSprite = player.GetComponent<SpriteRenderer>();
        if (playerSprite != null)
        {
            playerSprite.color = new Color(0.6f, 1f, 0.95f, 0.5f); // 青白半透明
            playerSprite.material.SetFloat("_EmissionAmount", 1f); // 如果有發光 shader
        }

        // 顯示人影
        ConsumedShadow shadow = FindObjectOfType<ConsumedShadow>(true);
        if (shadow != null)
            shadow.gameObject.SetActive(true);
        yield break;
    }
}