using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player_Attack : MonoBehaviour
{
    public Player_References data;
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
        data.input.OnAttack += Input_OnAttack;
    }
    private void OnDisable()
    {
        data.input.OnAttack -= Input_OnAttack;
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
            var hits = Physics2D.OverlapBoxAll(_getHitBoxCenter(), weapon.hitBoxSize, 0f);
            foreach (var hit in hits)
            {
                if(hit.TryGetComponent(out IDamagableByPlayer damagable))
                {
                    damagable.OnHit(weapon.hitDamage, transform.GetInstanceID());
                    _objectsHitted.Add(damagable);
                }
            }
            data.movement.StopPlayerMove(new Vector2(0f,.1f));
        }
    }
    private Vector3 _getHitBoxCenter()
    {
        return data.attack_Pivolt.position + (data.flip_Pivolt.localScale.x * (weapon.hitBoxSize.x / 2) * Vector3.right);
    }
    private void Input_OnAttack()
    {
        if (_isAttacking) return;
        if (_attackTime > 0f) return;
        _isAttacking = true;
        if(weapon.otherCustomAnimation != null) 
        {
            data.PlayAnimation(weapon.customAnimationHash, 1);
        }
        else
        {
            data.PlayAnimation(Player_References.animations.attack, 1);
        }
        data.movement.SetMoveState(false);  
    }
    public void AnimationEnded()
    {
        _attackTime = weapon.attackSpeed;
        _isAttacking = false;
        data.ResetAnimationPriority();
        data.movement.SetMoveState(true);
        foreach (var _hitted in _objectsHitted)
        {
            _hitted.ResetHitID();
        }
        _objectsHitted.Clear();
    }
}
