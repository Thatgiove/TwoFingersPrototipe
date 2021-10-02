using Assets.Scripts.Character;
using Assets.Scripts.Delegates;
using Assets.Scripts.Utils;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    public bool enableRotation;
    Ray ray;
    RaycastHit hit;
    Transform enemy;

    void Awake()
    {
    }
    void Start()
    {
    }

    void Update()
    {

        ray = Camera.main.ScreenPointToRay(Input.mousePosition);

        if (Physics.Raycast(ray, out hit))
        {
            //se il cursore passa sul nemico effettuo una rotazione sulla
            //sua position
            if (Input.GetMouseButtonDown(0) && Utils.HasComponent<Enemy>(hit.transform.gameObject))
            {
                enemy = hit.transform;
                EventManager<OnCharacterSelection>.Trigger?.Invoke(hit.transform.gameObject);
                RotateToEnemy();
            }
        }
    }

    void RotateToEnemy()
    {
        Vector3 lookVector = enemy.position - gameObject.transform.position;
        lookVector.y = gameObject.transform.position.y;
        Quaternion rot = Quaternion.LookRotation(lookVector);
        transform.rotation = Quaternion.Slerp(transform.rotation, rot, 1);
    }
}
