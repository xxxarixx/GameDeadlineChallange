using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enm_OnCollisionDealDamage : MonoBehaviour
{
    private Enm_Behaviour _data;
    [SerializeField]private int damageToDeal = 1;
    private void Start()
    {
        _data = GetComponent<Enm_Behaviour>();
    }
    private void OnCollisionStay2D(Collision2D collision)
    {
        if (_data == null) return;
        if(collision.gameObject.TryGetComponent(out IDamagableByEnemy damagableByEnemy))
        {
            damagableByEnemy.OnHit(damageToDeal, _data.refer.flip_Pivolt.position);
        }
    }
}
