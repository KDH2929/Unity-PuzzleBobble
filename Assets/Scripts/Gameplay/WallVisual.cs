using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(BoxCollider2D))]
public class WallVisual : MonoBehaviour
{
    void Awake()
    {
        BoxCollider2D collider = GetComponent<BoxCollider2D>();

        // 자식 GameObject 생성
        GameObject visual = new GameObject("Visual");
        visual.transform.parent = transform;
        visual.transform.localPosition = Vector3.zero;
        visual.transform.localRotation = Quaternion.identity;

        // 자식 GameObject에 SpriteRenderer를 붙이고 Scale을 조정한다.
        SpriteRenderer spriteRenderer = visual.AddComponent<SpriteRenderer>();


        Texture2D texture = new Texture2D(1, 1);
        texture.SetPixel(0, 0, Color.black);
        texture.Apply();

        // Create(Texture2D texture, Rect rect, Vector2 pivot, float pixelsPerUnit)
        spriteRenderer.sprite = Sprite.Create(texture, new Rect(0, 0, 1, 1), new Vector2(0.5f, 0.5f), 1f);

        spriteRenderer.color = Color.black;

        spriteRenderer.transform.localScale = new Vector3(collider.size.x, collider.size.y, 1f);

    }
}