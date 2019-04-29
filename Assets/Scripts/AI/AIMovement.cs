namespace Assets.Scripts
{
    using System.Collections.Generic;
    using UnityEngine;

    [RequireComponent(typeof(SpriteRenderer))]
    public class AIMovement : MonoBehaviour
    {
        private AIController _aiController { get { return AIController.Instance; } }
        private Scorekeeper _scoreKeeper { get { return Scorekeeper.Instance; } }

        private NeuralNet.Network _neuralNetwork;

        private bool IsRunning = false;
        private float _lastMoveValue;

        public float[][][] Weights
        {
            get {
                return _neuralNetwork.Weights;
            }

            set
            {
                _neuralNetwork.Weights = value;
            }
        }

        private void OnEnable()
        {
            AIController.OnGenerationStart += OnGameStart;
            AIController.OnGenerationEnd += OnGameEnd;

            ResetNeuralNetwork();
        }

        private void OnDisable()
        {
            AIController.OnGenerationStart -= OnGameStart;
            AIController.OnGenerationEnd -= OnGameEnd;
        }

        public void ResetNeuralNetwork()
        {
            _neuralNetwork = new NeuralNet.Network(6, 3, new int[] { 5 });
        }

        public void VaryWeightsByAmount(float variance)
        {
            _neuralNetwork.SetWeightsWithVariance(_neuralNetwork.Weights, variance);
        }

        public void Update()
        {
            if (!IsRunning || _aiController.Paused) return;

            CalculateAndMove();
        }

        private void CalculateAndMove()
        {
            if (_scoreKeeper.Score <= 0) return;

            _neuralNetwork.Calculate(GetInputValues());

            var nextMoveIndex = _neuralNetwork.GetHighestOutputNeuronIndex();

            if (nextMoveIndex > 0)
            {
                transform.position = (new Vector3(transform.position.x, transform.position.y + ((nextMoveIndex == 1 ? 1 : -1) * Time.deltaTime * _aiController.ShipSpeed)));
                _lastMoveValue = (nextMoveIndex == 1 ? 1.0f : -1.0f);
            }
            else
            {
                _lastMoveValue = 0;
            }
        }

        private void OnGameStart()
        {
            IsRunning = true;
        }

        private void OnGameEnd()
        {
            IsRunning = false;
        }

        private float[] GetInputValues()
        {
            var distances = new List<float>();
            var origin = new Vector2(transform.position.x + 0.4f, transform.position.y);
            var raycastDistance = 22f;

            var upRight = new Vector2(1, 1);
            var downRight = new Vector2(1, -1);

            distances.Add(GetRayCastDistance(new Vector2(transform.position.x, transform.position.y + 0.2f), Vector2.up, raycastDistance));
            distances.Add(GetRayCastDistance(origin, upRight, raycastDistance));
            distances.Add(GetRayCastDistance(origin, Vector2.right, raycastDistance));
            distances.Add(GetRayCastDistance(origin, downRight, raycastDistance));
            distances.Add(GetRayCastDistance(new Vector2(transform.position.x, transform.position.y - 0.2f), Vector2.down, raycastDistance));
            distances.Add(_lastMoveValue);

            return distances.ToArray();
        }

        private float GetRayCastDistance(Vector2 origin, Vector2 dir, float raycastDistance)
        {
            var wallMask = 1 << LayerMask.NameToLayer("Walls");
            var ray = Physics2D.Raycast(origin, dir, raycastDistance, wallMask);
            if (ray.collider)
            {
                Debug.DrawRay(origin, transform.InverseTransformDirection(ray.point - origin), Color.red, Time.deltaTime);
                return ray.distance;
            }

            Debug.DrawRay(origin, dir * raycastDistance, Color.red, Time.deltaTime);
            return raycastDistance;
        }

        private void OnTriggerEnter2D(Collider2D collider)
        {
            if (collider.tag == "wall")
            {
                if (IsRunning) _aiController.ShipDied(Weights);
                IsRunning = false;
                gameObject.SetActive(false);
                var renderer = gameObject.GetComponent(typeof(SpriteRenderer)) as SpriteRenderer;

                GenerationHealthManager.Instance.ShipDied(renderer.color);
            }
        }

        public void ResetPosition()
        {
            transform.position = new Vector3(transform.position.x, 0);
        }
    }
}
