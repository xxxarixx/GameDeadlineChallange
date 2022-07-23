using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enviro_BreakableProp : MonoBehaviour,IDamagableByPlayer
{
    [SerializeField]private float maxHealth;
    [Header("DropItems:")]
    public int dropItemID;
    public int dropCount = 1;
    [Range(0f,1f)]public float dropEverythingInXseconds;
    public float currentHealth { get; private set; }
    private int hitID;
    void Start()
    {
        currentHealth = maxHealth;
    }
    public void OnHit(float _damage, int _hitID, Vector3 _hitInvokerPosition, float _weaponKnockForce)
    {
        //detection one damage per swing
        if (this.hitID == _hitID) return;
        this.hitID = _hitID;
        Main_GameManager.instance.SpawnDamagePopup(transform.position, _damage);
        currentHealth -= _damage;
        _DeadChecker();
    }
    public void ResetHitID()
    {
        hitID = -1;
    }
    private void _DeadChecker()
    {
        if (currentHealth > 0) return;
        Main_GameManager.instance.DropItem(dropItemID, transform.position + .2f * Vector3.up, dropCount, dropEverythingInXseconds);
        Destroy(gameObject);
    }
}
