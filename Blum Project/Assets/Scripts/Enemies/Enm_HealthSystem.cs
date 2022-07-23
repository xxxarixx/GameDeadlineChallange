using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enm_HealthSystem : MonoBehaviour, IDamagableByPlayer
{
    public Enm_References data;
    [SerializeField]private int maxHealth;
    public float currentHealth { get; private set; }
    public float knockMultiplayer = 1f;
    private int _hitID = -1;
    public delegate void DelHitted(Vector3 _hitInvokerPosition);
    public event DelHitted OnGetHitted;
    void Start()
    {
        currentHealth = maxHealth;
    }
    public void OnHit(float _damage, int _hitID, Vector3 _hitInvokerPosition, float _weaponKnockForce)
    {
        if (this._hitID == _hitID) return;
        this._hitID = _hitID;
        currentHealth -= _damage;
        Main_GameManager.instance.SpawnDamagePopup(data.flip_Pivolt.position, _damage);
        StartCoroutine(KnockFromDirection(_CalculateKnockDirection(data.flip_Pivolt.position, _hitInvokerPosition), _weaponKnockForce));
        OnGetHitted?.Invoke(_hitInvokerPosition);
        _DeadChecker();
    }
    private IEnumerator KnockFromDirection(Vector3 _knockDirection, float _weaponKnockForce)
    {
        data.behaviour.SetForceStopMovement(true);
        data.rb.velocity = new Vector2(_knockDirection.x,data.rb.velocity.y) * (knockMultiplayer * _weaponKnockForce);
        yield return new WaitForSeconds(.125f);
        data.behaviour.SetForceStopMovement(false);
    }
    private Vector3 _CalculateKnockDirection(Vector3 _myPosition, Vector3 _hitInvokerPosition) 
    { 
        return (_myPosition - _hitInvokerPosition).normalized;
    }
    public void ResetHitID()
    {
        _hitID = -1;
    }
    private void _DeadChecker()
    {
        if (currentHealth > 0) return;
        Destroy(gameObject);
    }
}
