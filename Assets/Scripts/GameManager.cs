using UnityEngine;

public class GameManager : MonoBehaviour {
    public static GameManager Instance { get; set; }

    public delegate void GameEvent();

    public static event GameEvent OnGameStart;
    public static event GameEvent OnGameEnd;

    public GameObject GameMenuPage;
    public GameObject PlayerShip;

    public float ShipSpeed;

    public bool IsRunning { get; private set; }

    private Scorekeeper _scoreKeeper { get { return Scorekeeper.Instance; } }

    public void Awake()
    {
        Instance = this;
    }

    public void Start()
    {
        _scoreKeeper.ResetScore();
    }

    public void StartGame()
    {
        PlayerShip.SetActive(true);
        GameMenuPage.SetActive(false);

        _scoreKeeper.ResetScore();
        IsRunning = true;

        OnGameStart();
    }

    public void GameOver()
    {
        OnGameEnd();
        IsRunning = false;

        GameMenuPage.SetActive(true);
        PlayerShip.SetActive(false);
    }
}
