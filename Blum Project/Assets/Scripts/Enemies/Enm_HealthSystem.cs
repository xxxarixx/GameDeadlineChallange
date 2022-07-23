using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enm_HealthSystem : MonoBehaviour, IDamagableByPlayer
{
    public Enm_References refer;
    [SerializeField]private int maxHealth;
    public float currentHealth { get; private set; }
    public float knockMultiplayer = 1f;
    private int _hitID = -1;
    [Header("DropItems:")]
    public int dropItemID;
    public int dropCount = 1;
    [Range(0f, 1f)] public float dropEverythingInXseconds;
    public delegate void DelHitted(Vector3 _hitInvokerPosition);
    public event DelHitted OnGetHitted;
    void Start()
    {
        currentHealth = maxHealth;
    }
    void OnDrawGizmosSelected()
    {
        if(refer == null)
        {
            if(TryGetComponent(out Enm_References enm_References))
            {
                refer = enm_References;
            }
        }
    }
    public void OnHit(float _damage, int _hitID, Vector3 _hitInvokerPosition, float _weaponKnockForce)
    {
        if (this._hitID == _hitID) return;
        this._hitID = _hitID;
        currentHealth -= _damage;
        Main_GameManager.instance.SpawnDamagePopup(refer.flip_Pivolt.position, _damage);
        StartCoroutine(KnockFromDirection(_CalculateKnockDirection(refer.flip_Pivolt.position, _hitInvokerPosition), _weaponKnockForce));
        OnGetHitted?.Invoke(_hitInvokerPosition);
        _DeadChecker();
    }
    private IEnumerator KnockFromDirection(Vector3 _knockDirection, float _weaponKnockForce)
    {
        refer.behaviour.SetForceStopMovement(true);
        refer.rb.velocity = new Vector2(_knockDirection.x,refer.rb.velocity.y) * (knockMultiplayer * _weaponKnockForce);
        yield return new WaitForSeconds(.125f);
        refer.behaviour.SetForceStopMovement(false);
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
        Main_GameManager.instance.DropItem(dropItemID, refer.flip_Pivolt.position + .2f * Vector3.up, dropCount, dropEverythingInXseconds);
        Destroy(gameObject);
    }
}
