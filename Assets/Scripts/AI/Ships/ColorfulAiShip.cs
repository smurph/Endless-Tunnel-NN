namespace Assets.Scripts.AI.Ships
{
    using System;
    using UnityEngine;

    public abstract class ColorfulAiShip : IAIShip
    {
        private readonly AIMovement _movement;
        private readonly SpriteRenderer _renderer;
        private readonly GameObject _prefab;
        private readonly Color _color;

        public ColorfulAiShip(GameObject myPrefab, Color color)
        {
            if (myPrefab == null)
            {
                throw new ArgumentNullException("myPrefab");
            }

            _movement = myPrefab.GetComponent<AIMovement>();
            _renderer = myPrefab.GetComponent<SpriteRenderer>();
            _renderer.color = color;
            _prefab = myPrefab;
            _color = color;

            if (_movement == null)
            {
                throw new MissingComponentException(typeof(AIMovement).Name);
            }

            if (_renderer == null)
            {
                throw new MissingComponentException(typeof(SpriteRenderer).Name);
            }
        }

        public bool Active { get { return _prefab.activeSelf; } }

        public Color Color { get { return _color; } }

        public AIMovement AIMovement { get { return _movement; } }

        public SpriteRenderer Renderer { get { return _renderer; } }

        public GameObject Prefab { get { return _prefab; } }

        public virtual void Evolve(float[][][] bestWeights)
        {
            _movement.Weights = bestWeights;
        }

        public void SetActive(bool value)
        {
            _prefab.SetActive(value);
        }
    }
}
