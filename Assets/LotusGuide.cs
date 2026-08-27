using UnityEngine;
using System.Collections;

public class LotusGuide : MonoBehaviour
{
    [Header("設定")]
    public PlayerController player;
    public float leadDistance = 8f;      // 保持在玩家前方距離
    public float floatAmplitude = 0.3f;  // 上下飄浮幅度
    public float floatSpeed = 1.5f;      // 飄浮速度
    public float followSpeed = 3f;       // 跟隨速度

    [Header("狀態")]
    public bool isGuiding = true;        // 引路中
    public bool isAbsorbed = false;      // 已被吸收

    private float baseY;
    private SpriteRenderer sr;
    private Animator anim;
    public Transform entityTransform;
    public float absorbDistance = 1.5f;
    public CameraFollow cameraFollow;
    public LeftWall leftWall;
    private TrailRenderer trail;

    void Start()
    {
        trail = GetComponentInChildren<TrailRenderer>();
        trail.emitting = false;
        baseY = transform.position.y;
        sr = GetComponent<SpriteRenderer>();
        anim = GetComponent<Animator>();

        // 初始位置在玩家前方
        transform.position = new Vector3(
            player.transform.position.x + leadDistance,
            baseY,
            0);
    }

    void Update()
    {
        if (!isGuiding || isAbsorbed) return;

        float targetX = player.transform.position.x + leadDistance;
        bool isMoving = targetX > transform.position.x;

        trail.emitting = isMoving; // 移動才拖尾
        // 只有 targetX 比現在更右才移動
        float newX = transform.position.x;
        if (targetX > transform.position.x)
        {
            newX = Mathf.Lerp(transform.position.x, targetX, followSpeed * Time.deltaTime);
        }

        float newY = baseY + Mathf.Sin(Time.time * floatSpeed) * floatAmplitude;
        transform.position = new Vector3(newX, newY, 0);

        float dist = Vector3.Distance(transform.position, entityTransform.position);
        if (dist < absorbDistance)
        {
            isAbsorbed = true;
            StartCoroutine(AbsorbIntoEntity(entityTransform));
        }
    }

    public IEnumerator AbsorbIntoEntity(Transform entityTransform)
    {
        isAbsorbed = true;
        isGuiding = false;

        player.SetMovementLocked(true);

        // 停用左牆
        if (leftWall != null)
            leftWall.isActive = false;

        // 鏡頭移向 Entity
        Vector3 entityCamPos = new Vector3(
            entityTransform.position.x,
            cameraFollow.transform.position.y,
            cameraFollow.transform.position.z);
        cameraFollow.MoveTo(entityCamPos, 1.5f);

        yield return new WaitForSeconds(1.5f);

        GetComponent<Animator>().SetTrigger("Dissolve");

        float clipLength = 24f / 5f;
        yield return new WaitForSeconds(clipLength);

        gameObject.SetActive(false);

        // ENTITY 說完話後 JoinSceneManager 負責呼叫 ResumeFollow
        FindObjectOfType<JoinSceneManager>().StartEntityDialogue();
    }
}