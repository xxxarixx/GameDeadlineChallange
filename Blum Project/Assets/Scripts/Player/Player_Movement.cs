using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player_Movement : MonoBehaviour
{
    [SerializeField] private Player_References refer;

    [Header("Movement")]
    [SerializeField] private float speedOnGround = 500f;
    [SerializeField] private float speedInAir = 250f;
    private Vector2 _lastVelocityBeforeStop;
    private bool _canMove = true;
    private enum _MoveAxis
    {
        Horizontal,
        Verical
    }
    public float speed_current { get; private set; }

    [Header("Jumping")]
    public float justpressJumpVelocity = 5f;
    public AnimationCurve jumpVelocity;
    private bool _jumpReachedEnd = false;
    private bool _coyoteTimeUsed = false;
    private bool _coyoteTimeEnded = false;

    [Header("Grounded")]
    [SerializeField] private float grounded_DistancePlayerGround;
    [SerializeField] private LayerMask groundLayerMask;
    private float _jumpPressProgress = 0f;
    private bool _isPerformingJump = false;
    private float _maxJumpProgressTime;

    [Header("Debug")]
    [SerializeField] private bool grounded;
    [SerializeField]private Vector3 debug_rbVelocity;
    #region Unity Functions
    private void FixedUpdate()
    {
        Debug.Log(_canMove);
        _FixedUpdate_Debug();
        _GroundedAndCoyoteTime();
        _Movement_horizontal();
        _Jump_Action();
    }
    private void OnEnable()
    {
        _VeriableSetup();
        refer.input.OnJumpCancelled += _Input_OnJumpCancelled;
        refer.input.OnJumpJustPressed += _Input_OnJumpJustPressed;
    }

    private void OnDisable()
    {
        refer.input.OnJumpCancelled -= _Input_OnJumpCancelled;
        refer.input.OnJumpJustPressed -= _Input_OnJumpJustPressed;
    }
    private void OnDrawGizmos()
    {
        Gizmos.color = (grounded)?Color.green : Color.red;
        Gizmos.DrawLine(refer.flip_Pivolt.position, refer.flip_Pivolt.position + Vector3.down * grounded_DistancePlayerGround);
    }
    #endregion


    #region Private Functions
    private IEnumerator _CoyoteTimeCountDown()
    {
        _coyoteTimeUsed = true;
        grounded = true;
        yield return new WaitForSeconds(.15f);
        _coyoteTimeEnded = true;
    }
    private void _GroundedAndCoyoteTime()
    {
        var localGrounded = Physics2D.Raycast(refer.flip_Pivolt.position, Vector2.down, grounded_DistancePlayerGround, groundLayerMask);
        if (_coyoteTimeUsed && _coyoteTimeEnded || localGrounded)
        {
            grounded = localGrounded;
            if (grounded)
            {
                _coyoteTimeUsed = false;
                _coyoteTimeEnded = false;
                StopCoroutine(_CoyoteTimeCountDown());
            }
        }
        if (!localGrounded && !_coyoteTimeUsed)
        {
            StartCoroutine(_CoyoteTimeCountDown());
        }
    }
    private void _Input_OnJumpJustPressed()
    {
        if (!_canMove) return;
        _isPerformingJump = (grounded) ? true : false;
        if (_isPerformingJump)
        {
            refer.PlayAnimation(Player_References.animations.jump,0);
        }
    }
    private void _Input_OnJumpCancelled()
    {
        _Jump_Reset();
    }
    private void _VeriableSetup()
    {
        _maxJumpProgressTime = jumpVelocity.keys[jumpVelocity.length - 1].time;
        speed_current = speedOnGround;
    }
    private void _Jump_Perform()
    {
        _jumpPressProgress += Time.deltaTime;
        _Move(jumpVelocity.Evaluate(_jumpPressProgress), _MoveAxis.Verical);
        _jumpReachedEnd = false;
        if(_jumpPressProgress >= _maxJumpProgressTime)
        {
            _jumpReachedEnd = true;
            _Jump_Reset();
        }
    }
    private void _Jump_Reset()
    {
        //reset player velocity Y if player doesnt reached end jump curve velocity and pressed up jump key
        if (!_jumpReachedEnd)
        {
            refer.rb.velocity = new Vector2(refer.rb.velocity.x, justpressJumpVelocity);
            _jumpReachedEnd = true;
        }
        _isPerformingJump = false;
        _jumpPressProgress = 0f;
    }
    private void _Jump_Action()
    {
        if (refer.input.jumpPressed && _isPerformingJump) _Jump_Perform();
        if (refer.rb.velocity.y < 0 && !grounded && _jumpReachedEnd) 
        { 
            refer.PlayAnimation(Player_References.animations.jump, 0); 
        }
    }
    private void _Movement_horizontal()
    {
        _lastVelocityBeforeStop = (refer.input.moveInput != Vector2.zero) ? refer.input.moveInput : _lastVelocityBeforeStop;
        if (!_canMove) return;
        _Move(speed_current, _MoveAxis.Horizontal);
        _FlipBasedOnVelocity();
        if (grounded && !_isPerformingJump)
        {
            if(Mathf.Abs(refer.rb.velocity.x) > 0f)
            {
                refer.PlayAnimation(Player_References.animations.walk,0);
            }
            else
            {
                refer.PlayAnimation(Player_References.animations.idle,0);
            }
        }

        if (!grounded)
        {
            _ChangeSpeed(speedInAir);
        }
        else
        {
            _ChangeSpeed(speedOnGround);
        }
    }
    private void _ChangeSpeed(float _newSpeed)
    {
        speed_current = _newSpeed;
    }
    private void _Move(float _speed, _MoveAxis _axis)
    {
        if (!_canMove) return;
        //axis that player should move
        Vector2 axis = (_axis == _MoveAxis.Horizontal)? new Vector2(1,0) : new Vector2(0,1);
        //invert axis to get velocity that shouldnt be changed by this movement
        Vector2 invertedAxis = (_axis == _MoveAxis.Horizontal) ? new Vector2(0, 1) : new Vector2(1, 0);
        refer.rb.velocity = axis * refer.input.moveInput * _speed * Time.fixedDeltaTime + invertedAxis * refer.rb.velocity;
    }
    public void MoveIndependentOnPlayerInput(float _speed, Vector2 _direction, bool _beEffectedByMoveState = true)
    {
        if (!_canMove && _beEffectedByMoveState) return;
        Vector2 axisToLeave = new Vector2((_direction.x == 0) ? 1f : 0f, (_direction.y == 0) ? 1f : 0f);
        refer.rb.velocity = _direction * _speed * Time.fixedDeltaTime + axisToLeave * refer.rb.velocity;
    }
    private void _FixedUpdate_Debug()
    {
        debug_rbVelocity = refer.rb.velocity;
    }
    private void _FlipBasedOnVelocity()
    {
        var moveDir = (refer.rb.velocity.x == 0)? _lastVelocityBeforeStop.x : refer.rb.velocity.x;
        refer.flip_Pivolt.localScale = new Vector3((moveDir > 0f) ? Mathf.Abs(refer.flip_Pivolt.localScale.x) : -Mathf.Abs(refer.flip_Pivolt.localScale.x), refer.flip_Pivolt.localScale.y, refer.flip_Pivolt.localScale.z);
    }
    #endregion

    #region Public Functions
    public void SetMoveState(bool _canMove)
    {
        this._canMove = _canMove;
        if (!_canMove) refer.rb.velocity = Vector2.zero;
    }
    public void StopPlayerMove()
    {
        refer.rb.velocity = Vector2.zero;
    }
    public void StopPlayerMove(Vector2 offsetVelocity)
    {
        refer.rb.velocity = Vector2.zero + offsetVelocity;
    }
    #endregion
}
