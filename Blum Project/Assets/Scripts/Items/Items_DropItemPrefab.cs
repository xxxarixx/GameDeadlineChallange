using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Items_DropItemPrefab : MonoBehaviour
{
    public SpriteRenderer sprend;
    public Animator animator;
    public int itemID;
    private Rigidbody2D rb;
    [Tooltip("it will change position Y based on this curve value and time")]public AnimationCurve jumpAnimation;
    public Vector2 randomXForce_FromTo;
    private float animationProgress = 0f;
    float xForce = 0f;
    public void SetAnimation(Main_GameManager.Item item)
    {
        if (item.animation == null) return;
        //create new animator overrider
        AnimatorOverrideController overrideController = new AnimatorOverrideController(animator.runtimeAnimatorController);
        //get sprite animation current playing
        var animationNameToChange = animator.GetCurrentAnimatorClipInfo(0)[0].clip.name;
        animator.runtimeAnimatorController = overrideController;
        //change sprite animation to new one
        overrideController[animationNameToChange] = item.animation;
    }
    private void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        xForce = Random.Range(randomXForce_FromTo.x, randomXForce_FromTo.y);
    }
    private void Update()
    {
        if (animationProgress > jumpAnimation.keys[jumpAnimation.length - 1].time)
        {
            //animation ended
        }
        else
        {
            //animation jump processing
            animationProgress += Time.deltaTime;
            rb.velocity = new Vector2(xForce, jumpAnimation.Evaluate(animationProgress));
        }
    }
}
