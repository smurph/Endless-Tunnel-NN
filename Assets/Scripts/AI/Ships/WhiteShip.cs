namespace Assets.Scripts.AI.Ships
{
    using UnityEngine;

    public class WhiteShip : ColorfulAiShip
    {
        public WhiteShip(GameObject myPrefab) : base(myPrefab, Color.white)
        {
            myPrefab.name = "White Ship";
        }

        public override void Evolve(float[][][] bestWeights)
        {
            AIMovement.ResetNeuralNetwork();
        }
    }
}
