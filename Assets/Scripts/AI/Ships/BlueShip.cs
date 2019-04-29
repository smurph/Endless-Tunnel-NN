namespace Assets.Scripts.AI.Ships
{
    using UnityEngine;

    public class BlueShip : ColorfulAiShip
    {
        public BlueShip(GameObject myPrefab) : base(myPrefab, Color.blue)
        {
            myPrefab.name = "Blue Ship";
        }

        public override void Evolve(float[][][] bestWeights)
        {
            base.Evolve(bestWeights);
            Movement.VaryWeightsByAmount(0.5f);
        }
    }
}
