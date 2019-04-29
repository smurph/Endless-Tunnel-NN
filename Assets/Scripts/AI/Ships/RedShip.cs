namespace Assets.Scripts.AI.Ships
{
    using UnityEngine;

    public class RedShip : ColorfulAiShip
    {
        public RedShip(GameObject myPrefab) : base(myPrefab, Color.red)
        {
            myPrefab.name = "Red Ship";
        }

        public override void Evolve(float[][][] bestWeights)
        {
            base.Evolve(bestWeights);
            AIMovement.VaryWeightsByAmount(0.75f);
        }
    }
}
