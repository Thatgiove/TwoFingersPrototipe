using UnityEngine;

namespace Assets.Scripts.Items
{
    public abstract class IItem : ScriptableObject
    {
        public int id;
        public new string name;
        public string description;
        public int order;
        public float buyValue;
        public float sellValue; 
        public abstract void Use(GameObject c);
        public abstract void TriggerAnimation(GameObject c);
    }
}
