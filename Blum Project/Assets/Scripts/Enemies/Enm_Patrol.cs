using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using CustomEditorAssistance;
[RequireComponent(typeof(Enm_Behaviour))]
public class Enm_Patrol : MonoBehaviour
{
    #region enums
    public enum EnemyType
    {
        Patrol_Edge_Edge,
        Patrol_Edge_Edge_WHitBoxChase,
        Patrol_DecleratedPoints
    }
    public EnemyType enemyType;
    #endregion
    private Enm_Behaviour _data;


    [Header("patrol Declerated points")]
    public List<Transform> dec_patrolPoints = new List<Transform>();

    [Header("patrol Edge_Edge WHitBoxChase")]
    public float eehit_attackSpeed;
    public Vector3 eehit_chaseHitBox;
    public Vector3 eehit_hitBoxSize;
    public LayerMask eehit_groundAndPlayerMask;
    public float eehit_chaseSpeedMultiplayer = 2f;

    private float _eehit__speedChase;
    private float _eehit_attackSpeedCooldown;
    private RaycastHit2D _eehit_shouldPerformAttack;
    private bool _eehit_attackAnimationEnded;

    [Header("patrol Edge_Edge")]
    private float _ee_groundRaycastLength = 0.3f;
    private float _ee_groundPatrolDirecitonRaycastLength = 0f;
    private bool _ee_grounded;
    private bool _ee_groundedPatrolDireciton;
    
    

    #region Unity functions
    private void Awake()
    {
        _data = GetComponent<Enm_Behaviour>();
    }
    private void OnValidate()
    {
        eehit_attackSpeed = Mathf.Clamp(eehit_attackSpeed, 0f, float.MaxValue);
    }
    private void Start()
    {
        _RaycastsLengthSetup();
        _SpeedSetup();
        _data.refer.healthSystem.OnGetHitted += _HealthSystem_OnGetHitted;
    }
    private void FixedUpdate()
    {
        switch (enemyType)
        {
            case EnemyType.Patrol_Edge_Edge:
                _PatrolMovement();
                break;
            case EnemyType.Patrol_Edge_Edge_WHitBoxChase:
                _AttackValidate();
                _ChaseMovement(out bool isChasing);
                if(!isChasing)_PatrolMovement();
                break;
            case EnemyType.Patrol_DecleratedPoints:
                _PatrolMovementWDeclaratedPoints();
                break;
            default:
                break;
        }

    }
    private void OnDrawGizmos()
    {
        if(_data == null) _data = GetComponent<Enm_Behaviour>();
        void PatrolLeftRightFunctions()
        {
            _RaycastsLengthSetup();
            _Draw_Raycast_Grounded();
            _Draw_Raycast_GroundedPatrolDireciton();
            _Draw_Raycast_InFront();
        }
        switch (enemyType)
        {
            case EnemyType.Patrol_Edge_Edge:
                PatrolLeftRightFunctions();
                break;
            case EnemyType.Patrol_Edge_Edge_WHitBoxChase:
                PatrolLeftRightFunctions();
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

    private void _HealthSystem_OnGetHitted(Vector3 _hitInvokerPosition)
    {
        //hitted from behind
        var dir = (_hitInvokerPosition - transform.position).normalized;
        var frontDir = _data._FrontDirectiong();
        if (Vector2.Dot(frontDir,dir) < 0)
        {
            _data._Flip();
            switch (enemyType)
            {
                case EnemyType.Patrol_Edge_Edge:
                    break;
                case EnemyType.Patrol_Edge_Edge_WHitBoxChase:
                    _eehit_attackSpeedCooldown = (1f / eehit_attackSpeed) / 4;
                    break;
                case EnemyType.Patrol_DecleratedPoints:
                    break;
                default:
                    break;
            }
        } 
    }

    #region Patrol Edge_Edge
    private void _PatrolMovement()
    {
        if (_data._stopMove) return;
        _ee_groundedPatrolDireciton = _HittingGroundedPatrolDireciton_Raycast();
        _ee_grounded = _HittingGrounded_Raycast();
        if (!_ee_groundedPatrolDireciton && _ee_grounded || _HittingInFront_Raycast() && _ee_grounded)
        {
            _data._Flip();
        }
        _data.Move(_data.currentSpeed, Enm_Behaviour._MoveAxis.Horizontal);
        if (_ee_grounded)
        {
            _data.refer.PlayAnimation(Enm_References.animations.walk, 1);
            _data._SetCurrentMoveSpeed(Enm_Behaviour.SpeedType.Ground);
        }
        else
        {
            _data._SetCurrentMoveSpeed(Enm_Behaviour.SpeedType.InAir);
        }
    }
    private void _RaycastsLengthSetup()
    {
        _ee_groundPatrolDirecitonRaycastLength = _data.refer.grounded_Pivolt.localPosition.y + _ee_groundRaycastLength + .1f;
    }
    private bool _HittingGrounded_Raycast() => Physics2D.Raycast(_data.refer.flip_Pivolt.position, Vector3.down, _ee_groundRaycastLength, _data.groundMask);
    private bool _HittingGroundedPatrolDireciton_Raycast() => Physics2D.Raycast(_data.refer.grounded_Pivolt.position, Vector3.down, _ee_groundPatrolDirecitonRaycastLength, _data.groundMask);
    private bool _HittingInFront_Raycast() => Physics2D.Raycast(_data.refer.grounded_Pivolt.position, _data._FrontDirectiong(), _ee_groundPatrolDirecitonRaycastLength, _data.groundMask);
    private void _Draw_Raycast_Grounded() => _data._Draw_Raycast(_data.refer.flip_Pivolt.position, Vector3.down, _ee_groundRaycastLength, Color.white);
    private void _Draw_Raycast_GroundedPatrolDireciton() => _data._Draw_Raycast(_data.refer.grounded_Pivolt.position, Vector3.down, _ee_groundPatrolDirecitonRaycastLength, Color.yellow);
    private void _Draw_Raycast_InFront() => _data._Draw_Raycast(_data.refer.grounded_Pivolt.position, _data._FrontDirectiong(), _ee_groundPatrolDirecitonRaycastLength, Color.green);
    #endregion

    #region Patrol Edge_Edge W HitBox Chase
    private void _SpeedSetup()
    {
        _eehit__speedChase = _data._GetSpeedEnumValue(Enm_Behaviour.SpeedType.Ground) * eehit_chaseSpeedMultiplayer;
    }
    private void _AttackValidate()
    {
        var attackHitBox = Physics2D.OverlapBox(_data.refer.attack_Pivolt.position + (_data.refer.flip_Pivolt.localScale.x * (eehit_hitBoxSize.x / 2) * Vector3.right), eehit_hitBoxSize, 0f, 1 << 8);
        _eehit_shouldPerformAttack = _HittingAttackValidate_Raycast();
        if (attackHitBox != null)
        {
            if (_eehit_shouldPerformAttack.collider == null || _eehit_shouldPerformAttack.collider != null && _eehit_shouldPerformAttack.collider.gameObject.CompareTag("Player"))
            {
                _data.SetForceStopMovement(true);
                if (_eehit_attackSpeedCooldown <= 0f)
                {
                    // stop and can attack
                    _eehit_attackAnimationEnded = false;
                    _data.refer.PlayAnimation(Enm_References.animations.attack, 2);
                    _eehit_attackSpeedCooldown = 1f / eehit_attackSpeed;
                }
            }
        }
        else
        {

            //if in previous frame attack should be performed attack and was applied force stop movement and in next frame player went from attack validate raycast then unlock movement
            if (_data._stopMove && !_eehit_shouldPerformAttack && _eehit_attackAnimationEnded) _data.SetForceStopMovement(false);
        }
        if (_eehit_attackSpeedCooldown > 0f)
        {
            _eehit_attackSpeedCooldown -= Time.deltaTime;
        }

        //performing attack cooldown
    }
    private void _ChaseMovement(out bool _isChasing)
    {
        _isChasing = false;
        if (_data._stopMove) return;
        var chaseHitBoxHit = Physics2D.OverlapBox(_data.refer.flip_Pivolt.position + (_data.refer.flip_Pivolt.localScale.x * (eehit_chaseHitBox.x / 2) * Vector3.right) + (eehit_chaseHitBox.y / 2) * Vector3.up, eehit_chaseHitBox, 0f, 1 << 8);
        if (chaseHitBoxHit != null)
        {
            //detect ground and player
            var canChaseRaycastHit = Physics2D.Raycast(_data.refer.flip_Pivolt.position + (eehit_chaseHitBox.y / 2) * Vector3.up, _data._FrontDirectiong(), eehit_chaseHitBox.x, eehit_groundAndPlayerMask);
            if (canChaseRaycastHit.collider == null || canChaseRaycastHit.collider != null && canChaseRaycastHit.collider.gameObject.CompareTag("Player")) //if player hit first or nothing was hitted then chase player 
            {
                _data._SetCurrentMoveSpeed(_eehit__speedChase);
                _data.refer.PlayAnimation(Enm_References.animations.walk, 1);
                _data.Move(_data.currentSpeed, Enm_Behaviour._MoveAxis.Horizontal);
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
        if(!_isChasing) _data._SetCurrentMoveSpeed(Enm_Behaviour.SpeedType.Ground);
    }
    public void AttackAnimationEnded()
    {
        _data.refer.ResetAnimationPriority();
        _data.SetForceStopMovement(false);
        _eehit_attackAnimationEnded = true;
    }
    private void _Draw_ChaseHitBox()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireCube(_data.refer.flip_Pivolt.position + (_data.refer.flip_Pivolt.localScale.x * (eehit_chaseHitBox.x / 2) * Vector3.right) + (eehit_chaseHitBox.y / 2) * Vector3.up, eehit_chaseHitBox);
    }
    private void _Draw_CanChase_Raycast() => _data._Draw_Raycast(_data.refer.flip_Pivolt.position + (eehit_chaseHitBox.y / 2) * Vector3.up, _data._FrontDirectiong(), eehit_chaseHitBox.x, Color.red);
    private RaycastHit2D _HittingAttackValidate_Raycast() => Physics2D.Raycast(_data.refer.attack_Pivolt.position, _data._FrontDirectiong(), eehit_hitBoxSize.x, eehit_groundAndPlayerMask);
    private void _Draw_HitBox()
    {
        Gizmos.color = Color.gray;
        Gizmos.DrawWireCube(_data.refer.attack_Pivolt.position + (_data.refer.flip_Pivolt.localScale.x * (eehit_hitBoxSize.x / 2) * Vector3.right), eehit_hitBoxSize);
    }
    private void _Draw_Raycast_AttackValidate() => _data._Draw_Raycast(_data.refer.attack_Pivolt.position, _data._FrontDirectiong(), eehit_hitBoxSize.x, Color.gray);
    #endregion

    #region Patrol W Declarated Points
    private void _PatrolMovementWDeclaratedPoints()
    {
        if (dec_patrolPoints.Count == 0) _data.refer.PlayAnimation(Enm_References.animations.idle,2);
    }

    #endregion
    
    
    

}



#if UNITY_EDITOR

[CustomEditor(typeof(Enm_Patrol))]
public class Enm_Patrol_Editor : Editor
{
    public override void OnInspectorGUI()
    {
        Enm_Patrol _Patrol = (Enm_Patrol)target;
        _DefaultProperties(_Patrol);
        switch (_Patrol.enemyType)
        {
            case Enm_Patrol.EnemyType.Patrol_Edge_Edge:
                
                break;
            case Enm_Patrol.EnemyType.Patrol_Edge_Edge_WHitBoxChase:
                CustomEditorAssistance_._DrawProperty(serializedObject, nameof(_Patrol.eehit_chaseSpeedMultiplayer));
                CustomEditorAssistance_._DrawProperty(serializedObject, nameof(_Patrol.eehit_chaseHitBox));
                CustomEditorAssistance_._DrawProperty(serializedObject, nameof(_Patrol.eehit_hitBoxSize));
                CustomEditorAssistance_._DrawProperty(serializedObject, nameof(_Patrol.eehit_attackSpeed));
                CustomEditorAssistance_._DrawProperty(serializedObject, nameof(_Patrol.eehit_groundAndPlayerMask));
                CustomEditorAssistance_._DrawText($"attack every {1f / _Patrol.eehit_attackSpeed} s ", Color.gray);
                break;
            case Enm_Patrol.EnemyType.Patrol_DecleratedPoints:
                CustomEditorAssistance_._DrawProperty(serializedObject, nameof(_Patrol.dec_patrolPoints));
                break;
            default:
                break;
        }


        //save changes
        serializedObject.ApplyModifiedProperties();
    }
    #region Custom Private Functions
    private void _DefaultProperties(Enm_Patrol _Patrol)
    {
        CustomEditorAssistance_._DrawProperty(serializedObject, nameof(_Patrol.enemyType));
    }
    #endregion
}




#endif