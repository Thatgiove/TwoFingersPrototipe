using UnityEngine;

namespace Assets.Scripts.Character
{
    class Enemy : Character
    {
        
        public float CalculateAttackTime(float maxTime)
        {
            return Random.Range(.5f, maxTime);
        }
    }
}
