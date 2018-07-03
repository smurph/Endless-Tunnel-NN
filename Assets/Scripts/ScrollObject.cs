namespace Assets.Scripts
{
    using System;
    using UnityEngine;

    public class ScrollObject
    {
        public bool IsActive { get; private set; }
        public Vector2 StartPosition { get; set; }
        public GameObject Obj { get; private set; }
                
        public ScrollObject(GameObject obj)
        {
            Obj = obj;
            Deactivate();
        }

        public void Activate()
        {
            ResetPosition(0, 0, StartPosition.x);
            IsActive = true;
        }

        public void Deactivate()
        {
            Obj.transform.position = new Vector3(1000, 0, 0);
            IsActive = false;
        }

        public void ResetPosition(float gapHeight, float verticalShiftAmount, float newX)
        {
            Obj.transform.position = new Vector3(newX, StartPosition.y);

            if (gapHeight > 0)
            {
                SetGap(gapHeight);
                ShiftVertical(verticalShiftAmount);
            }
        }

        public void Narrow(float amount)
        {
            var upperWall = Obj.transform.Find("upper_wall");
            var lowerWall = Obj.transform.Find("lower_wall");

            upperWall.transform.Translate(new Vector3(0, -amount, 0));
            lowerWall.transform.Translate(new Vector3(0, amount, 0));
        }
        
        public void ResetGap()
        {
            var upperWall = Obj.transform.Find("upper_wall");
            var lowerWall = Obj.transform.Find("lower_wall");

            upperWall.transform.position = new Vector3(upperWall.transform.position.x, 9, 0);
            lowerWall.transform.position = new Vector3(lowerWall.transform.position.x, -9, 0);
        }

        public void SetGap(float amount)
        {
            float newY = 9f;
            if (amount < 0)
            {
                amount = 0;
            }

            newY = (amount / 2) + 5.25f;

            var upperWall = Obj.transform.Find("upper_wall");
            var lowerWall = Obj.transform.Find("lower_wall");

            upperWall.transform.position = new Vector3(upperWall.transform.position.x, newY, 0);
            lowerWall.transform.position = new Vector3(lowerWall.transform.position.x, -newY, 0);
        }

        public void ResetVerticalShift()
        {
            Obj.transform.position = new Vector3(Obj.transform.position.x, 0);
        }

        public void ShiftVertical(float amount)
        {
            Obj.transform.Translate(new Vector3(0, amount));

            var upperWall = Obj.transform.Find("upper_wall");
            var lowerWall = Obj.transform.Find("lower_wall");

            if (upperWall.transform.position.y > 9)
            {
                upperWall.transform.position = new Vector3(upperWall.transform.position.x, 9);
            }

            if (lowerWall.transform.position.y < -9)
            {
                lowerWall.transform.position = new Vector3(lowerWall.transform.position.x, -9);
            }
        }
    }
}
