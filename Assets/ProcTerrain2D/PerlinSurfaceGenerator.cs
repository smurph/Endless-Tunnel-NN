namespace Assets.Scripts.ProcTerrain2D
{
    using System.Collections.Generic;
    using System.Linq;
    using UnityEngine;

    public class PerlinSurfaceGenerator
    {
        public static PerlinSurfaceGenerator Instance { get; set; }

        public PerlinSurfaceGenerator()
        {
            Reset();
        }

        public PerlinSurfaceGenerator(int historyLength, float resolution, float amplitude)
        {
            Resolution = resolution;
            HistoryLength = historyLength;
            Amplitude = amplitude;

            Reset();
        }

        public float Resolution { get; private set;}

        public int HistoryLength { get; private set; }

        public float Amplitude { get; private set; }

        public List<float> Offsets { get { return _surfaceHeights.ToList();  } }

        private Vector2 _location;

        private Queue<float> _surfaceHeights;

        public void Reset(bool flat = false)
        {
            _location = new Vector2();

            RandomizeY();

            if (_surfaceHeights == null) _surfaceHeights = new Queue<float>();

            _surfaceHeights.Clear();

            if (flat)
            {
                InitFlatSurface();
            }
            else
            {
                InitRandomSurface();
            }
        }

        private void InitFlatSurface()
        {
            for (int i = 0; i < HistoryLength; i++)
            {
                _surfaceHeights.Enqueue(0f);
            }
        }

        private void InitRandomSurface()
        {
            for (int i = 0; i < HistoryLength; i++)
            {
                NextSurfaceHeight();
            }
        }

        private void RandomizeY()
        {
            _location.y = Random.Range(1, 99999) / 99999f;
        }

        public float NextSurfaceHeight()
        {
            _location.x += Resolution;

            var offset = (Mathf.PerlinNoise(_location.x, _location.y) - 0.5f) * Amplitude;
            _surfaceHeights.Enqueue(offset);

            if (_surfaceHeights.Count > HistoryLength) _surfaceHeights.Dequeue();

            return offset;
        }
    }
}
