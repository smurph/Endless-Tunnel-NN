using Assets.Scripts;
using System;
using System.Collections.Generic;
using UnityEngine;

public class DynamicWallScroller : MonoBehaviour
{
    public static DynamicWallScroller Instance { get; set; }
    public bool Paused { get; set; }

    [SerializeField]
    private Transform _upperLeft;

    [SerializeField]
    private float _clip = 4f;

    [SerializeField]
    private Transform _lowerRight;
    
    [SerializeField]
    private int _segmentCount = 250;

    [SerializeField]
    private float _perlinResolution = 0.125f;

    [SerializeField]
    private float _perlinAmplitude = 4.25f;

    [SerializeField]
    private GameObject _upperWall;

    [SerializeField]
    private GameObject _lowerWall;

    [SerializeField]
    private float _gap;

    private float _currentGap;

    [SerializeField]
    private int _shiftsPerScore = 10;

    private int _shiftsSinceScore = 0;

    private DynamicWall _upperDynamicWall;
    private DynamicWall _lowerDynamicWall;
    
    private bool _isRunning = false;

    // Use this for initialization
    void Start ()
    {
        Instance = this;

        if (Perlin.Instance == null) Perlin.Instance = new Perlin(_segmentCount, _perlinResolution, _perlinAmplitude);
        
        SetupDynamicWalls();
    }

    private void SetupDynamicWalls()
    {
        if (_upperWall == null) throw new MissingComponentException("Upper Wall has not been assigned");
        if (_lowerWall == null) throw new MissingComponentException("Lower Wall has not been assigned");

        _upperDynamicWall = _upperWall.GetComponent<DynamicWall>();
        _lowerDynamicWall = _lowerWall.GetComponent<DynamicWall>();

        if (_upperDynamicWall == null || _lowerDynamicWall == null)
        {
            throw new MissingComponentException("Upper Wall and Lower Wall must both contain a DynamicWall script");
        }

        _upperDynamicWall.IsUpperWall = true;
        _lowerDynamicWall.IsUpperWall = false;

        _upperDynamicWall.UpperLeft = _upperLeft;
        _lowerDynamicWall.UpperLeft = _upperLeft;

        _upperDynamicWall.LowerRight = _lowerRight;
        _lowerDynamicWall.LowerRight = _lowerRight;

        _upperDynamicWall.SegmentCount = _segmentCount;
        _lowerDynamicWall.SegmentCount = _segmentCount;

        _upperDynamicWall.Clip = _clip;
        _lowerDynamicWall.Clip = _clip;

        ResetGap();

        Perlin.Instance.Reset(true);

        _upperDynamicWall.InitMesh();
        _lowerDynamicWall.InitMesh();
    }

    private void ResetGap()
    {
        _currentGap = _gap;
    }

    private void ResetMeshes()
    {
        ResetGap();
        Perlin.Instance.Reset(true);

        _upperDynamicWall.UpdateMesh(_currentGap);
        _lowerDynamicWall.UpdateMesh(_currentGap);
    }

    // Update is called once per frame
    void Update () {
        if (_isRunning && !Paused) ScrollWalls();
	}

    void ScrollWalls()
    {
        Perlin.Instance.NewOffset();

        _upperDynamicWall.UpdateMesh(_currentGap);
        _lowerDynamicWall.UpdateMesh(_currentGap);

        _shiftsSinceScore++;

        CheckForScore();
        CheckForGapChange();
    }

    private void CheckForScore()
    {
        if (_shiftsSinceScore >= _shiftsPerScore)
        {
            _shiftsSinceScore = 0;
            Scorekeeper.Instance.IncrementScore();
        }
    }

    private void CheckForGapChange()
    {
        if (Scorekeeper.Instance.Score % 50 == 0 && _currentGap > 1.25f)
        {
            _currentGap -= 0.025f;
        }
    }

    private void OnDisable()
    {
        AIManager.OnGenerationStart -= OnGameStart;
        AIManager.OnGenerationEnd -= OnGameEnd;

        GameManager.OnGameStart -= OnGameStart;
        GameManager.OnGameEnd -= OnGameEnd;
    }

    private void OnEnable()
    {
        AIManager.OnGenerationStart += OnGameStart;
        AIManager.OnGenerationEnd += OnGameEnd;

        GameManager.OnGameStart += OnGameStart;
        GameManager.OnGameEnd += OnGameEnd;
    }

    private void OnGameStart()
    {
        _isRunning = true;
    }

    private void OnGameEnd()
    {
        ResetGap();
        _isRunning = false;
        ResetMeshes();
    }
}
