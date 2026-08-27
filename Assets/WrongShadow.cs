using UnityEngine;
using System.Collections;

public class WrongShadow : MonoBehaviour
{
    public Transform player;
    public SpriteRenderer shadowRenderer;
    public float minInterval = 15f;
    public float maxInterval = 35f;
    public float wrongMoveSpeed = 2f;
    public float wrongMoveDuration = 2f;

    private bool isActingWrong = false;
    public Vector3 normalOffset = new Vector3(0, -0.8f, 0);

    void Start()
    {
        StartCoroutine(RandomWrongBehaviour());
    }

    void Update()
    {
        if (!isActingWrong)
        {
            // 正常跟著玩家
            transform.position = player.position + normalOffset;
        }
    }

    IEnumerator RandomWrongBehaviour()
    {
        while (true)
        {
            yield return new WaitForSeconds(Random.Range(minInterval, maxInterval));

            isActingWrong = true;
            Vector3 frozenPos = transform.position;

            float elapsed = 0f;
            while (elapsed < wrongMoveDuration)
            {
                elapsed += Time.deltaTime;
                frozenPos += Vector3.right * wrongMoveSpeed * Time.deltaTime;
                transform.position = frozenPos;

                // 慢慢淡出
                float alpha = Mathf.Lerp(0.8f, 0f, elapsed / wrongMoveDuration);
                shadowRenderer.color = new Color(0, 0, 0, alpha);

                yield return null;
            }

            // 走掉後直接停止，不再回來
            shadowRenderer.color = new Color(0, 0, 0, 0f);
            gameObject.SetActive(false);
            yield break; // 結束 Coroutine
        }
    }
}