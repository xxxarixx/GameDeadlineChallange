using System.Collections;
using System.Collections.Generic;
using UnityEngine;
/// <summary>
/// it is centerlizesion of all player references and animations in one easy accesable place
/// </summary>
public class Player_References : MonoBehaviour
{
    public static Player_References instance { get; private set; }
    [Header("Components")]
    public Player_Input input;
    public Player_Movement movement;
    public Player_Attack attack;
    public Player_HealthSystem healthSystem;
    public Rigidbody2D rb;
    public CircleCollider2D collision;
    public SpriteRenderer mainSprend;
    public Animator anim;
    [Header("Pivolts")]
    public Transform attack_Pivolt;
    public Transform flip_Pivolt;
    public Transform center_Pivolt;
    [Header("Sprites")]
    public Sprite fullHealthSprite;
    public Sprite emptyHealthSprite;
    [Header("Prefabs")]
    public GameObject fullHealthContainerPrefab;
    public GameObject dealDamageEffectPrefab;
    public GameObject jumpEffectPrefab;
    private void Awake()
    {
        instance = this;
        _currentAnimationPriority = -1;
    }
    #region animation handling
    private int _currentAnimationPriority = -1;
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
    public void PlayAnimation(animations _animationEnum, int _priority)
    {
        if (_currentAnimationPriority > _priority) return;
        _currentAnimationPriority = _priority;
        var animationToPlay = _animations_EnumToAnimation(_animationEnum);
        anim.Play(animationToPlay);

    }
    public void PlayAnimation(animations _animationEnum, int _priority, out bool _canPlayAnimation)
    {
        _canPlayAnimation = false;
        if (_currentAnimationPriority > _priority) return;
        _currentAnimationPriority = _priority;
        var animationToPlay = _animations_EnumToAnimation(_animationEnum);
        _canPlayAnimation = true;
        anim.Play(animationToPlay);
        
    }
    public void PlayAnimation(int _animationHash, int _priority, out bool _canPlayAnimation)
    {
        _canPlayAnimation = false;
        if (_currentAnimationPriority > _priority) return;
        _currentAnimationPriority = _priority;
        _canPlayAnimation = true;
        anim.Play(_animationHash);

    }
    public void PlayAnimation(int _animationHash, int _priority)
    {
        if (_currentAnimationPriority > _priority) return;
        _currentAnimationPriority = _priority;
        anim.Play(_animationHash);

    }
    public void ResetAnimationPriority()
    {
        _currentAnimationPriority = -1;
    }
    private int _animations_EnumToAnimation(animations _animationEnum)
    {
        switch (_animationEnum)
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
