using Assets.Scripts.Character;
using Assets.Scripts.Controller;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.InputSystem.HID;


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
        navMeshAgent.SetDestination(target.position);

        if (enemy != null && enemy.isDead) { navMeshAgent.isStopped = true; }
    }
}
