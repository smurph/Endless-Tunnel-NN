using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Scorekeeper : MonoBehaviour
{
    public static Scorekeeper Instance { get; set; }

    public Text ScoreDisplay;
    public int StartingScore;

    public int Score { get; private set; }

    public void Awake()
    {
        Instance = this;
    }

    public void IncrementScore(int amount = 1)
    {
        Score += amount;
        UpdateGameText();
    }

    private void UpdateGameText()
    {
        ScoreDisplay.text = Score > 0 ? Score.ToString() : "0";
    }

    public void ResetScore()
    {
        Score = StartingScore;
        UpdateGameText();
    }
}
