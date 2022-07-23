using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player_Attack : MonoBehaviour
{
    public Player_References refer;
    public Player_Weapon weapon;
    private bool _isAttacking;
    private float _attackTime;
    private void OnDrawGizmos()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireCube(_getHitBoxCenter(), weapon.hitBoxSize);
    }
    private void OnEnable()
    {
        refer.input.OnAttack += Input_OnAttack;
    }
    private void OnDisable()
    {
        refer.input.OnAttack -= Input_OnAttack;
    }
    private List<IDamagableByPlayer> _objectsHitted = new List<IDamagableByPlayer>();
    private void Update()
    {
        if(!_isAttacking && _attackTime > 0f) _attackTime -= Time.deltaTime;
    }
    private void FixedUpdate()
    {
        if (_isAttacking)
        {
            refer.movement.StopPlayerMove(new Vector2(0f,.1f));
        }
    }
    private Vector3 _getHitBoxCenter()
    {
        return refer.attack_Pivolt.position + (refer.flip_Pivolt.localScale.x * (weapon.hitBoxSize.x / 2) * Vector3.right);
    }
    private void Input_OnAttack()
    {
        if (_isAttacking) return;
        if (_attackTime > 0f) return;
        _isAttacking = true;
        if(weapon.otherCustomAnimation != null) 
        {
            refer.PlayAnimation(weapon.customAnimationHash, 1);
        }
        else
        {
            refer.PlayAnimation(Player_References.animations.attack, 1);
        }
        refer.movement.SetMoveState(false);  
    }
    public void AnimationEnded()
    {
        _attackTime = 1f / weapon.attackSpeed;
        _isAttacking = false;
        refer.ResetAnimationPriority();
        refer.movement.SetMoveState(true);
        foreach (var _hitted in _objectsHitted)
        {
            _hitted.ResetHitID();
        }
        _objectsHitted.Clear();
    }
    public void HurtFrame()
    {
        var hits = Physics2D.OverlapBoxAll(_getHitBoxCenter(), weapon.hitBoxSize, 0f);
        foreach (var hit in hits)
        {
            if (hit.TryGetComponent(out IDamagableByPlayer damagable))
            {
                damagable.OnHit(weapon.hitDamage, transform.GetInstanceID(), refer.attack_Pivolt.position, weapon.knockForce);
                _objectsHitted.Add(damagable);
            }
        }
    }
}
