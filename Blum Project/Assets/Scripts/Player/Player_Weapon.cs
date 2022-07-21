using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
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
        _DrawText($"DPS: {_weapon.hitDamage * _weapon.attackSpeed}", Color.red, TextAnchor.MiddleCenter, 25);
        serializedObject.ApplyModifiedProperties();
    }
    private void _DrawText(string _text, Color _newColor, int _fontSize = 15)
    {
        GUIStyle guiStyle = new GUIStyle();
        guiStyle.normal.textColor = _newColor;
        guiStyle.fontSize = _fontSize;
        GUILayout.Label(_text, guiStyle);
    }
    private void _DrawText(string _text, Color _newColor, TextAnchor alignment, int _fontSize = 15)
    {
        GUIStyle guiStyle = new GUIStyle();
        guiStyle.normal.textColor = _newColor;
        guiStyle.alignment = alignment;
        guiStyle.fontSize = _fontSize;
        GUILayout.Label(_text, guiStyle);
    }
    private void _DrawProperty(string _propertyName)
    {
        var property = serializedObject.FindProperty(_propertyName);
        EditorGUILayout.PropertyField(property);
    }
}
#endif
