using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class NextBubbleSprite : MonoBehaviour
{
    private SpriteRenderer spriteRenderer;

    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        spriteRenderer.enabled = false; // 초기에는 숨김
    }

    // 다음 버블 정보로 갱신
    public void SetNextBubble(Sprite sprite)
    {
        if (spriteRenderer != null)
        {
            spriteRenderer.sprite = sprite;
            spriteRenderer.enabled = true;
        }
    }

    // 필요 시 숨기기
    public void Clear()
    {
        if (spriteRenderer != null)
        {
            spriteRenderer.enabled = false;
        }
    }
}