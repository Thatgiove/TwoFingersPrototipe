
using Assets.Scripts.Character;
using System.Collections;
using UnityEngine;

namespace Assets.Scripts.Controller
{
    public class BaseController : MonoBehaviour
    {
        //TODO - Questo va nel controller così alla morte posso disattivarlo
        //TODO - va reso generico per tutti i tipi di azioni
        //I personaggi si girano verso il nemico 
        //e attaccano alla fine della rotazione
        //alla fine dell'animazione dell'attacco 
        //emette un evento intercettato da combatGameMode
        //che fa passare il turno
        public IEnumerator PerformActionAtTheEndOfRotation(
            float lerpTime,
            Assets.Scripts.Character.Character shooter,
            Assets.Scripts.Character.Character receiver,
            ActionType actionType
            )
        {
           
            float elapsedTime = 0f;

            while (elapsedTime <= lerpTime)
            {
                if (receiver.gameObject.transform.position != shooter.transform.position)
                {
                    shooter.transform.rotation = Quaternion.Slerp(
                                     shooter.transform.rotation,
                                     Quaternion.LookRotation(receiver.gameObject.transform.position - shooter.transform.position, Vector3.up),
                                     elapsedTime / lerpTime);
                }

                elapsedTime += (Time.deltaTime * 10f);
                yield return null;
            }

            //TODO -- la rotazione non avviene solo durante l'attacco , anche se uso oggetti o abilità
            if (actionType == ActionType.Item)
            {
                actionType = ActionType.None;
                shooter.TriggerAnimation("throw");
            }
            else
            {
                actionType = ActionType.None;
                shooter.Shoot();
            }

        }
    }
}
