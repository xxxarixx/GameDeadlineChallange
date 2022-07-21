using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player_References : MonoBehaviour
{
    public static Player_References instance { get; private set; }
    public Player_Input input;
    public Player_Movement movement;
    public Player_Attack attack;
    public Rigidbody2D rb;
    public CircleCollider2D collision;
    public Transform flip_Pivolt;
    public Transform attack_Pivolt;
    public Animator anim;
    private int _currentAnimationPriority = -1;
    private void Awake()
    {
        _currentAnimationPriority = -1;
    }
    #region animation handling
    public enum animations
    {
        walk,
        idle,
        jump,
        attack,
        falling
    }
    private int _walkAnimationHash { get { return Animator.StringToHash(walkAnimation.name); } set { } }
    [SerializeField] private AnimationClip walkAnimation;
    private int _idleAnimationHash { get { return Animator.StringToHash(idleAnimation.name); } set { } }
    [SerializeField] private AnimationClip idleAnimation;
    private int _jumpAnimationHash { get { return Animator.StringToHash(jumpAnimation.name); } set { } }
    [SerializeField] private AnimationClip jumpAnimation;
    private int _fallingAnimationHash { get { return Animator.StringToHash(fallingAnimation.name); } set { } }
    [SerializeField] private AnimationClip fallingAnimation;
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
            case animations.jump:
                return _jumpAnimationHash;
            case animations.falling:
                return _fallingAnimationHash;
            case animations.attack:
                return _attackAnimationHash;
            default:
                return _idleAnimationHash;
        }
    }
    #endregion
}
