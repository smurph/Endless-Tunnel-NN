using Assets.Scripts;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

public class AIManager : MonoBehaviour
{
    public static AIManager Instance { get; set; }

    private Scorekeeper _scoreKeeper { get { return Scorekeeper.Instance; } }

    private InterfaceManager _interface { get { return InterfaceManager.Instance; } }

    public delegate void GameEvent();

    public static event GameEvent OnGenerationStart;
    public static event GameEvent OnGenerationEnd;

    public GameObject ShipPrefab;

    private List<GameObject> _ships;

    public float ShipSpeed;

    private float[][][] _bestWeights { get; set; }
    private float[][][] _bestWeightsThisGeneration { get; set; }

    private int _shipsPerGeneration = 6;
    private int _bestWeightScore { get; set; }
    private int _bestWeightGeneration { get; set; }
    private int _generation { get; set; }
    public bool Paused { get; private set; }

    private bool _isRunning;
    private bool _endGameWhenOnlyBestWeightsRemain = true;
    private bool _saveOnNewHighScore = true;

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
        if (activeShipCount == 0 || (_endGameWhenOnlyBestWeightsRemain && (activeShipCount == 1 && _ships.Where(s => s.gameObject.activeSelf).First().GetComponent<SpriteRenderer>().color == Color.black)))
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
        if (_shipsPerGeneration <= 0) return;

        for (int x = 0; x < _shipsPerGeneration; x++)
        {
            _ships.Add(Instantiate(ShipPrefab));
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

        GenerationHealthManager.Instance.ResetShips();

        var x = 0;
        foreach (var ship in _ships)
        {
            var aiMovement = ship.GetComponent(typeof(AIMovement)) as AIMovement;
            var shipRenderer = ship.GetComponent(typeof(SpriteRenderer)) as SpriteRenderer;

            aiMovement.ResetPosition();

            ship.SetActive(true);
            if (_bestWeights == null)
            {
                aiMovement.ResetNeuralNetwork();
                _bestWeights = aiMovement.Weights;
            }

            aiMovement.Weights = _bestWeights;

            if (x == 0)
            {
                // Don't modify weights
                shipRenderer.color = Color.black;
                GenerationHealthManager.Instance.AddShip(Color.black);
            }
            else if (x == 1)
            {
                aiMovement.VaryWeightsByAmount(0.75f);
                shipRenderer.color = Color.red;
                GenerationHealthManager.Instance.AddShip(Color.red);
            }
            else if (x == 2)
            {
                aiMovement.VaryWeightsByAmount(0.5f);
                shipRenderer.color = Color.blue;
                GenerationHealthManager.Instance.AddShip(Color.blue);
            }
            else if (x == 3)
            {
                aiMovement.VaryWeightsByAmount(0.25f);
                shipRenderer.color = Color.green;
                GenerationHealthManager.Instance.AddShip(Color.green);
            }
            else if (x == 4)
            {
                aiMovement.ResetNeuralNetwork();
                shipRenderer.color = Color.white;
                GenerationHealthManager.Instance.AddShip(Color.white);
            }
            else
            {
                if (_bestWeightsThisGeneration == null || _bestWeightsThisGeneration.SequenceEqual(_bestWeights))
                {
                    aiMovement.gameObject.SetActive(false);
                }
                else
                {
                    shipRenderer.color = Color.magenta;
                    aiMovement.Weights = _bestWeightsThisGeneration; // technically best weights *last* generation at this point
                    GenerationHealthManager.Instance.AddShip(Color.magenta);
                }
            }
            
            x++;
        }

        _bestWeightsThisGeneration = null;
    }

    private void ResetWeightsAndScore()
    {
        _bestWeightScore = 0;
        _bestWeightGeneration = 0;
        _bestWeightsThisGeneration = null;
        _bestWeights = null;
    }

    /// <summary>
    /// Called by GameMenuPage when Start (AI) is clicked.
    /// This is the start of the AI mode, and will start the first generation
    /// </summary>
    public void StartGame(bool loadLastSave=false)
    {
        ResetWeightsAndScore();
        _isRunning = true;
        InitShips();
        _interface.ShowAIUI();
        
        if (loadLastSave)
        {
            try
            {
                var ship = ShipInfo.LoadFromFile();
                _bestWeights = ship.Weights;
                _bestWeightGeneration = ship.Generation;
                _bestWeightScore = ship.HighScore;
            }
            catch {}
        }

        SetHighScore(_bestWeightScore, _bestWeightGeneration);
        StartGeneration(_bestWeightGeneration + 1);
    }

    /// <summary>
    /// End all AI simulation. Display game menu.
    /// </summary>
    public void EndGame()
    {
        DestroyShips();
        EndGeneration(true);
        _interface.ShowGameMenu();
        _isRunning = false;
        
        GenerationHealthManager.Instance.Background.SetActive(false);
    }

    /// <summary>
    /// Toggles whether we stop the generation when only Black (Best weights) ship remains
    /// </summary>
    public void ToggleEndGameCondition()
    {
        _endGameWhenOnlyBestWeightsRemain = !_endGameWhenOnlyBestWeightsRemain;
        _interface.EndGameConditionButtonText.text = (_endGameWhenOnlyBestWeightsRemain ? "Yes" : "No");
    }

    public void ToggleSaveOnNewHighScore()
    {
        _saveOnNewHighScore = !_saveOnNewHighScore;
        _interface.SaveOnNewHighScoreButtonText.text = (_saveOnNewHighScore ? "Yes" : "No");
    }

    /// <summary>
    /// Pause the game
    /// </summary>
    /// <param name="paused">false to unpause the game</param>
    public void PauseGame(bool paused = true)
    {
        Paused = paused;
        DynamicWallScroller.Instance.Paused = paused;
    }


    /// <summary>
    /// Save the current high score weights to disk
    /// </summary>
    public void SaveGame()
    {
        Debug.Log("Saving Game");
        var ship = new ShipInfo()
        {
            Weights = _bestWeights,
            HighScore = _bestWeightScore,
            Generation = _bestWeightGeneration
        };

        ship.SaveToFile();
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
        OnGenerationEnd();
        if (_isRunning && !final)
        {
            StartGeneration(++_generation);
        }
    }

    private void SetGeneration(int generation)
    {
        _generation = generation;
        _interface.SetGenerationDisplay(_generation);
    }

    public void ShipDied(float[][][] weights)
    {
        if (_scoreKeeper.Score > _bestWeightScore)
        {
            if (_saveOnNewHighScore) SaveGame();

            SetHighScore(_scoreKeeper.Score, _generation);
            _bestWeights = weights;
        }

        if (!_bestWeights.SequenceEqual(weights)) _bestWeightsThisGeneration = weights;
    }

    private void SetHighScore(int score, int generation)
    {
        _bestWeightScore = score;
        _bestWeightGeneration = generation;
        _interface.SetHighScoreDisplay(score, generation);
    }
}
