using Assets.Scripts.Character;
using Assets.Scripts.Controller;
using Assets.Scripts.Delegates;
using Assets.Scripts.Utils;
using System.Collections;
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
           
            //se il cursore passa sul nemico effettuo una rotazione sulla
            //sua position
            if (Input.GetMouseButtonDown(0) && hit.transform.gameObject.GetComponent<Character>() && !isOverUI)
            {
                EventManager<OnCharacterSelection>.Trigger?.Invoke(hit.transform.gameObject);
            }
        }
        //reloading
        if (Input.GetKeyDown(KeyCode.R) && player && player.weapon && !player.isReloading)
        {
            player.Reload();
        }

    }

}
