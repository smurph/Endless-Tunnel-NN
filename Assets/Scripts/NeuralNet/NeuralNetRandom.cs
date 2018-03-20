namespace Assets.Scripts.NeuralNet
{
    using Assets.Scripts.Interfaces;
    using System;
    
    /// <summary>
    /// Default random class using UnityEngine.Random
    /// </summary>
    public class NeuralNetRandom : IRandom
    {
        public float NextFloat(float minimum, float maximum)
        {
            return UnityEngine.Random.Range(minimum, maximum);
        }
    }
}
