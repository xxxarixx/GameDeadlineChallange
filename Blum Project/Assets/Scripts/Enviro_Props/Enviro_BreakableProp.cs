using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enviro_BreakableProp : MonoBehaviour,IDamagableByPlayer
{
    public int MaxHealth;
    private int _currentHealth;
    private int hitID;
    void Start()
    {
        _currentHealth = MaxHealth;
    }
    public void OnHit(int _damage, int _hitID)
    {
        //detection one damage per swing
        if (this.hitID == _hitID) return;
        this.hitID = _hitID;
        Main_GameManager.instance.SpawnDamagePopup(transform.position, _damage);
        _currentHealth -= _damage;
        _DeadChecker();
    }
    public void ResetHitID()
    {
        hitID = -1;
    }
    private void _DeadChecker()
    {
        if (_currentHealth > 0) return;
        Destroy(gameObject);
    }
}
