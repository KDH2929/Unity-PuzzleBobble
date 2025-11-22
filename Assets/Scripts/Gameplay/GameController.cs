using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SocialPlatforms.Impl;
using TMPro;

public class GameController : MonoBehaviour
{
    public static GameController Instance { get; private set; }

    [Header("Dependencies")]
    public Board board;
    public BoardRenderer boardRenderer;
    public Shooter shooter;
    public GameObject ceilingWall;  // 천장 벽 오브젝트 (콜라이더)

    [Header("Game Settings")]
    public float shiftDownInterval = 8.0f; // 보드가 한 칸 하강하는 주기 (초)
    private float timer;
    public int initialRowsToFill = 4;


    [Header("State")]
    private bool isGameOver = false;

    [Header("Score")]
    public int basePoints = 10;
    public int bonusPoints = 20;
    private int score = 0;

    [Header("UI Dependencies")]
    public TextMeshProUGUI scoreText;
    public GameObject gameOverPanel;


    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }


    private void Start()
    {

        gameOverPanel.SetActive(false);

        UpdateScoreUI();

        if (board == null || boardRenderer == null)
        {
            Debug.LogError("GameController requires Board and BoardRenderer components assigned.");
            enabled = false; // 컴포넌트 비활성화
            return;
        }

        board.InitializeBubbles(initialRowsToFill, 0.5f);
        boardRenderer.RenderBoard();

        timer = 0f;
    }

    private void Update()
    {
        if (isGameOver)
        {
            return;
        }

        timer += Time.deltaTime;

        if (timer >= shiftDownInterval)
        {
            timer = 0f;
            AttemptShiftDown();
        }
    }


    private void AttemptShiftDown()
    {
        boardRenderer.StartBoardShake();

        Invoke(nameof(ShiftDown), boardRenderer.shakeDuration);
    }

    private void ShiftDown()
    {
        // Board.ShiftDown()은 내부적으로 Game Over를 체크하고, 조건이 충족되면 return
        board.ShiftDown();

        if (board.CheckForGameOver())
        {
            SetGameOver();
        }

        if (isGameOver)
        {
            return;
        }

        boardRenderer.RenderBoard();
        MoveCeilingWallDown();
    }

    private void MoveCeilingWallDown()
    {
        if (ceilingWall == null)
        {
            Debug.LogWarning("Ceiling Wall is not assigned. Cannot move.");
            return;
        }

        float verticalShiftDistance = boardRenderer.BubbleDiameter;

        Vector3 wallPosition = ceilingWall.transform.position;

        // 벽을 아래로 이동 (버블 지름만큼)
        wallPosition.y -= verticalShiftDistance;
        ceilingWall.transform.position = wallPosition;

        Debug.Log($"Board shifted down. Wall moved by {verticalShiftDistance:F2}");
    }

    public void SetGameOver()
    {
        if (!isGameOver)
        {
            isGameOver = true;
            GameManager.Instance.EndGame();

            Debug.Log("--- GAME OVER! ---");

            if (gameOverPanel != null)
            {
                gameOverPanel.SetActive(true);
            }

            Time.timeScale = 0f;

            // 게임오버상태해제시 timeScale을 다시 1로 해야함
        }
    }


    public void AddScore(int matchedCount, int floatingCount)
    {
        // 점수 계산 로직을 GameController에서 수행
        int calculatedScore = (matchedCount * basePoints) + (floatingCount * bonusPoints);

        if (calculatedScore > 0)
        {
            score += calculatedScore;
            Debug.Log($"[Score] Total Score Added: {calculatedScore}");

            UpdateScoreUI();

            if (board.CheckForWin())
            {
                Invoke("SetGameOver", 1f);
                return;
            }
        }
    }

    private void UpdateScoreUI()
    {
        if (scoreText != null)
        {
            scoreText.text = $"Score: {score}";
        }
    }

    public void NotifyBubbleAttached()
    {
        if (isGameOver)
        {
            return;
        }

        shooter.CreateNextBubble();
    }
}