using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class UIController : MonoBehaviour
{

    public TextMeshProUGUI scoreText;

    public TetrisManager tetrisManager;

    public GameObject endGamePanel;

    public void UpdateScore()
    {
        scoreText.text = $"SCORE: {tetrisManager.score:n0}";
    }

    public void UpdateGameOver()
    {
        // when the game over Event is broadcast,
        // the end game panel will show when the game is over
        // it will hide when the game resets
        endGamePanel.SetActive(tetrisManager.gameOver);
    }

    public void PlayAgain()
    {
        // setting the game over to false resets the game
        tetrisManager.SetGameOver(false);
    }
}
