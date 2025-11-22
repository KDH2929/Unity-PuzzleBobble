using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    public enum GameState { Ready, Playing, GameOver }
    public GameState State { get; private set; }

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        State = GameState.Ready;
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (State == GameState.Playing || State == GameState.GameOver)
            {
                GoToMainMenu();
            }
        }
    }

    public void StartGame()
    {
        State = GameState.Playing;
    }

    public void EndGame()
    {
        State = GameState.GameOver;
    }

    public void GoToMainMenu()
    {
        Debug.Log("[GameManager] GoToMainMenu 함수 호출됨.");
        
        State = GameState.Ready;

        // 시간복원
        Time.timeScale = 1f;
        SceneManager.LoadScene("MainMenu");
    }
}