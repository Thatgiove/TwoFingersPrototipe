
using UnityEngine;
using UnityEngine.InputSystem;

public class CameraSwitcher : MonoBehaviour
{
    [SerializeField] InputAction action;
    Animator animator;
    bool fieldCamera = true;

    private void Awake()
    {
        animator = GetComponent<Animator>();
    }
    void Start()
    {
        action.performed += _ => SwitchState();
    }
    private void OnEnable()
    {
        action.Enable();
    }
    void SwitchState()
    {
        if (fieldCamera)
        {
            animator.Play("CombatCamera");
        }
        else
        {
            animator.Play("FieldCamera");
        }
        fieldCamera = !fieldCamera;

    }
    private void OnDisable()
    {
        action.Disable();
    }

}
