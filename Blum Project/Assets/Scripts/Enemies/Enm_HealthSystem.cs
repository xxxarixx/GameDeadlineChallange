using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enm_HealthSystem : MonoBehaviour, IDamagableByPlayer
{
    public Enm_References data;
    [SerializeField]private int maxHealth;
    public int currentHealth { get; private set; }
    private int _hitID = -1;
    void Start()
    {
        currentHealth = maxHealth;
    }
    public void OnHit(int _damage, int _hitID)
    {
        if (this._hitID == _hitID) return;
        this._hitID = _hitID;
        currentHealth -= _damage;
        Main_GameManager.instance.SpawnDamagePopup(data.flip_Pivolt.position, _damage);
        _DeadChecker();
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
