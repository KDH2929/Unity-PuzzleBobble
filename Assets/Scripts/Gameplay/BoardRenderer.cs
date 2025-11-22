using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BoardRenderer : MonoBehaviour
{
    public Board board;
    public Bubble bubblePrefab;

    private float bubbleRadius;
    private float bubbleDiameter;

    [Header("Board Shake Warning")]
    public float shakeMagnitude = 0.05f; // 흔들림의 최대 강도 (월드 유닛)
    public float shakeSpeed = 15f;       // 흔들림의 속도 (빠를수록 더 빨리 진동)
    public float shakeDuration = 0.5f;   // 전체 흔들림이 지속되는 시간

    private bool isBoardShaking = false;
    private float currentShakeTime = 0f;
    private Vector2 currentShakeOffset = Vector2.zero; // 현재 적용될 흔들림 오프셋

    // 싱글톤
    public static BoardRenderer Instance { get; private set; }

    public float BubbleDiameter
    {
        get { return bubbleDiameter; }
    }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            // 씬에 이미 다른 인스턴스가 있다면, 자신을 파괴
            Destroy(gameObject);
            return;
        }

        Instance = this;

        InitializeBubbleSize();
        RenderBoard();
    }

    private void Update()
    {
        if (isBoardShaking)
        {
            currentShakeTime += Time.deltaTime;

            // 좌우 흔들림 (X축)
            // Mathf.Sin을 이용하여 시간(currentShakeTime)에 따라 -magnitude에서 +magnitude 사이를 반복
            float xOffset = Mathf.Sin(currentShakeTime * shakeSpeed) * shakeMagnitude;
            currentShakeOffset = new Vector2(xOffset, 0f); // Y축은 0으로 고정

            // RenderBoard()를 호출하여 모든 버블 위치를 업데이트
            RenderBoard();

            if (currentShakeTime >= shakeDuration)
            {
                isBoardShaking = false;
                currentShakeOffset = Vector2.zero; // 흔들림 끝나면 오프셋 리셋
                RenderBoard(); // 모든 버블을 원래 위치로
            }
        }
    }

    private void InitializeBubbleSize()
    {
        SpriteRenderer renderer = bubblePrefab.GetComponent<SpriteRenderer>();
        bubbleDiameter = renderer.bounds.size.x;   // 원이므로 가로 = 세로
        bubbleRadius = bubbleDiameter * 0.5f;
    }


    public void RenderBoard()
    {
        for (int row = 0; row < board.rows; row++)
        {
            int colCount = board.GetColCount(row);

            for (int col = 0; col < colCount; col++)
            {
                // Board에 Bubble 없으면 생성
                Bubble bubble = board.GetBubble(row, col);

                if (bubble != null)
                {
                    PlaceBubble(bubble, row, col);
                }
            }
        }
    }

    // 원의 중심좌표 리턴
    public Vector2 GetWorldPos(int row, int col)
    {
        float x = col * bubbleDiameter;
        float y = -row * bubbleDiameter;

        int currentColCount = board.GetColCount(row);

        if (currentColCount == Board.NARROW_ROW_COLS)
        {
            x += bubbleRadius;
        }

        // BoardRenderer 위치를 기준으로 offset
        Vector2 boardOrigin = transform.position;

        Vector2 baseWorldPos = new Vector2(x + boardOrigin.x, y + boardOrigin.y);

        if (isBoardShaking)
        {
            return baseWorldPos + currentShakeOffset;
        }

        return baseWorldPos;
    }

    public void PlaceBubble(Bubble bubble, int row, int col)
    {
        bubble.transform.position = GetWorldPos(row, col);
        bubble.Stop();
    }

    public Vector2Int GetNearestCell(Vector2 worldPos)
    {
        int nearestRow = 0;
        int nearestCol = 0;
        float minDistance = float.MaxValue;

        // 모든 board의 Cell 중에서 중심과 현재 붙을 Bubble의 중심거리를 비교하여
        // 비어있는 칸 중 가까운 위치를 찾는다.

        for (int row = 0; row < board.rows; row++)
        {
            int colCount = board.GetColCount(row);
            for (int col = 0; col < colCount; col++)
            {
                if (board.GetBubble(row, col) != null)
                {
                    continue; // 이미 버블 있음
                }

                Vector2 cellPos = GetWorldPos(row, col);
                float dist = Vector2.Distance(worldPos, cellPos);

                if (dist < minDistance)
                {
                    minDistance = dist;
                    nearestRow = row;
                    nearestCol = col;
                }
            }
        }

        return new Vector2Int(nearestRow, nearestCol);
    }

    public Vector2Int AttachBubbleToBoard(Bubble bubble)
    {
        Vector2Int cell = GetNearestCell(bubble.transform.position);
        board.SetBubble(cell.x, cell.y, bubble);
        PlaceBubble(bubble, cell.x, cell.y);

        bubble.Stop(); // Rigidbody 정지

        return cell;
    }

    // 외부에서 호출
    public void StartBoardShake()
    {
        if (isBoardShaking)
        {
            return;
        }

        isBoardShaking = true;
        currentShakeTime = 0f;
    }

}

