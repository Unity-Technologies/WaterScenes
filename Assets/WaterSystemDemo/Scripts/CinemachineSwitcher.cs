using UnityEditor.Rendering;
using UnityEngine;
using UnityEngine.InputSystem;

public class CinemachineSwitcher : MonoBehaviour
{

    [SerializeField] private InputAction action;

    private Animator animator;

    private bool Virtualcamera1 = true;
    
    private void Awake()
    {
        animator = GetComponent<Animator>();
    }

    private void OnEnable()
    {
        action.Enable();
    }

    private void OnDisable()
    {
        action.Disable();
    }

    void Start()
    {
        action.performed += _ => SwitchState();
        
    }

    private void SwitchState()
    {
        if (Virtualcamera1)
        {
            animator.Play("State1");
        }

        else
        {
            animator.Play("State2");
        }
        Virtualcamera1 = !Virtualcamera1;
    }

}
