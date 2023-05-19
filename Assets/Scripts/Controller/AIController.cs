using Assets.Scripts.Character;
using Assets.Scripts.Controller;
using UnityEngine;
using UnityEngine.AI;

public class AIController : BaseController
{
    NavMeshAgent navMeshAgent;
    public Transform target;
    Enemy enemy;


    void Start()
    {
        navMeshAgent = GetComponent<NavMeshAgent>();
        enemy = GetComponent<Enemy>();
    }

    void Update()
    {
        if (!enemy.isDead)
        {
            //navMeshAgent.SetDestination(target.position);
            Vector3 direction = target.position - transform.position;
            transform.rotation = Quaternion.LookRotation(direction);
            //enemy.Shoot();
        }
    }
}
