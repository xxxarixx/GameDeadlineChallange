using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IDamagableByPlayer
{
    /// <summary>
    /// 
    /// </summary>
    /// <param name="Damage"> damage </param>
    /// <param name="HitID"> to detect deal damage per swing (only one per enemy) this can be any ID</param>
    public void OnHit(int Damage, int HitID);
    /// <summary>
    /// reset swing
    /// </summary>
    public void ResetHitID();
}

public interface IDamagableByEnemy
{
    public void OnHit(int Damage);
}
