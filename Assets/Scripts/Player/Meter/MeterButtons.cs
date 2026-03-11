using UnityEngine;
using Flyfe.UI;

namespace Flyfe.Player
{
    public class MeterButtons : MonoBehaviour
    {
        public MeterScript timeMeter; 
        public int currentTime; 
        public int maxTime = 80; 

        void Start()
        {
            currentTime = maxTime; 
            if (timeMeter != null)
            {
                timeMeter.SetMaxTime(maxTime);
            }
        }

        void FixedUpdate()
        {
            if (timeMeter != null)
            {
                timeMeter.SetTime(currentTime);
            }
        }

        public void Increase()
        {
            currentTime += 10;
        }

        public void Decrease()
        {
            currentTime -= 10;
        }
    }
}
