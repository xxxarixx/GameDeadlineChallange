using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
public class Enm_Behaviour : MonoBehaviour
{
    #region enums
    public enum EnemyType
    {
        PatrolJust,
        PatrolWRadialChase
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
    [SerializeField] private float maxSpeed;
    [SerializeField] private LayerMask groundMask;


    [Header("patrolWRadialChase")]
    [SerializeField]private float chaseRadius;
    [SerializeField]private Vector3 hitBoxSize;

    [Header("Debug")]
    //speed with modifires
    private float speedInAir;
    private float speedGrounded;
    //Raycasts length
    private float groundRaycastLength = 0.3f;
    private float groundPatrolDirecitonRaycastLength = 0f;
    //move rules
    private float _moveDirX = 1f;
    private bool _stopMove;
    private bool _grounded;
    private bool _groundedPatrolDireciton;

    #region Unity functions
    void Start()
    {
        _SpeedSetup();
        _MoveSetup();
        _RaycastsLengthSetup();
    }
    private void FixedUpdate()
    {
        if(_stopMove) return;
        _groundedPatrolDireciton = _HittingGroundedPatrolDireciton_Raycast();
        _grounded = _HittingGrounded_Raycast();
        if (!_groundedPatrolDireciton && _grounded || _HittingInFront_Raycast() && _grounded)
        {
            _Flip();
        }
        _Move(currentSpeed, _MoveAxis.Horizontal);
        if (_grounded)
        {
            data.PlayAnimation(Enm_References.animations.walk, 0);
            _SetCurrentMoveSpeed(_MoveSpeed.OnGround);
        }
        else
        {
            _SetCurrentMoveSpeed(_MoveSpeed.InAir);
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
            case EnemyType.PatrolWRadialChase:
                _Draw_chaseRadius();
                break;
            default:
                break;
        }
    }
    #endregion

    #region Custom Private Functions
    private void _SpeedSetup()
    {
        speedGrounded = maxSpeed + Random.Range(-currentSpeed / 10, currentSpeed / 10);
        currentSpeed = speedGrounded;
        speedInAir = currentSpeed * inAirSpeedMultiplayer;
    }
    private void _MoveSetup()
    {
        _moveDirX = 1f;
    }
    private void _RaycastsLengthSetup()
    {
        groundPatrolDirecitonRaycastLength = data.grounded_Pivolt.localPosition.y + groundRaycastLength + .1f;
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
                currentSpeed = speedGrounded;
                break;
            case _MoveSpeed.InAir:
                currentSpeed = speedInAir;
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
    private bool _HittingGrounded_Raycast() => Physics2D.Raycast(data.flip_Pivolt.position, Vector3.down, groundRaycastLength, groundMask);
    private bool _HittingGroundedPatrolDireciton_Raycast() => Physics2D.Raycast(data.grounded_Pivolt.position, Vector3.down, groundPatrolDirecitonRaycastLength, groundMask);
    private bool _HittingInFront_Raycast() => Physics2D.Raycast(data.grounded_Pivolt.position, data.flip_Pivolt.right * Mathf.Clamp(_moveDirX, -1, 1), groundPatrolDirecitonRaycastLength, groundMask);
    private void _Draw_Raycast_Grounded() => _Draw_Raycast(data.flip_Pivolt.position, Vector3.down, groundRaycastLength, Color.white);
    private void _Draw_Raycast_GroundedPatrolDireciton() => _Draw_Raycast(data.grounded_Pivolt.position, Vector3.down, groundPatrolDirecitonRaycastLength, Color.yellow);
    private void _Draw_Raycast_InFront() => _Draw_Raycast(data.grounded_Pivolt.position, data.flip_Pivolt.right * Mathf.Clamp(_moveDirX, -1, 1), groundPatrolDirecitonRaycastLength, Color.green);
    private void _Draw_chaseRadius()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(data.flip_Pivolt.position, chaseRadius);
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
            case Enm_Behaviour.EnemyType.PatrolWRadialChase:
                _DrawProperty("chaseRadius");
                _DrawProperty("hitBoxSize");
                break;
            default:
                break;
        }


        //save changes
        serializedObject.ApplyModifiedProperties();
    }
    #region Custom Private Functions
    private void _DrawProperty(string _propertyName)
    {
        var property = serializedObject.FindProperty(_propertyName);
        EditorGUILayout.PropertyField(property);
    }
    private void _DefaultProperties()
    {
        _DrawProperty("data");
        _DrawProperty("enemyType");
        _DrawProperty("damagePerHit");
        _DrawProperty("maxSpeed");
        _DrawProperty("inAirSpeedMultiplayer");
        _DrawProperty("groundMask");
    }
    #endregion
}




#endif