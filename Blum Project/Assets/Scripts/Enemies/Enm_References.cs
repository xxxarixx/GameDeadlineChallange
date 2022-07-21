using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enm_References : MonoBehaviour
{
    public Enm_Behaviour behaviour;
    public Enm_HealthSystem healthSystem;

    public Rigidbody2D rb;
    public CircleCollider2D collision;
    public Transform flip_Pivolt;
    public Transform grounded_Pivolt;
    public Animator anim;
    private void Awake()
    {
        _currentAnimationPriority = -1;
    }
    #region animation handling
    private int _currentAnimationPriority = -1;
    public enum animations
    {
        walk,
        idle,
        attack,
    }
    private int _walkAnimationHash { get { return Animator.StringToHash(walkAnimation.name); } set { } }
    [SerializeField] private AnimationClip walkAnimation;
    private int _idleAnimationHash { get { return Animator.StringToHash(idleAnimation.name); } set { } }
    [SerializeField] private AnimationClip idleAnimation;
    private int _attackAnimationHash { get { return Animator.StringToHash(attackAnimation.name); } set { } }
    [SerializeField] private AnimationClip attackAnimation;

    public void PlayAnimation(animations animationEnum, int Priority)
    {
        if (_currentAnimationPriority > Priority) return;
        _currentAnimationPriority = Priority;
        var animationToPlay = _animations_EnumToAnimation(animationEnum);
        anim.Play(animationToPlay);

    }
    public void PlayAnimation(int animationHash, int Priority)
    {
        if (_currentAnimationPriority > Priority) return;
        _currentAnimationPriority = Priority;
        anim.Play(animationHash);

    }
    public void ResetAnimationPriority()
    {
        _currentAnimationPriority = -1;
    }
    private int _animations_EnumToAnimation(animations animationEnum)
    {
        switch (animationEnum)
        {
            case animations.walk:
                return _walkAnimationHash;
            case animations.idle:
                return _idleAnimationHash;
            case animations.attack:
                return _attackAnimationHash;
            default:
                return _idleAnimationHash;
        }
    }
    #endregion
}
