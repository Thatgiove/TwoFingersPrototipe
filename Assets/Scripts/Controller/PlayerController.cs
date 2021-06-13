using Assets.Scripts.Character;
using Assets.Scripts.Delegates;
using Assets.Scripts.Utils;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    void Start()
    {
    }

    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            RaycastHit hitInfo = new RaycastHit();
            bool hit = Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out hitInfo);
            
            if (hit && Utils.HasComponent<Enemy>(hitInfo.transform.gameObject))
            {
                EventManager<OnCharacterSelection>.Trigger?.Invoke(hitInfo.transform.gameObject); 
            }

        }
    }
}
