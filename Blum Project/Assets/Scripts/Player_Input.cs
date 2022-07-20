using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
public class Player_Input : MonoBehaviour
{
    private Player_InputSystem _inputSystem;
    public delegate void Del_none();
    public event Del_none OnJumpJustPressed;
    public event Del_none OnJumpCancelled;
    public bool jumpPressed { get; private set; }
    public Vector2 moveInput { get; private set; }
    private void Awake()
    {
        _inputSystem = new Player_InputSystem();
    }
    private void MoveInput_performed(InputAction.CallbackContext ctx) => moveInput = ctx.ReadValue<Vector2>();
    private void MoveInput_canceled(InputAction.CallbackContext ctx) => moveInput = Vector2.zero;
    private void Jump_canceled(InputAction.CallbackContext ctx) { OnJumpCancelled?.Invoke(); jumpPressed = false; }
    private void Jump_started(InputAction.CallbackContext ctx) { OnJumpJustPressed?.Invoke(); jumpPressed = true; }

   
    //subscribe all input events and enable all inputs
    private void OnEnable()
    {
        _inputSystem.Player.MoveInput.performed += MoveInput_performed;
        _inputSystem.Player.MoveInput.canceled += MoveInput_canceled;
        _inputSystem.Player.Jump.canceled += Jump_canceled;
        _inputSystem.Player.Jump.performed += Jump_started;
        _inputSystem.Enable();
    }


    //unsubscribe all input events and disable all inputs
    private void OnDisable()
    {
        _inputSystem.Player.MoveInput.performed -= MoveInput_performed;
        _inputSystem.Player.MoveInput.canceled -= MoveInput_canceled;
        _inputSystem.Player.Jump.canceled -= Jump_canceled;
        _inputSystem.Player.Jump.performed -= Jump_started;
        _inputSystem.Disable();
    }
}
