
using UnityEngine;

public class LeftWall : MonoBehaviour
{
    public Camera mainCamera;
    public Rigidbody2D playerRb;
    public bool isActive = true;
    void Update()
    {
        if (!isActive) return; // 停用時不推玩家

        float leftEdge = mainCamera.transform.position.x
                         - mainCamera.orthographicSize * mainCamera.aspect;

        if (playerRb.position.x < leftEdge + 0.7f)
        {
            playerRb.position = new Vector2(
                leftEdge + 0.7f,
                playerRb.position.y);
        }
    }
}