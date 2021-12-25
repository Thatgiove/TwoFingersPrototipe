using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets.Scripts.Items
{
    [CreateAssetMenu]
    [Serializable]
    public class StatusItem : IItem
    {
        public override void Use(GameObject c)
        {
            Debug.Log(name + " " + description);
        }
    }
}
