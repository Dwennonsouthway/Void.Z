using UnityEngine;
using System.Collections;

public class ShadowFlash : MonoBehaviour
{
    public GameObject shadowSprite; // 黑色人形 Sprite
    public float minInterval = 8f;
    public float maxInterval = 20f;
    public float flashDuration = 0.08f; // 閃現多久
    public Sprite[] shadowVariants;
    private SpriteRenderer sr;

    void Start()
    {
        sr = shadowSprite.GetComponent<SpriteRenderer>();
        shadowSprite.SetActive(false);
        StartCoroutine(RandomFlash());
    }

    IEnumerator RandomFlash()
    {
        while (true)
        {
            yield return new WaitForSeconds(Random.Range(minInterval, maxInterval));

            sr.sprite = shadowVariants[Random.Range(0, shadowVariants.Length)];

            // 根據實際畫面寬度計算邊緣位置
            float camHeight = Camera.main.orthographicSize;
            float camWidth = camHeight * Camera.main.aspect;

            // 隨機出現在畫面左或右邊緣內側
            float side = Random.value > 0.5f ? 1f : -1f;
            float x = Camera.main.transform.position.x + side * (camWidth * 0.7f);
            float y = Camera.main.transform.position.y + Random.Range(-camHeight * 0.5f, camHeight * 0.5f);

            shadowSprite.transform.position = new Vector3(x, y, 0);

            shadowSprite.SetActive(true);
            yield return new WaitForSeconds(flashDuration);
            shadowSprite.SetActive(false);
        }
    }
}