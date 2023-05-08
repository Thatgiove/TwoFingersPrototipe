using Assets.Scripts.Character;
using Assets.Scripts.Controller;
using Assets.Scripts.Delegates;
using Assets.Scripts.Utils;
using UnityEngine;

public class PlayerController : BaseController
{
    public bool enableRotation;
    Ray ray;
    RaycastHit hit;
    Transform enemy;
    Character player;
    CharacterController characterController;
    Vector3 movementDirection;

    void Awake()
    {
        player = GetComponent<Character>();
        characterController = GetComponent<CharacterController>();
    }
    void Start()
    {
    }

    void Update()
    {
        Rotate();
        Move();


        //reloading
        //if (Input.GetKeyDown(KeyCode.R) && player && player.weapon && !player.isReloading)
        //{
        //    player.Reload();
        //}

        //passa il turno
        //if (Input.GetKeyDown(KeyCode.Space) && player && !player.isReloading)
        //{
        //    FindObjectOfType<CombatGameMode>().EndTurn();
        //}

        //set Aim mode
        //if (Input.GetMouseButtonDown(1))
        //{
        //    player.SetCombatMode(CombatMode.AimMode);
        //}
        //if (Input.GetMouseButtonUp(1))
        //{
        //    player.SetCombatMode(CombatMode.ShootingMode);
        //}
    }

    void Rotate()
    {
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

            if (Input.GetMouseButtonDown(0))
            {

                print(hit.transform.gameObject.name);
            }
        }
    }

    void Move()
    {
        var input = new Vector3(Input.GetAxis("Horizontal"), 0, Input.GetAxis("Vertical"));
        input = transform.TransformDirection(input);
        movementDirection = input;
        characterController.Move(movementDirection * 5 * Time.deltaTime);
    }
}
