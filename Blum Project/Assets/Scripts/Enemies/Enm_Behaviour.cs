using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using CustomEditorAssistance;
public class Enm_Behaviour : MonoBehaviour
{
    #region enums
    public enum EnemyType
    {
        PatrolJust,
        PatrolWHitBoxChase
    }
    private enum _MoveAxis
    {
        Horizontal,
        Vertical
    }
    private enum _MoveSpeed
    {
        OnGround,
        InAir
    }
    #endregion
    public Enm_References data;

    public EnemyType enemyType;

    [Header("allType")]
    [SerializeField] private int damagePerHit;
    public float currentSpeed { get; private set; }
    public float inAirSpeedMultiplayer = .7f;
    [SerializeField] private float maxGroundSpeed = 100f;
    [SerializeField] private float maxChaseSpeed = 200f;
    [SerializeField] private LayerMask groundMask;


    [Header("patrolWRadialChase")]
    [SerializeField]private Vector3 chaseHitBox;
    [SerializeField]private Vector3 hitBoxSize;
    public float attackSpeed;
    [SerializeField] private LayerMask groundAndPlayerMask;

    [Header("Debug")]
    //speed with modifires
    private float _speedInAir;
    private float _speedGrounded;
    //damage dealer
    private float _attackSpeedCooldown;
    //Raycasts length
    private float _groundRaycastLength = 0.3f;
    private float _groundPatrolDirecitonRaycastLength = 0f;
    //move rules
    private float _moveDirX = 1f;
    private bool _stopMove;
    private bool _grounded;
    private bool _groundedPatrolDireciton;
    private bool _shouldPerformAttack;
    private bool _attackAnimationEnded;

    #region Unity functions
    private void OnValidate()
    {
        attackSpeed = Mathf.Clamp(attackSpeed, 0f, float.MaxValue);
    }
    private void Start()
    {
        _SpeedSetup();
        _MoveSetup();
        _RaycastsLengthSetup();
        data.healthSystem.OnGetHitted += _HealthSystem_OnGetHitted;
    }


    private void FixedUpdate()
    {
        if (_stopMove) data.PlayAnimation(Enm_References.animations.idle,0);
        switch (enemyType)
        {
            case EnemyType.PatrolJust:
                _PatrolMovement();
                break;
            case EnemyType.PatrolWHitBoxChase:
                _AttackValidate();
                _ChaseMovement(out bool isChasing);
                if(!isChasing)_PatrolMovement();
                break;
            default:
                break;
        }

    }
    private void OnDrawGizmos()
    {
        _RaycastsLengthSetup();
        _Draw_Raycast_Grounded();
        _Draw_Raycast_GroundedPatrolDireciton();
        _Draw_Raycast_InFront();
        switch (enemyType)
        {
            case EnemyType.PatrolJust:

                break;
            case EnemyType.PatrolWHitBoxChase:
                _Draw_ChaseHitBox();
                _Draw_HitBox();
                _Draw_Raycast_AttackValidate();
                _Draw_CanChase_Raycast();
                break;
            default:
                break;
        }
    }
    #endregion

    #region Custom Private Functions
    private void _SpeedSetup()
    {
        _speedGrounded = maxGroundSpeed + Random.Range(-currentSpeed / 10, currentSpeed / 10);
        currentSpeed = _speedGrounded;
        _speedInAir = currentSpeed * inAirSpeedMultiplayer;
    }
    private void _MoveSetup()
    {
        _moveDirX = 1f;
    }
    private void _RaycastsLengthSetup()
    {
        _groundPatrolDirecitonRaycastLength = data.grounded_Pivolt.localPosition.y + _groundRaycastLength + .1f;
    }
    private void _PatrolMovement()
    {
        if (_stopMove) return;
        _groundedPatrolDireciton = _HittingGroundedPatrolDireciton_Raycast();
        _grounded = _HittingGrounded_Raycast();
        if (!_groundedPatrolDireciton && _grounded || _HittingInFront_Raycast() && _grounded)
        {
            _Flip();
        }
        _Move(currentSpeed, _MoveAxis.Horizontal);
        if (_grounded)
        {
            data.PlayAnimation(Enm_References.animations.walk, 1);
            _SetCurrentMoveSpeed(_MoveSpeed.OnGround);
        }
        else
        {
            _SetCurrentMoveSpeed(_MoveSpeed.InAir);
        }
    }
    private void _HealthSystem_OnGetHitted(Vector3 _hitInvokerPosition)
    {
        //hitted from behind
        var dir = (_hitInvokerPosition - transform.position).normalized;
        var frontDir = _FrontDirectiong();
        if (Vector2.Dot(frontDir,dir) < 0) _Flip();
    }
    private void _AttackValidate()
    {
        //if in previous frame attack should be performed attack and was applied force stop movement and in next frame player went from attack validate raycast then unlock movement
        if(_stopMove && _shouldPerformAttack && !_HittingAttackValidate_Raycast() && _attackAnimationEnded) SetForceStopMovement(false);
        _shouldPerformAttack = _HittingAttackValidate_Raycast();
        if(_shouldPerformAttack) SetForceStopMovement(true);
        //performing attack cooldown
        if (_shouldPerformAttack && _attackSpeedCooldown <= 0f)
        {
            _attackAnimationEnded = false;
            data.PlayAnimation(Enm_References.animations.attack, 2);
            _attackSpeedCooldown = 1f / attackSpeed;
        }
        else if (_attackSpeedCooldown > 0f)
        {
            _attackSpeedCooldown -= Time.deltaTime;
        }
    }
    private void _ChaseMovement(out bool _isChasing)
    {
        _isChasing = false;
        if (_stopMove) return;
        var chaseHitBoxHit = Physics2D.OverlapBox(data.flip_Pivolt.position + (data.flip_Pivolt.localScale.x * (chaseHitBox.x / 2) * Vector3.right) + (chaseHitBox.y / 2) * Vector3.up, chaseHitBox, 0f, 1 << 8);
        if (chaseHitBoxHit != null)
        {
            //detect ground and player
            var canChaseRaycastHit = Physics2D.Raycast(data.flip_Pivolt.position + (chaseHitBox.y / 2) * Vector3.up, _FrontDirectiong(), chaseHitBox.x, groundAndPlayerMask);
            if(canChaseRaycastHit.collider != null) Debug.Log(canChaseRaycastHit.collider.name);
            if (canChaseRaycastHit.collider == null || canChaseRaycastHit.collider != null && canChaseRaycastHit.collider.gameObject.CompareTag("Player")) //if player hit first or nothing was hitted then chase player 
            {
                currentSpeed = maxChaseSpeed;
                data.PlayAnimation(Enm_References.animations.walk, 1);
                _Move(currentSpeed, _MoveAxis.Horizontal);
                _isChasing = true;
            }
            else
            {
                _isChasing = false;
            }
        }
        else
        {
            _isChasing = false;
        }
        if(!_isChasing) currentSpeed = maxGroundSpeed;
    }
    public void AttackAnimationEnded()
    {
        data.ResetAnimationPriority();
        SetForceStopMovement(false);
        _attackAnimationEnded = true;
    }
    private void _Move(float _speed, _MoveAxis _axis)
    {
        //axis that player should move
        Vector2 axis = (_axis == _MoveAxis.Horizontal) ? new Vector2(1, 0) : new Vector2(0, 1);
        //invert axis to get velocity that shouldnt be changed by this movement
        Vector2 invertedAxis = (_axis == _MoveAxis.Horizontal) ? new Vector2(0, 1) : new Vector2(1, 0);
        data.rb.velocity = axis * new Vector2(_moveDirX,1f) * _speed * Time.fixedDeltaTime + invertedAxis * data.rb.velocity;
    }
    private void _SetCurrentMoveSpeed(_MoveSpeed _moveSpeedType)
    {
        switch (_moveSpeedType)
        {
            case _MoveSpeed.OnGround:
                currentSpeed = _speedGrounded;
                break;
            case _MoveSpeed.InAir:
                currentSpeed = _speedInAir;
                break;
            default:

                break;
        }
    }
    private void _Flip()
    {
        _moveDirX = -_moveDirX;
        data.flip_Pivolt.localScale = new Vector3(-data.flip_Pivolt.localScale.x, data.flip_Pivolt.localScale.y, data.flip_Pivolt.localScale.z);
    }
    private bool _HittingGrounded_Raycast() => Physics2D.Raycast(data.flip_Pivolt.position, Vector3.down, _groundRaycastLength, groundMask);
    private bool _HittingGroundedPatrolDireciton_Raycast() => Physics2D.Raycast(data.grounded_Pivolt.position, Vector3.down, _groundPatrolDirecitonRaycastLength, groundMask);
    private bool _HittingInFront_Raycast() => Physics2D.Raycast(data.grounded_Pivolt.position, _FrontDirectiong(), _groundPatrolDirecitonRaycastLength, groundMask);
    private bool _HittingAttackValidate_Raycast() => Physics2D.Raycast(data.grounded_Pivolt.position + 0.01f * Vector3.up, _FrontDirectiong(), _groundPatrolDirecitonRaycastLength, 1 << 8);
    private void _Draw_Raycast_Grounded() => _Draw_Raycast(data.flip_Pivolt.position, Vector3.down, _groundRaycastLength, Color.white);
    private void _Draw_Raycast_GroundedPatrolDireciton() => _Draw_Raycast(data.grounded_Pivolt.position, Vector3.down, _groundPatrolDirecitonRaycastLength, Color.yellow);
    private void _Draw_Raycast_InFront() => _Draw_Raycast(data.grounded_Pivolt.position, _FrontDirectiong(), _groundPatrolDirecitonRaycastLength, Color.green);
    private void _Draw_Raycast_AttackValidate() => _Draw_Raycast(data.grounded_Pivolt.position + 0.01f * Vector3.up, _FrontDirectiong(), hitBoxSize.x, Color.magenta);
    private Vector3 _FrontDirectiong()
    {
        return data.flip_Pivolt.right * Mathf.Clamp(_moveDirX, -1, 1);
    }
    private void _Draw_ChaseHitBox()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireCube(data.flip_Pivolt.position + (data.flip_Pivolt.localScale.x * (chaseHitBox.x / 2) * Vector3.right) + (chaseHitBox.y / 2) * Vector3.up, chaseHitBox);
    }
    private void _Draw_CanChase_Raycast() => _Draw_Raycast(data.flip_Pivolt.position + (chaseHitBox.y / 2) * Vector3.up, _FrontDirectiong(), chaseHitBox.x, Color.red);
    private void _Draw_HitBox()
    {
        Gizmos.color = Color.gray;
        Gizmos.DrawWireCube(data.attack_Pivolt.position + (data.flip_Pivolt.localScale.x * (hitBoxSize.x / 2) * Vector3.right), hitBoxSize);
    }
    private void _Draw_Raycast(Vector3 origin, Vector3 direction, float length, Color color)
    {
        Gizmos.color = color;
        Gizmos.DrawLine(origin, origin + direction * length);
    }
    #endregion

    #region Custom Public Functions
    public void SetForceStopMovement(bool stopMove)
    {
        this._stopMove = stopMove;
        data.rb.velocity = Vector3.zero;
    }
    #endregion
}



#if UNITY_EDITOR

[CustomEditor(typeof(Enm_Behaviour))]
public class Enm_Behaviour_Editor : Editor
{
    public override void OnInspectorGUI()
    {
        _DefaultProperties();
        Enm_Behaviour _Behaviour = (Enm_Behaviour)target;
        switch (_Behaviour.enemyType)
        {
            case Enm_Behaviour.EnemyType.PatrolJust:
                
                break;
            case Enm_Behaviour.EnemyType.PatrolWHitBoxChase:
                CustomEditorAssistance_._DrawProperty(serializedObject, "maxChaseSpeed");
                CustomEditorAssistance_._DrawProperty(serializedObject, "chaseHitBox");
                CustomEditorAssistance_._DrawProperty(serializedObject,"hitBoxSize");
                CustomEditorAssistance_._DrawProperty(serializedObject,"attackSpeed");
                CustomEditorAssistance_._DrawProperty(serializedObject, "groundAndPlayerMask");
                CustomEditorAssistance_._DrawText($"attack every {1f / _Behaviour.attackSpeed} s ", Color.gray);
                break;
            default:
                break;
        }


        //save changes
        serializedObject.ApplyModifiedProperties();
    }
    #region Custom Private Functions
    private void _DefaultProperties()
    {
        CustomEditorAssistance_._DrawProperty(serializedObject,"data");
        CustomEditorAssistance_._DrawProperty(serializedObject,"enemyType");
        CustomEditorAssistance_._DrawProperty(serializedObject,"damagePerHit");
        CustomEditorAssistance_._DrawProperty(serializedObject, "maxGroundSpeed");
        CustomEditorAssistance_._DrawProperty(serializedObject,"inAirSpeedMultiplayer");
        CustomEditorAssistance_._DrawProperty(serializedObject,"groundMask");
    }
    #endregion
}




#endif