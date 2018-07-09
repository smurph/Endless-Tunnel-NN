namespace Assets.Scripts
{
    using System.Collections.Generic;
    using System.Linq;
    using UnityEngine;

    public class Perlin
    {
        public static Perlin Instance { get; set; }

        public Perlin()
        {
            Reset();
        }

        public Perlin(int historyLength, float resolution, float amplitude)
        {
            Resolution = resolution;
            HistoryLength = historyLength;
            Amplitude = amplitude;

            Reset();
        }

        public float Resolution { get; private set;}

        public int HistoryLength { get; private set; }

        public float Amplitude { get; private set; }

        public List<float> Offsets { get { return _segmentOffsets.ToList();  } }

        private Vector2 _location;

        private Queue<float> _segmentOffsets;
        
        public void Reset(bool flat = false)
        {
            _location = new Vector2();

            RandomizeY();

            if (_segmentOffsets == null) _segmentOffsets = new Queue<float>();

            _segmentOffsets.Clear();

            if (flat)
            {
                InitOffsetsFlat();
            }
            else
            {
                InitOffsetsRandom();
            }
        }

        private void InitOffsetsFlat()
        {
            for (int i = 0; i < HistoryLength; i++)
            {
                _segmentOffsets.Enqueue(0f);
            }
        }

        private void InitOffsetsRandom()
        {
            for (int i = 0; i < HistoryLength; i++)
            {
                NewOffset();
            }
        }

        private void RandomizeY()
        {
            _location.y = Random.Range(1, 99999) / 99999f;
        }

        public float NewOffset()
        {
            _location.x += Resolution;

            var offset = (Mathf.PerlinNoise(_location.x, _location.y) - 0.5f) * Amplitude;
            _segmentOffsets.Enqueue(offset);

            if (_segmentOffsets.Count > HistoryLength) _segmentOffsets.Dequeue();

            return offset;
        }
    }
}
