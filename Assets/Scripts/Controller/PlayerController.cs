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
            //se il cursore passa sul nemico effettuo una rotazione sulla
            //sua position
            if (Input.GetMouseButtonDown(0) && Utils.HasComponent<Enemy>(hit.transform.gameObject))
            {
                enemy = hit.transform;
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
