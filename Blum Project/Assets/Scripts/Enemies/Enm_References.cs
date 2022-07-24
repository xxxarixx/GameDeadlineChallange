using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enm_References : MonoBehaviour
{
    [Header("Components")]
    public Enm_Behaviour behaviour;
    public Enm_HealthSystem healthSystem;

    public Rigidbody2D rb;
    public CircleCollider2D collision;
    [Header("Pivolts")]
    public Transform flip_Pivolt;
    public Transform grounded_Pivolt;
    public Transform attack_Pivolt;
    public Transform center_Pivolt;
    [Header("Animations")]
    public Animator anim;
    private animations _curPlayingAnimation;
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
        hurt,
        death
    }
    private int _walkAnimationHash { get { return Animator.StringToHash(walkAnimation.name); } set { } }
    [SerializeField] private AnimationClip walkAnimation;
    private int _idleAnimationHash { get { return Animator.StringToHash(idleAnimation.name); } set { } }
    [SerializeField] private AnimationClip idleAnimation;
    private int _attackAnimationHash { get { return Animator.StringToHash(attackAnimation.name); } set { } }
    [SerializeField] private AnimationClip attackAnimation;
    private int _hurtAnimationHash { get { return Animator.StringToHash(hurtAnimation.name); } set { } }
    [SerializeField] private AnimationClip hurtAnimation;
    private int _deathAnimationHash { get { return Animator.StringToHash(deathAnimation.name); } set { } }
    [SerializeField] private AnimationClip deathAnimation;

    public void PlayAnimation(animations _animationEnum, int _priority)
    {
        if (_currentAnimationPriority > _priority) return;
        _currentAnimationPriority = _priority;
        var animationToPlay = _animations_EnumToAnimation(_animationEnum);
        _curPlayingAnimation = _animationEnum;
        anim.Play(animationToPlay);

    }
    public void PlayAnimation(animations _animationEnum, int _priority, out bool canPlayThis)
    {
        canPlayThis = false;
        if (_currentAnimationPriority > _priority) return;
        _currentAnimationPriority = _priority;
        var animationToPlay = _animations_EnumToAnimation(_animationEnum);
        _curPlayingAnimation = _animationEnum;
        canPlayThis = true;
        anim.Play(animationToPlay);

    }
    public void ResetAnimationPriority()
    {
        _currentAnimationPriority = -1;
    }
    public animations GetCurrentPlayingAnimation()
    {
        return _curPlayingAnimation;
    }
    private int _animations_EnumToAnimation(animations _animationEnum)
    {
        switch (_animationEnum)
        {
            case animations.walk:
                return _walkAnimationHash;
            case animations.idle:
                return _idleAnimationHash;
            case animations.attack:
                return _attackAnimationHash;
            case animations.hurt:
                return _hurtAnimationHash;
            case animations.death:
                return _deathAnimationHash;
            default:
                return _idleAnimationHash;
        }
    }
    #endregion
}
