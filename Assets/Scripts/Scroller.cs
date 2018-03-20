namespace Assets.Scripts
{
    using System;
    using System.Collections.Generic;
    using UnityEngine;

    public class Scroller : MonoBehaviour
    {
        private Scorekeeper _scoreKeeper { get { return Scorekeeper.Instance; } }
        
        public GameObject Prefab;

        public Vector2 StartPosition;
        public float WallSpeed;

        private bool _isRunning { get; set; }
        private List<ScrollObject> _objects { get; set; }
        private float _startingGapHeight = 4.5f;
        private float _gapHeight;
        private float _startingVerticalShift = 0.25f;
        private float _verticalShift;
        private bool _objectsAreReset { get; set; }

        // Use this for initialization
        void Start()
        {
            InitObjects();
        }

        // Update is called once per frame
        void Update()
        {
            Shift();
        }

        private void InitObjects()
        {
            _objects = new List<ScrollObject>();
            for(int x = 0; x < 40; x++)
            {
                Spawn();
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

            foreach (var obj in _objects)
            {
                obj.Obj.transform.position = new Vector3(nextX, 0, 0);
                obj.ResetGap();
                obj.ResetVerticalShift();
                nextX -= 0.55f;
            }
            _objectsAreReset = true;
        }

        private ScrollObject Spawn()
        {
            var obj = Instantiate(Prefab) as GameObject;

            var scrollObj = new ScrollObject(obj);
            scrollObj.StartPosition = StartPosition;
            _objects.Add(scrollObj);

            return scrollObj;
        }

        private void Shift()
        {
            if (!_isRunning) return;

            _objectsAreReset = false;
            foreach (var obj in _objects)
            {
                obj.Obj.transform.Translate(Vector2.left * WallSpeed);

                if (obj.Obj.transform.position.x < -StartPosition.x)
                {
                    obj.ResetPosition(_gapHeight, _verticalShift);

                    //todo: find a home for these settings (frequency (10) and height adjustment (-0.025f))
                    if (DateTime.Now.Second % 10 == 0 && _gapHeight > 0.9f)
                    {
                        _gapHeight -= 0.025f;
                    }

                    // -.14x+1
                    var currentY = obj.Obj.transform.position.y;

                    var maxWallShiftVariance = 0.4f;
                    var rangeMin = -maxWallShiftVariance;
                    var rangeMax = maxWallShiftVariance;

                    var slope = -0.125f;

                    if (_gapHeight <= 1.25f) slope *= 0.75f;

                    if (Mathf.Abs(currentY) > 0.05f)
                    {
                        if (currentY < 0)
                        {
                            rangeMin = ((-currentY * slope) + maxWallShiftVariance) * -1;
                        }
                        else if (currentY > 0)
                        {
                            rangeMax = (currentY * slope) + maxWallShiftVariance;
                        }
                    }

                    _verticalShift += UnityEngine.Random.Range(rangeMin, rangeMax);

                    _scoreKeeper.IncrementScore();
                }
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
        }

        private void ResetHeightAndShift()
        {
            _verticalShift = _startingVerticalShift;
            _gapHeight = _startingGapHeight;
        }
    }
}