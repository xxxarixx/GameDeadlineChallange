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
    [SerializeField]private Animator anim;
    [SerializeField]private AnimatorOverrideController animations;
    [SerializeField]private float destroyAfterValueSeconds = 0f;
    [Header("FadeOut")]
    [SerializeField] private float fadeOutAnimationAfterValueSeconds = 0f;
    public float fadeOutSpeed = 10f;
    public List<SpriteRenderer> fadeOutSprites = new List<SpriteRenderer>();
    private float fadeOutAnimationAfterValueSecondsProcess;
    public float currentHealth { get; private set; }
    private int hitID;
    void Start()
    {
        currentHealth = maxHealth;
        fadeOutAnimationAfterValueSecondsProcess = fadeOutAnimationAfterValueSeconds;
        if (anim.runtimeAnimatorController != animations) anim.runtimeAnimatorController = animations;
    }
    void Update()
    {
        if (currentHealth > 0) return;
        if (fadeOutAnimationAfterValueSecondsProcess > 0f) fadeOutAnimationAfterValueSecondsProcess -= Time.deltaTime;
        else
        {
            foreach (var item in fadeOutSprites)
            {
                if(item.color.a <= 0f)
                if (item.color.a < 5f) 
                {
                    item.color = new Color(item.color.r,item.color.g,item.color.b,0f);
                    return;
                } 
                item.color = new Color(item.color.r, item.color.g, item.color.b, Mathf.Lerp(item.color.a, 0f, Time.deltaTime * fadeOutSpeed));
            }
        }
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
        anim.Play("Enivro_DestroyAnimatorReference_Destroy");
        Destroy(gameObject, destroyAfterValueSeconds);
    }
}
