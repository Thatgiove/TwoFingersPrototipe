
using Assets.Scripts.Delegates;
using Assets.Scripts.Utils;
using Cinemachine;
using System.Linq;
using UnityEngine;

public class CameraManager : MonoBehaviour
{
    CinemachineStateDrivenCamera stateDrivenCamera;
    CinemachineVirtualCamera cameraSelected;
    Animator animator;
    bool dirRight = true;

    private void Awake()
    {
        animator = GetComponent<Animator>();
        stateDrivenCamera = GetComponent<CinemachineStateDrivenCamera>();

        //viene chiamato da CombatGameMode quando setto il personaggio in turno
        EventManager<OnSetCharacterInTurn>.Register(ChangeCameraState);
    }
    private void LateUpdate()
    {
        if (cameraSelected)
        {

            var dolly = cameraSelected.GetCinemachineComponent<CinemachineTrackedDolly>();

            if (!dolly) return;

            if (dolly.m_PathPosition >= Mathf.Round(1.0f))
            {
                dirRight = false;
            }
            else if (dolly.m_PathPosition <= 0)
            {
                dirRight = true;
            }


            if (dirRight)
            {
                dolly.m_PathPosition += 0.0001f;
            }
            else
            {
                dolly.m_PathPosition -= 0.0001f;
            }
        }
    }

    //TODO da rivere
    void ChangeCameraState(GameObject charInTurn)
    {
        if (!stateDrivenCamera) return;

        if (charInTurn.name == "Jennifer")
        {
            cameraSelected = (CinemachineVirtualCamera)stateDrivenCamera.ChildCameras.Where(c => c.name == "JenniferCamera").First();
            animator.Play("JenniferCamera");
        }
        else if (charInTurn.name == "SwatGuy")
        {
            cameraSelected = (CinemachineVirtualCamera)stateDrivenCamera.ChildCameras.Where(c => c.name == "SwatGuyCamera").First();

            animator.Play("SwatGuyCamera");
        }
        else
        {
            cameraSelected = (CinemachineVirtualCamera)stateDrivenCamera.ChildCameras.Where(c => c.name == "FieldCamera").First();
            animator.Play("FieldCamera");
        }
    }
}
