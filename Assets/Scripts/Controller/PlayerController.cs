using Assets.Scripts.Character;
using Assets.Scripts.Controller;
using UnityEngine;
using UnityEngine.AI;

public class PlayerController : BaseController
{
    float velocity = 10f;

    Transform enemy;
    Character character;

    CharacterController characterController;
    Vector3 movementDirection;
    NavMeshAgent navMeshAgent;
    CameraFollow cameraFollow;

    void Awake()
    {
        character = GetComponent<Character>();
        characterController = GetComponent<CharacterController>();
        cameraFollow = Camera.main.GetComponent<CameraFollow>();
    }
    void Start()
    {
        navMeshAgent = GetComponent<NavMeshAgent>();
    }

    void Update()
    {
        Rotate();
        Move();

        //With navmesh
        //if (Input.GetMouseButtonDown(0))
        //{
        //    MoveToClickPoint();
        //}
        //if (Input.GetMouseButton(1))
        //{
        //    RotateCharacter();
        //}
    }

    void MoveToClickPoint()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;

        if(Physics.Raycast(ray, out hit))
        {
            if (hit.collider.CompareTag("Ground"))
            {
                navMeshAgent.SetDestination(hit.point);
            }
            else if (hit.transform.GetComponent<Enemy>())
            {
                Vector3 direction = hit.transform.position - transform.position;
                transform.rotation = Quaternion.LookRotation(direction);

                hit.transform.GetComponent<Character>().TakeDamage(1.5f); //TODO - weapom damage
            }
        }
    }

    void RotateCharacter()
    {
        float mouseX = Input.GetAxis("Mouse X");

        // Calcola l'angolo di rotazione in base alla velocità di rotazione e all'input del mouse
        float rotationAngle = mouseX * 7;

        // Applica la rotazione sull'asse Y
        transform.Rotate(Vector3.up, rotationAngle);
    }
    void Rotate()
    {
        if (cameraFollow.isRotating) return;

        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit))
        {
            //Debug.DrawRay(ray.origin, ray.direction * hit.distance, Color.red);

            Vector3 targetPosition = ray.GetPoint(hit.distance);
            Vector3 direction = targetPosition - transform.position;
            direction.y = 0f; // Imposta la componente Y a 0 per ruotare solo intorno all'asse Y

            Quaternion targetRotation = Quaternion.LookRotation(direction);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, 12 * Time.deltaTime);

            if (hit.transform.GetComponent<Enemy>())
            {
                enemy = hit.transform;

                if (Input.GetMouseButtonDown(0))
                {
                    character.Shoot();
                    hit.transform.GetComponent<Character>().TakeDamage(1.5f); //TODO - weapom damage
                }
                    
            }
            else
            {
                enemy = null;
            }
        }
    }

    void Move()
    {
        var input = new Vector3(Input.GetAxis("Horizontal"), 0, Input.GetAxis("Vertical"));
        input = transform.TransformDirection(input);
        movementDirection = input;
        characterController.Move(movementDirection * velocity * Time.deltaTime);
    }
}
