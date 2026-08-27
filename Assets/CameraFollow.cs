using UnityEngine;
using System.Collections;
public class CameraFollow : MonoBehaviour
{
    public Transform target;
    public float smoothSpeed = 5f;
    public Vector3 offset = new Vector3(0, 0, -10);
    public bool infiniteHorizontal = false;
    public float leftBound = -15f;
    public float rightBound = 15f;
    public float topBound = 10f;
    public float bottomBound = -10f;
    private Camera mainCamera;
    private float fixedY;
    private float furthestX;

    void Start()
    {
        mainCamera = GetComponent<Camera>();
        fixedY = transform.position.y;
        furthestX = transform.position.x;
    }

    void LateUpdate()
    {
        if (target == null) return;

        Vector3 desiredPos = target.position + offset;

        if (infiniteHorizontal)
        {
            if (desiredPos.x > furthestX)
            {
                furthestX = desiredPos.x;
            }

            desiredPos.x = furthestX;
            desiredPos.y = fixedY;
        }
        else
        {
            float camHalfHeight = mainCamera.orthographicSize;
            float camHalfWidth = mainCamera.orthographicSize * mainCamera.aspect;

            desiredPos.x = Mathf.Clamp(desiredPos.x, leftBound + camHalfWidth, rightBound - camHalfWidth);
            desiredPos.y = Mathf.Clamp(desiredPos.y, bottomBound + camHalfHeight, topBound - camHalfHeight);
        }

        transform.position = Vector3.Lerp(
            transform.position, desiredPos, smoothSpeed * Time.deltaTime);
    }

    public void MoveTo(Vector3 targetPosition, float duration)
    {
        StartCoroutine(MoveToCoroutine(targetPosition, duration));
    }

    IEnumerator MoveToCoroutine(Vector3 targetPosition, float duration)
    {
        target = null;

        Vector3 startPos = transform.position;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;
            transform.position = Vector3.Lerp(startPos, targetPosition, t);
            yield return null;
        }
    }

    public void ResumeFollow(Transform newTarget)
    {
        target = newTarget;
    }
}