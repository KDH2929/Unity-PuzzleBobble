using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Board : MonoBehaviour
{
    // 싱글톤
    public static Board Instance { get; private set; }

    public int rows = 11;

    // 각 row마다 열 개수가 다름: 8 / 7
    public const int WIDE_ROW_COLS = 8; // 8개짜리 행 구조
    public const int NARROW_ROW_COLS = 7; // 7개짜리 행 구조

    private int topRow = 0;

    // 버블이 들어갈 격자
    private Bubble[][] grid;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            // 씬에 이미 다른 인스턴스가 있다면, 자신을 파괴
            Destroy(gameObject);
            return;
        }

        Instance = this;

        InitGrid();
    }

    void InitGrid()
    {
        grid = new Bubble[rows][];

        for (int r = 0; r < rows; r++)
        {
            // r=0, 2, 4...는 넓은 행으로, r=1, 3, 5...는 좁은 행으로 시작
            int colCount = (r % 2 == 0) ? WIDE_ROW_COLS : NARROW_ROW_COLS;
            grid[r] = new Bubble[colCount];
        }
    }

    public bool IsInside(int r, int c)
    {
        // 행 유효성 검사
        if (r < 0 || r >= rows)
        {
            return false;
        }

        // 열 유효성 검사
        if (c < 0 || c >= GetColCount(r))
        {
            return false;
        }

        return true;
    }

    public Bubble GetBubble(int r, int c)
    {
        if (!IsInside(r, c))
        {
            return null;
        }

        return grid[r][c];
    }

    public void SetBubble(int r, int c, Bubble bubble)
    {
        if (!IsInside(r, c))
        {
            return;
        }

        grid[r][c] = bubble;
    }

    public int GetColCount(int r)
    {
        // 행 인덱스 유효성 검사
        if (r < 0 || r >= rows || grid[r] == null)
        {
            return 0; // 유효하지 않은 행이면 0 반환
        }

        // 해당 행 배열의 실제 길이를 반환
        return grid[r].Length;
    }

    public List<(int, int)> GetNeighbors(int r, int c)
    {
        List<(int, int)> neighbors = new List<(int, int)>();

        // r이 유효한 행인지 먼저 확인
        if (r < 0 || r >= rows || grid[r] == null)
        {
            return neighbors;
        }

        // 현재 행의 구조(Wide=8 또는 Narrow=7)를 파악
        bool isWideRowStructure = grid[r].Length == WIDE_ROW_COLS;

        // (Wide) 구조의 오프셋 (8개 열)
        int[][] wideOffsets = new int[][]
        {
        new int[]{ 0, -1 }, // 좌
        new int[]{ 0,  1 }, // 우
        new int[]{ -1, -1 }, // 좌상
        new int[]{ -1, 0 }, // 우상
        new int[]{  1, -1 }, // 좌하
        new int[]{  1, 0 }, // 우하
        };

        // (Narrow) 구조의 오프셋 (7개 열)
        int[][] narrowOffsets = new int[][]
        {
        new int[]{ 0, -1 }, // 좌
        new int[]{ 0,  1 }, // 우
        new int[]{ -1, 0 }, // 좌상
        new int[]{ -1,  1 }, // 우상
        new int[]{  1, 0 }, // 좌하
        new int[]{  1,  1 }, // 우하
        };

        // 행 구조의 길이에 따라 오프셋 선택
        int[][] offsets = isWideRowStructure ? wideOffsets : narrowOffsets;

        // 인접 노드 순회
        foreach (var offset in offsets)
        {
            int nr = r + offset[0]; // 이웃 행
            int nc = c + offset[1]; // 이웃 열

            // IsInside를 사용하여 해당 위치가 보드 범위 내에 있고, 
            // 목표 행(nr)의 배열 길이 내에 nc가 있는지 확인
            if (IsInside(nr, nc))
            {
                neighbors.Add((nr, nc));
            }
        }

        return neighbors;
    }


    public List<(int r, int c)> FindSameColorGroup(int startR, int startC)
    {
        Bubble startBubble = GetBubble(startR, startC);
        if (startBubble == null)
        {
            Debug.LogWarning($"[Find Group] 시작 위치 ({startR}, {startC})에 버블이 없습니다. 탐색 취소.");

            return new List<(int, int)>();
        }

        BubbleColor targetColor = startBubble.color;
        Debug.Log($"[Find Group] 탐색 시작 버블 색상: {targetColor}");

        List<(int, int)> result = new List<(int, int)>();
        Queue<(int r, int c)> queue = new Queue<(int, int)>();
        HashSet<(int, int)> visited = new HashSet<(int, int)>();

        queue.Enqueue((startR, startC));
        visited.Add((startR, startC));

        // BFS 로 인접노드들을 파악하면 된다.
        while (queue.Count > 0)
        {
            var (r, c) = queue.Dequeue();
            result.Add((r, c));

            foreach (var (nr, nc) in GetNeighbors(r, c))
            {
                if (visited.Contains((nr, nc)))
                {
                    continue;
                }

                Bubble neighbor = GetBubble(nr, nc);
                if (neighbor == null)
                {
                    continue;
                }

                // 같은 색상만 연결
                if (neighbor.color == targetColor)
                {
                    visited.Add((nr, nc));
                    queue.Enqueue((nr, nc));
                }
            }
        }

        return result;
    }


    public void RemoveConnectedSameColor(int startR, int startC)
    {
        List<(int r, int c)> group = FindSameColorGroup(startR, startC);

        Debug.Log($"[Match Check] Row:{startR}, Col:{startC} 에서 찾은 그룹 크기: {group.Count}개");

        // 3개 이상일 때만 제거
        if (group.Count >= 3)
        {
            int matchedCount = RemoveBubbles(group);
            int floatingCount = RemoveFloatingBubbles();

            GameController.Instance.AddScore(matchedCount, floatingCount);
        }
    }

    public int RemoveBubbles(List<(int r, int c)> positions)
    {
        int removedCount = 0;

        foreach (var (r, c) in positions)
        {
            Bubble bubble = GetBubble(r, c);
            if (bubble != null)
            {
                SetBubble(r, c, null);           // Board 그리드에서 null 처리

                bubble.Fall();

                removedCount++;
            }
        }

        return removedCount;
    }

    public int RemoveFloatingBubbles()
    {
        bool[][] visited = new bool[rows][];
        for (int r = 0; r < rows; r++)
        {
            visited[r] = new bool[GetColCount(r)];
        }

        Queue<(int r, int c)> queue = new Queue<(int r, int c)>();

        int topRowColCount = GetColCount(topRow);
        int removedCount = 0;

        // 최상단 Row에서 BFS 시작  (MultiSource-BFS)
        for (int c = 0; c < topRowColCount; c++)
        {
            if (grid[topRow][c] != null)
            {
                queue.Enqueue((topRow, c));
                visited[topRow][c] = true;
            }
        }

        // BFS 수행
        // 벽에 연결된 버블들만 visited 처리됨
        while (queue.Count > 0)
        {
            var (r, c) = queue.Dequeue();

            foreach (var (nr, nc) in GetNeighbors(r, c))
            {
                if (!visited[nr][nc] && grid[nr][nc] != null)
                {
                    visited[nr][nc] = true;
                    queue.Enqueue((nr, nc));
                }
            }
        }

        // visited = false 이면서 grid에 버블이 존재하면 제거
        for (int r = 0; r < rows; r++)
        {
            for (int c = 0; c < GetColCount(r); c++)
            {
                if (!visited[r][c] && grid[r][c] != null)
                {
                    removedCount++;

                    Bubble bubble = grid[r][c];

                    // Board 상에서 논리적인 제거
                    grid[r][c] = null;

                    // 화면에서 떨어지는 애니메이션
                    bubble.Fall();
                }
            }
        }

        return removedCount;
    }

    // 게임 종료조건 달성 시 true 리턴
    public void ShiftDown()
    {
        int previousTopRowColCount = GetColCount(topRow);

        topRow++;

        // 버블 및 행 구조를 아래로 이동 (r=rows-1 부터 r=1까지)
        for (int r = rows - 1; r > 0; r--)
        {
            // 배열 참조 자체 이동
            grid[r] = grid[r - 1];
        }


        // 새로운 최상단 행은 이전 최상단 행과 반대 구조를 가져야 패턴이 유지됩니다.
        int newTopRowColCount;

        if (previousTopRowColCount == WIDE_ROW_COLS)
        {
            // 이전이 8개(Wide) -> 새로 생길 행은 7개(Narrow)
            newTopRowColCount = NARROW_ROW_COLS;
        }
        else
        {
            // 이전이 7개(Narrow) -> 새로 생길 행은 8개(Wide)
            newTopRowColCount = WIDE_ROW_COLS;
        }

        // 최상단 행 (r=0)을 새 구조로 초기화
        grid[0] = new Bubble[newTopRowColCount];

    }


    // 떨어지는 애니메이션없이 즉시 제거
    public void RemoveFloatingBubblesInstant()
    {
        bool[][] visited = new bool[rows][];
        for (int r = 0; r < rows; r++)
        {
            // GetColCount(r)을 사용하여 visited 배열 초기화
            visited[r] = new bool[GetColCount(r)];
        }

        Queue<(int r, int c)> queue = new Queue<(int r, int c)>();

        // 최상단 Row에서 BFS 시작 (topRow는 현재 0이어야 함)
        for (int c = 0; c < GetColCount(topRow); c++)
        {
            if (grid[topRow][c] != null)
            {
                queue.Enqueue((topRow, c));
                visited[topRow][c] = true;
            }
        }

        // BFS 수행 (연결된 버블만 visited 처리)
        while (queue.Count > 0)
        {
            var (r, c) = queue.Dequeue();

            foreach (var (nr, nc) in GetNeighbors(r, c))
            {
                if (!visited[nr][nc] && grid[nr][nc] != null)
                {
                    visited[nr][nc] = true;
                    queue.Enqueue((nr, nc));
                }
            }
        }

        // visited = false 이면서 grid에 버블이 존재하면 즉시 제거
        for (int r = 0; r < rows; r++)
        {
            for (int c = 0; c < GetColCount(r); c++)
            {
                if (!visited[r][c] && grid[r][c] != null)
                {
                    Bubble bubble = grid[r][c];

                    grid[r][c] = null;
                    Destroy(bubble.gameObject);
                }
            }
        }
    }


    public bool CheckForGameOver()
    {
        int lastR = rows - 1;
        for (int c = 0; c < GetColCount(lastR); c++)
        {
            if (grid[lastR][c] != null)
            {
                // Debug.LogError("GAME OVER: ...")
                return true;
            }
        }
        return false;
    }

    public void InitializeBubbles(int initialRows, float chanceToSpawn = 0.5f)
    {
        if (BubbleFactory.Instance == null)
        {
            Debug.LogError("BubbleFactory가 초기화되지 않았습니다. 버블 초기화 실패.");
            return;
        }

        // Board의 rows 변수를 넘어가지 않도록 행의 최대값을 제한
        int maxR = Mathf.Min(initialRows, rows);

        for (int r = 0; r < maxR; r++)
        {
            int colCount = GetColCount(r);
            for (int c = 0; c < colCount; c++)
            {
                if (Random.value < chanceToSpawn)
                {
                    Bubble newBubble = BubbleFactory.Instance.CreateRandomBubble(Vector3.zero);
                    SetBubble(r, c, newBubble);
                }
            }
        }

        RemoveFloatingBubblesInstant();
    }


    public bool CheckForWin()
    {
        for (int r = 0; r < rows; r++)
        {
            int colCount = GetColCount(r);
            for (int c = 0; c < colCount; c++)
            {
                if (grid[r][c] != null)
                {
                    return false;
                }
            }
        }

        return true;
    }
}