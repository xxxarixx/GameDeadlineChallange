using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IDamagableByPlayer
{
    /// <summary>
    /// 
    /// </summary>
    /// <param name="_damage"> damage </param>
    /// <param name="_hitID"> to detect deal damage per swing (only one per enemy) this can be any ID</param>
    public void OnHit(float _damage, int _hitID, Vector3 _hitInvokerPosition, float _weaponKnockForce);
    /// <summary>
    /// reset swing
    /// </summary>
    public void ResetHitID();
}

public interface IDamagableByEnemy
{
    public void OnHit(int _damage, Vector3 _invokerPosition, float _knockBackMultiplayer = 1f);
}
