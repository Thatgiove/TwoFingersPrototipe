using UnityEngine;
using UnityEngine.AI;

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

        private void Update()
        {
            //Vector3 direction = target.position - transform.position;
            //transform.rotation =  Quaternion.LookRotation(direction);
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
