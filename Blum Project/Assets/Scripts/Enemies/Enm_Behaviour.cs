using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enm_Behaviour : MonoBehaviour
{
    public Enm_References refer;
    [Header("allType")]
    [SerializeField] private int damagePerHit;
    public float currentSpeed { get; private set; }
    public float inAirSpeedMultiplayer = .7f;
    [SerializeField] private float maxGroundSpeed = 100f;

    public LayerMask groundMask;
    private float _moveDirX = 1f;
    //speed with modifires
    private float _speedInAir;
    private float _speedGrounded;
    [HideInInspector]public bool _stopMove;
    private void Awake()
    {
        _SpeedSetup();
        _MoveSetup();
    }
    private void FixedUpdate()
    {
        if (_stopMove) refer.PlayAnimation(Enm_References.animations.idle, 0);
    }
    public enum SpeedType
    {
        Ground,
        InAir
    }
    public enum _MoveAxis
    {
        Horizontal,
        Vertical
    }
    private void _MoveSetup()
    {
        _moveDirX = 1f;
    }
    public void Move(float _speed, _MoveAxis _axis)
    {
        if (_stopMove) return;
        //axis that player should move
        Vector2 axis = (_axis == _MoveAxis.Horizontal) ? new Vector2(1, 0) : new Vector2(0, 1);
        //invert axis to get velocity that shouldnt be changed by this movement
        Vector2 invertedAxis = (_axis == _MoveAxis.Horizontal) ? new Vector2(0, 1) : new Vector2(1, 0);
        refer.rb.velocity = axis * new Vector2(_moveDirX, 1f) * _speed * Time.fixedDeltaTime + invertedAxis * refer.rb.velocity;
    }
    private void _SpeedSetup()
    {
        _speedGrounded = maxGroundSpeed + Random.Range(-currentSpeed / 10, currentSpeed / 10);
        currentSpeed = _speedGrounded;
        _speedInAir = currentSpeed * inAirSpeedMultiplayer;
    }
    public void SetForceStopMovement(bool stopMove)
    {
        this._stopMove = stopMove;
        refer.rb.velocity = Vector3.zero;
    }
    public float _GetSpeedEnumValue(SpeedType moveSpeedType)
    {
        switch (moveSpeedType)
        {
            case SpeedType.Ground:
                return _speedGrounded;
                break;
            case SpeedType.InAir:
                return _speedInAir;
                break;
            default:
                return _speedGrounded;
                break;
        }
    }
    public void _SetCurrentMoveSpeed(SpeedType moveSpeedType)
    {
        switch (moveSpeedType)
        {
            case SpeedType.Ground:
                currentSpeed = _speedGrounded;
                break;
            case SpeedType.InAir:
                currentSpeed = _speedInAir;
                break;
            default:
                break;
        }
        
    }
    public void _SetCurrentMoveSpeed(float moveSpeedValue)
    {
        currentSpeed = moveSpeedValue;
    }
    public void _Flip()
    {
        _moveDirX = -_moveDirX;
        refer.flip_Pivolt.localScale = new Vector3(-refer.flip_Pivolt.localScale.x, refer.flip_Pivolt.localScale.y, refer.flip_Pivolt.localScale.z);
    }
    public Vector3 _FrontDirectiong()
    {
        return refer.flip_Pivolt.right * Mathf.Clamp(_moveDirX, -1, 1);
    }
    public void _Draw_Raycast(Vector3 origin, Vector3 direction, float length, Color color)
    {
        Gizmos.color = color;
        Gizmos.DrawLine(origin, origin + direction * length);
    }
}
