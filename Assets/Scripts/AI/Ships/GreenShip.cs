namespace Assets.Scripts.AI.Ships
{
    using UnityEngine;

    public class GreenShip : ColorfulAiShip
    {
        public GreenShip(GameObject myPrefab) : base(myPrefab, Color.green)
        {
            myPrefab.name = "Green Ship";
        }

        public override void Evolve(float[][][] bestWeights)
        {
            base.Evolve(bestWeights);
            Movement.VaryWeightsByAmount(0.25f);
        }
    }
}
