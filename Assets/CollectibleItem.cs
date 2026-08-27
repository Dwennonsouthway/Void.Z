using UnityEngine;
using TMPro;
using UnityEngine.InputSystem;

public class CollectibleItem : MonoBehaviour
{
    [Header("設定")]
    public string itemName;
    public TextMeshProUGUI hudText;
    public bool disappearOnCollect = true;

    private bool collected = false;
    private bool playerInRange = false;
    private static int activeRangeCount = 0;
    void Start()
    {
        collected = PlayerPrefs.GetInt("Collected_" + itemName, 0) == 1;
        if (collected && disappearOnCollect)
            gameObject.SetActive(false);
    }

    void Update()
    {
        if (playerInRange && !collected)
        {
            if (Keyboard.current.eKey.wasPressedThisFrame)
                Collect();
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player") && !collected)
        {
            playerInRange = true;
            activeRangeCount++;
            if (hudText != null)
            {
                hudText.text = "[ E ] \npick up ";
                hudText.gameObject.SetActive(true);
            }
        }
    }

    void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            playerInRange = false;

            // 已經撿了的話不要關 hudText，讓 HideHudAfterDelay 控制
            if (!collected && hudText != null)
                hudText.gameObject.SetActive(false);
        }
    }

    void Collect()
    {
        collected = true;
        PlayerPrefs.SetInt("Collected_" + itemName, 1);
        PlayerPrefs.Save();

        int total = GetTotalCollected();

        if (hudText != null)
        {
            hudText.gameObject.SetActive(true);
            hudText.text = total >= 3
                ? "the circle is ready."
                : itemName + " collected  \n[ " + total + "/3 ]";
        }

        // 先找 manager、先啟動 Coroutine，再 SetActive(false)
        VoidSpaceManager manager = FindObjectOfType<VoidSpaceManager>();
        if (manager != null)
            manager.StartCoroutine(HideHudAfterDelay(total >= 3 ? 3f : 2f));

        // SetActive 放最後
        if (disappearOnCollect)
            gameObject.SetActive(false);
    }
    int GetTotalCollected()
    {
        int count = 0;
        if (PlayerPrefs.GetInt("Collected_bong", 0) == 1) count++;
        if (PlayerPrefs.GetInt("Collected_weed", 0) == 1) count++;
        if (PlayerPrefs.GetInt("Collected_bowl", 0) == 1) count++;
        return count;
    }

    System.Collections.IEnumerator HideHudAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        if (hudText != null)
            hudText.gameObject.SetActive(false);
    }
}