namespace Assets.Scripts
{
    using System;
    using System.Collections.Generic;
    using UnityEngine;
    using System.Linq;

    public class Scroller : MonoBehaviour
    {
        public static Scroller Instance { get; set; }

        private Scorekeeper _scoreKeeper { get { return Scorekeeper.Instance; } }
        
        public GameObject Prefab;

        public Vector2 StartPosition;
        public float WallSpeed;

        private bool _isRunning { get; set; }
        private List<ScrollObject> _objects { get; set; }
        private float _startingGapHeight = 4.5f;
        private float _gapHeight;
        private float _startingVerticalShift = 0.0f;
        private float _verticalShift;
        private bool _objectsAreReset { get; set; }

        private float _perlinNoiseX { get; set; }
        private float _perlinNoiseY { get; set; }
        public bool Paused { get; set; }

        private void Awake()
        {
            Instance = this;
        }

        // Use this for initialization
        void Start()
        {
            InitObjects();
            RandomizePerlinNoiseLocation();
            _perlinNoiseX = (float)UnityEngine.Random.Range(0, 10000) / 10000;
        }

        // Update is called once per frame
        void Update()
        {
            Shift();
        }

        private void InitObjects()
        {
            _objects = new List<ScrollObject>();
            var objectsToSpawn = (int)Mathf.Ceil((StartPosition.x * 2) / 0.5625f);

            while (objectsToSpawn > 0)
            {
                Spawn();
                objectsToSpawn--;
            }
            ResetObjects();
        }

        private void ResetObjects()
        {
            if (_objects == null || _objectsAreReset)
            {
                return;
            }

            float nextX = StartPosition.x;

            transform.position = new Vector3();

            foreach (var obj in _objects)
            {
                obj.Obj.transform.position = new Vector3(nextX, 0, 0);
                obj.ResetGap();
                obj.ResetVerticalShift();
                nextX -= 0.5625f;
            }
            _objectsAreReset = true;
        }

        private ScrollObject Spawn()
        {
            var obj = Instantiate(Prefab) as GameObject;

            obj.transform.SetParent(transform);

            var scrollObj = new ScrollObject(obj);
            scrollObj.StartPosition = StartPosition;
            _objects.Add(scrollObj);

            return scrollObj;
        }

        private void Shift()
        {
            if (!_isRunning || !_objects.Any() || Paused) return;

            _objectsAreReset = false;
            Vector2 translateLeftAmount = Vector2.left * (float)Math.Round(WallSpeed * Time.deltaTime, 1);
            transform.Translate(translateLeftAmount);

            var orderedObjects = _objects.OrderBy(o => o.Obj.transform.position.x);

            var obj = orderedObjects.First();
            if (obj.Obj.transform.position.x < -StartPosition.x)
            {
                //todo: find a home for these settings (frequency (10) and height adjustment (-0.025f))
                if (DateTime.Now.Second % 10 == 0 && _gapHeight > 0.9f)
                {
                    _gapHeight -= 0.025f;
                }

                _verticalShift = (Mathf.PerlinNoise(_perlinNoiseX, _perlinNoiseY) - 0.5f) * 7.625f;
                
                
                _perlinNoiseX += 0.085f;

                var nextX = orderedObjects.Last().Obj.transform.position.x + 0.5625f;

                obj.ResetPosition(_gapHeight, _verticalShift, nextX);

                _scoreKeeper.IncrementScore();
            }
        }

        private void OnEnable()
        {
            AIManager.OnGenerationStart += OnGameStart;
            AIManager.OnGenerationEnd += OnGameEnd;

            GameManager.OnGameStart += OnGameStart;
            GameManager.OnGameEnd += OnGameEnd;
        }

        private void OnDisable()
        {
            AIManager.OnGenerationStart -= OnGameStart;
            AIManager.OnGenerationEnd -= OnGameEnd;

            GameManager.OnGameStart -= OnGameStart;
            GameManager.OnGameEnd -= OnGameEnd;
        }

        private void OnGameEnd()
        {
            _isRunning = false;
            ResetObjects();
        }

        private void OnGameStart()
        {
            ResetHeightAndShift();
            _isRunning = true;
            RandomizePerlinNoiseLocation();
        }

        private void RandomizePerlinNoiseLocation()
        {
            _perlinNoiseY = (float)UnityEngine.Random.Range(0, 10000) / 10000;
        }

        private void ResetHeightAndShift()
        {
            _verticalShift = _startingVerticalShift;
            _gapHeight = _startingGapHeight;
        }
    }
}