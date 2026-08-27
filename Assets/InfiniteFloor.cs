using UnityEngine;

public class InfiniteFloor : MonoBehaviour
{
    public Transform floor1;
    public Transform floor2;
    public Transform floor3;
    public Transform cameraTransform;

    private float floorWidth;

    void Start()
    {
        floorWidth = floor1.GetComponent<SpriteRenderer>().bounds.size.x;
        float startX = cameraTransform.position.x - floorWidth;
        floor1.position = new Vector3(startX, floor1.position.y, 0);
        floor2.position = new Vector3(startX + floorWidth, floor2.position.y, 0);
        floor3.position = new Vector3(startX + floorWidth * 2, floor3.position.y, 0);
    }

    void Update()
    {
        // 只要地板跑到 Camera 左側就接到最右邊
        Transform rightmost = GetRightmost();

        if (floor1.position.x + floorWidth < cameraTransform.position.x)
            floor1.position = new Vector3(rightmost.position.x + floorWidth, floor1.position.y, 0);

        if (floor2.position.x + floorWidth < cameraTransform.position.x)
            floor2.position = new Vector3(GetRightmost().position.x + floorWidth, floor2.position.y, 0);

        if (floor3.position.x + floorWidth < cameraTransform.position.x)
            floor3.position = new Vector3(GetRightmost().position.x + floorWidth, floor3.position.y, 0);
    }

    Transform GetRightmost()
    {
        if (floor1.position.x >= floor2.position.x && floor1.position.x >= floor3.position.x)
            return floor1;
        if (floor2.position.x >= floor1.position.x && floor2.position.x >= floor3.position.x)
            return floor2;
        return floor3;
    }
}