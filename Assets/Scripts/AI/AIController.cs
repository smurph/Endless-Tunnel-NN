using System;
using System.Collections.Generic;
using System.Linq;
using Assets.Scripts;
using Assets.Scripts.AI.Ships;
using UnityEngine;

public class AIController : MonoBehaviour
{
    public static AIController Instance { get; set; }

    private Scorekeeper _scoreKeeper { get { return Scorekeeper.Instance; } }

    private InterfaceManager _interface { get { return InterfaceManager.Instance; } }

    public delegate void GameEvent();

    public static event GameEvent OnGenerationStart;
    public static event GameEvent OnGenerationEnd;

    public GameObject ShipPrefab;

    private List<IAIShip> _ships;

    public float ShipSpeed;

    private float[][][] _bestWeights { get; set; }
    private float[][][] _bestWeightsThisGeneration { get; set; }

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

        if (ShouldEndGeneration())
        {
            EndGeneration();
        }
    }

    private bool ShouldEndGeneration()
    {
        var activeShips = _ships.Where(s => s.Active);

        if (activeShips.Count() == 0) return true;

        return (_endGameWhenOnlyBestWeightsRemain
            && (activeShips.Count() == 1
                && _ships.First(s => s.Active).Color == Color.black));
    }

    public void Awake()
    {
        Instance = this;
    }

    private void InitShips()
    {
        //todo: automate this somehow?

        _ships = new List<IAIShip>
        {
            NewColorfulShip<RedShip>(),
            NewColorfulShip<GreenShip>(),
            NewColorfulShip<BlueShip>(),
            NewColorfulShip<BlackShip>(),
            NewColorfulShip<WhiteShip>()
        };
    }

    private ColorfulAiShip NewColorfulShip<T>() where T : ColorfulAiShip
    {
        var ship = Instantiate(ShipPrefab);

        T obj = (T)Activator.CreateInstance(typeof(T), new object[] { ship });

        return obj;
    }

    private void DestroyShips()
    {
        if (_ships != null && _ships.Count > 0)
        {
            foreach (var ship in _ships)
            {
                Destroy(ship.Prefab);
            }
        }

        _ships = new List<IAIShip>();
    }

    private void ResetShips()
    {
        if (_ships.Count == 0) return;

        GenerationHealthManager.Instance.ResetShips();

        // if best weights is null, then the first set of random weights we get counts as "best"
        if (_bestWeights == null)
        {
            _ships[0].Movement.ResetNeuralNetwork();
            _bestWeights = _ships[0].Movement.Weights;
        }

        foreach (var ship in _ships)
        {
            ship.Movement.ResetPosition();

            ship.SetActive(true);

            ship.Evolve(_bestWeights);
            GenerationHealthManager.Instance.AddShip(ship.Color);
        }
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

    /// <summary>
    /// Toggles whether we persist to disk on every new high score.
    /// </summary>
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

    /// <summary>
    /// Indicate that a ship died. Pass in the weights so that they can be cached if needed.
    /// </summary>
    /// <param name="weights">The set of weights from the Neural Network of the ship that died</param>
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

    private void StartGeneration(int generation)
    {
        SetGeneration(generation);
        _scoreKeeper.ResetScore();
        _bestWeightsThisGeneration = null;
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

    private void SetHighScore(int score, int generation)
    {
        _bestWeightScore = score;
        _bestWeightGeneration = generation;
        _interface.SetHighScoreDisplay(score, generation);
    }
}
