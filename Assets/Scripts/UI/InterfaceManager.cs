using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class InterfaceManager : MonoBehaviour
{
    public static InterfaceManager Instance { get; set; }

    public GameObject GameMenuPage;
    public Text EndGameConditionButtonText;
    public Text SaveOnNewHighScoreButtonText;
    public Text GenerationDisplay;
    public Text HighScoreDisplay;
    public GameObject AIUIPage;
    public GameObject AISettingsPage;

    // Use this for initialization
    void Start ()
    {
        Instance = this;
	}
	
    public void ShowAIUI()
    {
        GameMenuPage.SetActive(false);
        AIUIPage.SetActive(true);
    }

    public void ShowGameMenu()
    {
        GameMenuPage.SetActive(true);
        AIUIPage.SetActive(false);
    }

    public void ShowLoadGameMenu()
    {
        //TODO
    }

    public void ToggleAISettings()
    {
        bool paused = !AISettingsPage.activeSelf;
        
        AIManager.Instance.PauseGame(paused);
        AISettingsPage.SetActive(paused);
    }

    internal void SetGenerationDisplay(int generation)
    {
        if (GenerationDisplay)
        {
            GenerationDisplay.text = generation.ToString();
        }
    }

    public void SetHighScoreDisplay(int score, int generation)
    {
        HighScoreDisplay.text = score.ToString() + " (Gen " + generation.ToString() + ")";
    }
}
