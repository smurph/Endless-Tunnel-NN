namespace Assets.Scripts.NeuralNet
{
    using Assets.Scripts.Interfaces;
    using System;
    using System.Collections.Generic;

    public class Network
    {
        private float[][] _neurons { get; set; }

        public float[][][] Weights { get; set; }

        public float[][] Biases { get; set; }
        
        private IRandom _rnd { get; set; }

        public Network(int inputNeuronCount, int outputNeuronCount, int[] middleLayerNeuronCounts, float[][][] weights = null, bool useBias = false, IRandom random = null)
        {
            var neurons = new List<float[]>();

            if (random == null)
            {
                _rnd = new NeuralNetRandom();
            }
            else
            {
                _rnd = random;
            }

            neurons.Add(new float[inputNeuronCount]);

            foreach (var size in middleLayerNeuronCounts)
            {
                neurons.Add(new float[size]);
            }

            neurons.Add(new float[outputNeuronCount]);

            _neurons = neurons.ToArray();

            if (useBias)
            {
                InitRandomBiases();
            }
            else
            {
                // Set biases all to 1
                InitBiasesToOne();
            }

            if (weights == null)
            {
                InitRandomWeights();
            }
            else
            {
                Weights = weights;
            }
        }

        #region Init methods

        private void InitRandomWeights(float variance = 1f)
        {
            var weights = new List<float[][]>();
            for (int x = 1; x < _neurons.Length; x++)
            {
                var layerWeights = new List<float[]>();
                for (int y = 0; y < _neurons[x].Length; y++)
                {
                    var neuronWeights = new float[_neurons[x - 1].Length];
                    for (int n = 0; n < _neurons[x - 1].Length; n++)
                    {
                        neuronWeights[n] = _rnd.NextFloat(-variance, variance);
                    }
                    layerWeights.Add(neuronWeights);
                }
                weights.Add(layerWeights.ToArray());
            }
            Weights = weights.ToArray();
        }

        // If all biases are 1, it will effectively remove bias in calculations later on.
        private void InitBiasesToOne()
        {
            var biases = new List<float[]>();
            
            for (int x = 1; x < _neurons.Length; x++)
            {
                var layer = new float[_neurons[x].Length];
                for (int y = 0; y < _neurons[x].Length; y++)
                {
                    layer[y] = 1;
                }
                biases.Add(layer);
            }

            Biases = biases.ToArray();
        }

        private void InitRandomBiases(float variance = 1)
        {
            var biases = new List<float[]>();
            
            for (int x = 1; x < _neurons.Length; x++)
            {
                var layer = new float[_neurons[x].Length];
                for (int y = 0; y < _neurons[x].Length; y++)
                {
                    layer[y] = _rnd.NextFloat(-variance, variance);
                }
                biases.Add(layer);
            }

            Biases = biases.ToArray();
        }

        #endregion

        public void SetWeightsWithVariance(float[][][] baseWeights, float variance = 1f)
        {
            var weights = new List<float[][]>();
            for (int x = 1; x < _neurons.Length; x++)
            {
                var layerWeights = new List<float[]>();
                for (int y = 0; y < _neurons[x].Length; y++)
                {
                    var neuronWeights = new float[_neurons[x - 1].Length];
                    for (int n = 0; n < _neurons[x - 1].Length; n++)
                    {
                        neuronWeights[n] = baseWeights[x - 1][y][n] + _rnd.NextFloat(-variance, variance); 

                        // clamp to (-1.0, 1.0)
                        if (neuronWeights[n] > 1.0f) neuronWeights[n] = 1.0f;
                        if (neuronWeights[n] < -1.0f) neuronWeights[n] = -1.0f;
                    }
                    layerWeights.Add(neuronWeights);
                }
                weights.Add(layerWeights.ToArray());
            }
            Weights = weights.ToArray();
        }

        public void SetBiasesWithVariance(float[][] baseBiases, float variance)
        {
            var biases = new List<float[]>();

            // skip input layer
            for (int x = 1; x < _neurons.Length; x++)
            {
                var layerBiases = new float[_neurons[x].Length];
                for (int y = 0; y < _neurons[x].Length; y++)
                {
                    layerBiases[y] = baseBiases[x-1][y] + _rnd.NextFloat(-variance, variance);
                }
                biases.Add(layerBiases);
            }

            Biases = biases.ToArray();
        }

        private void SetInputs(float[] inputs, bool activateInputs=false)
        {
            if (inputs.Length != _neurons[0].Length) throw new ArgumentException("Must provide " + _neurons[0].Length.ToString() + " inputs to this neural network.");

            for(int y = 0; y < _neurons[0].Length; y++)
            {
                _neurons[0][y] = activateInputs ? Activate(inputs[y]) : inputs[y];
            }
        }

        private float Activate(float input)
        {
            return (float)Math.Tanh(input);
        }

        public void Calculate(float[] inputs)
        {
            SetInputs(inputs);

            for (int x = 1; x < _neurons.Length; x++)
            {
                for(int y = 0; y < _neurons[x].Length; y++)
                {
                    _neurons[x][y] = 0;
                    for (int n = 0; n < _neurons[x - 1].Length; n++)
                    {
                        _neurons[x][y] += _neurons[x - 1][n] * Weights[x - 1][y][n];
                    }
                    _neurons[x][y] = Activate(_neurons[x][y] * Biases[x - 1][y]);
                }
            }
        }

        public int GetHighestOutputNeuronIndex()
        {
            var outputLayerIndex = _neurons.Length - 1;

            int highestIndex = 0;
            float highestValue = float.MinValue;
            for(int x = 0; x < _neurons[outputLayerIndex].Length; x++)
            {
                if (_neurons[outputLayerIndex][x] > highestValue)
                {
                    highestValue = _neurons[outputLayerIndex][x];
                    highestIndex = x;
                }
            }

            return highestIndex;
        }
    }
}
