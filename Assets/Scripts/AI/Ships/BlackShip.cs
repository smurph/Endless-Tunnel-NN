namespace Assets.Scripts.AI.Ships
{
    using UnityEngine;

    public class BlackShip : ColorfulAiShip
    {
        public BlackShip(GameObject myPrefab) : base(myPrefab, Color.black)
        {
            myPrefab.name = "Black Ship";
        }
    }
}
