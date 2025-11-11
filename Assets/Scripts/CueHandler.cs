using UnityEngine;
using UnityEngine.InputSystem;

public class CueHandler : MonoBehaviour
{
    public InputActionReference _clickInputAction;

    public InputActionReference _leftController;
    public InputActionReference _rightController;



    private Rigidbody _cueRigidBody;
    private Vector3 _frontPos;
    private Vector3 _backPos;

    private void Start()
    {
        _cueRigidBody = GetComponent<Rigidbody>();
    }
    private void OnEnable()
    {
        _clickInputAction.action.performed += OnClickPerformed;
    }

    private void OnDisable()
    {
        _clickInputAction.action.performed -= OnClickPerformed;
    }

    private void Update()
    {
        UpdateCuePosition();
    }
    private void UpdateCuePosition()
    {
        _frontPos =_leftController.action.ReadValue<Vector3>();
        _backPos = _rightController.action.ReadValue<Vector3>();
        
    }
    private void OnClickPerformed(InputAction.CallbackContext context)
    {
        
    }
    


}
