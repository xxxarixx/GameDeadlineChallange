using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using CustomEditorAssistance;
[CreateAssetMenu(menuName ="Player/New Weapon", fileName = "new Weapon")]
public class Player_Weapon : ScriptableObject
{
    public int hitDamage;
    [Tooltip("attack cooldown = (1f / attacks speed)")]public float attackSpeed;
    public float knockForce;
    public Vector3 hitBoxSize;
    public AnimationClip otherCustomAnimation;
    [HideInInspector] public int customAnimationHash { get { return Animator.StringToHash(otherCustomAnimation.name); } private set { } }

}

#if UNITY_EDITOR
[CustomEditor(typeof(Player_Weapon))]
public class Player_Weapon_CustomEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();
        Player_Weapon _weapon = (Player_Weapon)target;
        CustomEditorAssistance_._DrawText($"attack every {1f / _weapon.attackSpeed} s ", Color.gray);
        CustomEditorAssistance_._DrawText($"DPS: {_weapon.hitDamage * _weapon.attackSpeed}", Color.red, TextAnchor.MiddleCenter, 25);
        serializedObject.ApplyModifiedProperties();
    }
    
    
}
#endif
