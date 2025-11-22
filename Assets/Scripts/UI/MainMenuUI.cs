using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuUI : MonoBehaviour
{
    public void StartGame()
    {
        Debug.Log("StartGame ÇÔ¼ö È£ÃâµÊ.");

        SceneManager.LoadScene("GameScene");
    }

    public void QuitGame()
    {
        Application.Quit();
    }
}
