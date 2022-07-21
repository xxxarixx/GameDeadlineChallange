using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName ="Player/New Weapon", fileName = "new Weapon")]
public class Player_Weapon : ScriptableObject
{
    public int hitDamage;
    public float attackSpeed;
    public Vector3 hitBoxSize;
    public AnimationClip otherCustomAnimation;
    [HideInInspector] public int customAnimationHash { get { return Animator.StringToHash(otherCustomAnimation.name); } private set { } }

}
