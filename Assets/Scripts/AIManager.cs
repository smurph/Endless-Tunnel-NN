using Assets.Scripts;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class AIManager : MonoBehaviour
{
    public static AIManager Instance { get; set; }

    private Scorekeeper _scoreKeeper { get { return Scorekeeper.Instance; } }

    public delegate void GameEvent();

    public static event GameEvent OnGenerationStart;
    public static event GameEvent OnGenerationEnd;

    public int GameScore { get; private set; }

    public float ShipSpeed;
    public GameObject Prefab;
    public GameObject GameMenuPage;

    public GameObject AIUIPage;
    public Text GenerationDisplay;
    public Text HighScoreDisplay;
    public int ShipsPerGeneration = 20;

    private List<GameObject> _ships;
    private bool _isRunning;

    private float[][][] _bestWeights { get; set; }
    private int _bestWeightScore { get; set; }

    private int _generation { get; set; }

    // Use this for initialization
    void Start ()
    {
        _isRunning = false;
        DestroyShips();
    }

    private void Update()
    {
        if (!_isRunning) return;

        if (Input.GetKeyDown(KeyCode.Escape)) EndGame();

        var activeShipCount = _ships.Count(s => s.gameObject.activeSelf);
        if (activeShipCount == 0 || (activeShipCount == 1 && _ships.Where(s => s.gameObject.activeSelf).First().GetComponent<SpriteRenderer>().color == Color.black))
        {
            EndGeneration();
        }
    }

    public void Awake()
    {
        Instance = this;
    }

    private void InitShips()
    {
        if (ShipsPerGeneration <= 0) return;

        for (int x = 0; x < ShipsPerGeneration; x++)
        {
            _ships.Add(Instantiate(Prefab));
        }
    }

    private void DestroyShips()
    {
        if (_ships != null && _ships.Count > 0)
        {
            foreach (var ship in _ships)
            {
                Destroy(ship);
            }
        }

        _ships = new List<GameObject>();
    }

    private void ResetShips()
    {
        if (_ships.Count == 0) return;
        
        var x = 0;
        foreach (var ship in _ships)
        {
            var aiMovement = ship.GetComponent(typeof(AIMovement)) as AIMovement;
            var shipRenderer = ship.GetComponent(typeof(SpriteRenderer)) as SpriteRenderer;

            aiMovement.ResetPosition();
            ship.SetActive(true);
            if (_bestWeights == null)
            {
                aiMovement.VaryWeightsByAmount(1.0f);
                _bestWeights = aiMovement.Weights;
            }

            aiMovement.Weights = _bestWeights;

            if (x == 0)
            {
                // Don't modify weights
                shipRenderer.color = Color.black;
            }
            else if (x == 1)
            {
                aiMovement.VaryWeightsByAmount(0.75f);
                shipRenderer.color = Color.red;
            }
            else if (x == 2)
            {
                aiMovement.VaryWeightsByAmount(0.5f);
                shipRenderer.color = Color.blue;
            }
            else
            {
                aiMovement.VaryWeightsByAmount(0.25f);
                shipRenderer.color = Color.green;
            }
            
            x++;
        }
    }

    private void ResetWeightsAndScore()
    {
        _bestWeightScore = 0;
        _bestWeights = null;
    }

    /// <summary>
    /// Called by GameMenuPage when Start (AI) is clicked.
    /// This is the start of the AI mode, and will start the first generation
    /// </summary>
    public void StartGame()
    {
        ResetWeightsAndScore();
        _isRunning = true;
        InitShips();
        GameMenuPage.SetActive(false);
        AIUIPage.SetActive(true);
        SetHighScore(0);
        StartGeneration(1);
    }

    /// <summary>
    /// End all AI simulation. Display game menu.
    /// </summary>
    public void EndGame()
    {
        DestroyShips();
        EndGeneration(true);
        GameMenuPage.SetActive(true);
        AIUIPage.SetActive(false);
        _isRunning = false;
    }

    private void StartGeneration(int generation)
    {
        SetGeneration(generation);
        _scoreKeeper.ResetScore();
        ResetShips();
        OnGenerationStart();
    }

    private void EndGeneration(bool final = false)
    {
        ResetShips();
        OnGenerationEnd();
        if (_isRunning && !final)
        {
            StartGeneration(++_generation);
        }
    }

    private void SetGeneration(int generation)
    {
        _generation = generation;
        if (GenerationDisplay)
        {
            GenerationDisplay.text = _generation.ToString();
        }
    }

    public void ShipDied(float[][][] weights)
    {
        if (_scoreKeeper.Score > _bestWeightScore)
        {
            SetHighScore(_scoreKeeper.Score);
            _bestWeights = weights;
        }
    }

    private void SetHighScore(int score)
    {
        _bestWeightScore = score;
        if (HighScoreDisplay)
        {
            HighScoreDisplay.text = score.ToString();
        }
    }
}
