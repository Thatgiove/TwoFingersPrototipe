using UnityEngine;

namespace Assets.Scripts.Items
{
    public abstract class IItem : ScriptableObject
    {
        public int id;
        public new string name;
        public Sprite icon;
        public string description;
        public int order;
        public float buyValue;
        public float sellValue; 
        public float timeCost; 
        public abstract void Use(GameObject c); //DEPRECATED
        public abstract void TriggerAnimation(GameObject c);
    }
}
