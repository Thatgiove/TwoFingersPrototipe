using Assets.Scripts.Character;
using Assets.Scripts.Delegates;
using Assets.Scripts.Utils;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    MouseLook mouseLook;
     void Awake()
    {
        //mouseLook = gameObject.GetComponent<MouseLook>();
    }
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
        //qui si entra nella modalità mira
        //if (Input.GetKey(KeyCode.Mouse1))
        //{
        //    if (gameObject.GetComponent<MouseLook>())
        //    {
        //       // print(gameObject.name);
        //        gameObject.GetComponent<MouseLook>().enabled = true;
        //    }
        //}
        //else
        //{
        //    gameObject.GetComponent<MouseLook>().enabled = false;
        //}
       
    }
}
