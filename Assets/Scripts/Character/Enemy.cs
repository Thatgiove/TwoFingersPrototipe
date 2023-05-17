using UnityEngine;

namespace Assets.Scripts.Character
{
    class Enemy : Character
    {
        public Transform target;
        public GameObject myMesh;
        public Material selected;
        public Material normal;
  
        public float CalculateAttackTime(float maxTime)
        {
            return Random.Range(.5f, maxTime);
        }


        void ChangeMaterial(Material _material)
        {
            myMesh.GetComponent<Renderer>().material = _material;
        }

        private void OnMouseEnter()
        {
            ChangeMaterial(selected);
        }

        private void OnMouseExit()
        {
            ChangeMaterial(normal);
        }
    }
}
