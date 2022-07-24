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
    private float _fadeOutAnimationAfterValueSecondsProcess;
    public float fadeOutSpeed = 10f;
    public List<SpriteRenderer> fadeOutSprites = new List<SpriteRenderer>();
    public float currentHealth { get; private set; }
    private int _hitID;
    void Start()
    {
        currentHealth = maxHealth;
        _fadeOutAnimationAfterValueSecondsProcess = fadeOutAnimationAfterValueSeconds;
        //if animation controller runtime was not replaced then automaticlly replace it
        if (anim.runtimeAnimatorController != animations) anim.runtimeAnimatorController = animations;
    }
    void Update()
    {
        if (currentHealth > 0) return;
        //fade out animation
        if (_fadeOutAnimationAfterValueSecondsProcess > 0f) _fadeOutAnimationAfterValueSecondsProcess -= Time.deltaTime;
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
        if (this._hitID == _hitID) return;
        this._hitID = _hitID;
        Main_GameManager.instance.SpawnDamagePopup(transform.position, _damage);
        currentHealth -= _damage;
        _DeadChecker();
    }
    public void ResetHitID()
    {
        _hitID = -1;
    }
    private void _DeadChecker()
    {
        if (currentHealth > 0) return;
        Main_GameManager.instance.DropItem(dropItemID, transform.position + .2f * Vector3.up, dropCount, dropEverythingInXseconds);
        //use static destroy animation to play and then it will use overrider to correct it
        anim.Play("Enivro_DestroyAnimatorReference_Destroy");
        Destroy(gameObject, destroyAfterValueSeconds);
    }
}
