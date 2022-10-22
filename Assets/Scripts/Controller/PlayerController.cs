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

    void Awake()
    {
        player = GetComponent<Character>();
    }
    void Start()
    {
    }

    void Update()
    {

        ray = Camera.main.ScreenPointToRay(Input.mousePosition);

        if (Physics.Raycast(ray, out hit))
        {
            bool isOverUI = UnityEngine.EventSystems.EventSystem.current.IsPointerOverGameObject();
           
            //se il cursore passa sul nemico effettuo una rotazione sulla sua position
            if (Input.GetMouseButtonDown(0) && hit.transform.gameObject.GetComponent<Character>() && !isOverUI)
            {
                //Seleziono il personaggio SelectCharacter in CombatGame mode
                EventManager<OnCharacterSelection>.Trigger?.Invoke(hit.transform.gameObject);

                //se seleziono un nemico e sono in modalità attacco
                if (hit.transform.gameObject.GetComponent<AIController>() && player.combatMode == CombatMode.AimMode)
                {
                    RotateToCharacter(2f, gameObject, hit.transform.gameObject);
                    player.ActiveShoot(hit.transform.gameObject);
                }
            }
        }

        //reloading
        if (Input.GetKeyDown(KeyCode.R) && player && player.weapon && !player.isReloading)
        {
            player.Reload();
        }

        //passa il turno
        if (Input.GetKeyDown(KeyCode.Space) && player && !player.isReloading)
        {
            FindObjectOfType<CombatGameMode>().EndTurn();
        }

        //set Aim mode
        if (Input.GetMouseButtonDown(1))
        {
            player.SetCombatMode(CombatMode.AimMode);
        }
        if (Input.GetMouseButtonUp(1))
        {
            player.SetCombatMode(CombatMode.ShootingMode);
        }
    }

}
