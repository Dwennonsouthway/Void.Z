using UnityEngine;
using TMPro;
using UnityEngine.InputSystem;
using System.Collections;

public class TerminalInteract : MonoBehaviour
{
    public TextMeshProUGUI promptText;
    public float zoomSize = 2f;
    private bool playerInRange = false;
    private Camera mainCamera;

    void Start()
    {
        mainCamera = Camera.main;
        if (promptText != null)
            promptText.gameObject.SetActive(false);
    }

    void Update()
    {
        if (playerInRange && Keyboard.current.eKey.wasPressedThisFrame)
        {
            playerInRange = false;
            if (promptText != null)
                promptText.gameObject.SetActive(false);
            StartCoroutine(ZoomAndLoad());
        }
    }

    IEnumerator ZoomAndLoad()
    {
        yield return StartCoroutine(ZoomCamera(zoomSize, 0.5f));
        yield return new WaitForSeconds(0.3f);
        SceneTransition.Instance.GoToScene("TerminalScene");
    }

    IEnumerator ZoomCamera(float targetSize, float duration)
    {
        float startSize = mainCamera.orthographicSize;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;
            mainCamera.orthographicSize = Mathf.Lerp(startSize, targetSize, t);
            yield return null;
        }

        mainCamera.orthographicSize = targetSize;
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            playerInRange = true;
            if (promptText != null)
                promptText.gameObject.SetActive(true);
        }
    }

    void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            playerInRange = false;
            if (promptText != null)
                promptText.gameObject.SetActive(false);
        }
    }
}