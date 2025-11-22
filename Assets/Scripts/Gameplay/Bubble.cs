using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum BubbleColor
{
    Red,
    Blue,
    Green,
    Yellow,
    Purple
}

public class Bubble : MonoBehaviour
{
    public BubbleColor color;   // 버블 색상
    public bool isMoving = false;  // 발사 중인지 여부
    public bool isFalling = false;

    private Vector2 fallVelocity = Vector2.zero;
    private const float GRAVITY_FORCE = 20f;

    private float reflectCooldown = 0f;

    private Rigidbody2D rb; 

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    private void Update()
    {

        reflectCooldown -= Time.deltaTime;

        if (isFalling)
        {
            // 가속도 적용
            fallVelocity.y -= GRAVITY_FORCE * Time.deltaTime;

            // 위치 업데이트
            transform.Translate(fallVelocity * Time.deltaTime);
        }
    }

    // 발사할 때 호출
    public void Launch(Vector2 direction, float speed)
    {
        isMoving = true;
        rb.isKinematic = false;
        rb.velocity = direction.normalized * speed;
    }

    // 버블 착지
    public void Stop()
    {
        isMoving = false;
        rb.velocity = Vector2.zero;
        rb.isKinematic = true;  // Rigidbody 움직임 멈춤
    }

    // 버블 제거
    public void Pop()
    {
        // TODO: 애니메이션/효과 추가
        Destroy(gameObject);
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (!isMoving)
        {
            return;
        }

        if (collision.collider.CompareTag("Wall"))
        {
            Debug.Log("Wall과 충돌 발생");

            ReflectHorizontal(collision);
        }

        if (collision.collider.CompareTag("TopWall") || collision.collider.CompareTag("Bubble"))
        {
            Stop();

            // Board에 붙이기
            Vector2Int cell = BoardRenderer.Instance.AttachBubbleToBoard(this);


            // 같은 색상 연결 그룹 제거
            // 해당 함수 내부적으로 인접한 노드개수를 파악하여 3개이상일 시 해당 노드들을 제거
            Board.Instance.RemoveConnectedSameColor(cell.x, cell.y);

            if (Board.Instance.CheckForGameOver())
            {
                GameController.Instance.SetGameOver();
            }

            if (GameController.Instance != null)
            {
                GameController.Instance.NotifyBubbleAttached(); 
            }
        }
    }

    private void ReflectHorizontal(Collision2D collision)
    {
        if (reflectCooldown > 0f)
        {
            return;
        }

        Vector2 normal = collision.contacts[0].normal;

        // 반사
        Vector2 newVelocity = Vector2.Reflect(rb.velocity, normal);
        rb.velocity = newVelocity;

        reflectCooldown = 0.05f;
    }


    public void Fall()
    {

        gameObject.layer = LayerMask.NameToLayer("FallingBubble"); // Board 버블과 충돌 방지

        isFalling = true;

        // 사라지도록 일정 시간 후 Destroy
        Destroy(gameObject, 4f);
    }

}