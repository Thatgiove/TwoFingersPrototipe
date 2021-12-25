using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets.Scripts.Items
{
    public abstract class IItem : ScriptableObject
    {
        public int id;
        public string name;
        public string description;
        public int order;
        public float buyValue;
        public float sellValue; 
        public abstract void Use(GameObject c);
    }
}
