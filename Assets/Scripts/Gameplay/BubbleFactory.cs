using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BubbleFactory : MonoBehaviour
{
    public static BubbleFactory Instance { get; private set; }

    public GameObject bubblePrefab;
    public BubbleSpriteLibrary spriteLibrary;

    private void Awake()
    {
        Instance = this;
    }

    public Bubble CreateBubble(Vector3 position, BubbleColor color)
    {
        GameObject obj = Instantiate(bubblePrefab, position, Quaternion.identity);
        Bubble bubble = obj.GetComponent<Bubble>();

        // 지정된 색상 사용
        bubble.color = color;

        // 스프라이트 적용
        SpriteRenderer renderer = obj.GetComponent<SpriteRenderer>();
        renderer.sprite = spriteLibrary.GetSprite(bubble.color);
        renderer.sortingLayerName = "Bubbles";
        renderer.sortingOrder = 5;

        return bubble;
    }

    public Bubble CreateRandomBubble(Vector3 position)
    {
        GameObject obj = Instantiate(bubblePrefab, position, Quaternion.identity);
        Bubble bubble = obj.GetComponent<Bubble>();

        // 랜덤 색상 부여
        bubble.color = (BubbleColor)Random.Range(0, System.Enum.GetValues(typeof(BubbleColor)).Length);

        // 스프라이트 적용
        SpriteRenderer renderer = obj.GetComponent<SpriteRenderer>();
        renderer.sprite = spriteLibrary.GetSprite(bubble.color);
        renderer.sortingLayerName = "Bubbles";
        renderer.sortingOrder = 5;

        return bubble;
    }
}