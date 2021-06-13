using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Assets.Scripts.Utils
{
    public static class Utils
    {
        public static bool HasComponent<T>(this GameObject go) where T : Component
        {
            return go.GetComponentsInChildren<T>().FirstOrDefault() != null;
        }

        public static List<GameObject> Randomize(List<GameObject> list)
        {
            List<GameObject> randomizedList = new List<GameObject>();
            System.Random rnd = new System.Random();

            while (list.Count > 0)
            {
                int index = rnd.Next(0, list.Count);
                randomizedList.Add(list[index]);
                list.RemoveAt(index);
            }
            return randomizedList;
        }
    }
}
