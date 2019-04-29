using UnityEngine;

namespace Assets.Scripts
{
    public interface IAIShip
    {
        AIMovement Movement { get; }

        SpriteRenderer Renderer { get; }

        GameObject Prefab { get; }

        Color Color { get; }

        bool Active { get; }

        void SetActive(bool value);

        void Evolve(float[][][] bestWeights);
    }
}
