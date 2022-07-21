using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
public class Enm_Behaviour : MonoBehaviour
{
    public Enm_References data;
    public EnemyType enemyType;
    public enum EnemyType
    {
        PatrolJust,
        PatrolWRadialChase
    }
    [Header("allType")]
    [SerializeField] private int damagePerHit;
    public float currentSpeed { get; private set; }
    [SerializeField] private float maxSpeed;
    [SerializeField] private LayerMask groundMask;
    [Header("patrolWRadialChase")]
    [SerializeField]private float radialRadious;
    [SerializeField] private Vector3 hitBoxSize;
    private Vector3 _moveDir;
    void Start()
    {
        currentSpeed = maxSpeed;
        _moveDir = new Vector3(1f, 0f, 0f);
    }
    private bool _grounded;
    void Update()
    {
        _grounded = Physics2D.Raycast(data.grounded_Pivolt.position, Vector3.down,0.5f, groundMask);
        if (!_grounded || Physics2D.Raycast(data.grounded_Pivolt.position,Vector2.right * Mathf.Clamp(data.rb.velocity.x, -1,1), 0.5f, groundMask))
        {
            _moveDir.x = -_moveDir.x;
            data.rb.velocity = Vector3.zero;
            data.flip_Pivolt.localScale = new Vector3(-data.flip_Pivolt.localScale.x, data.flip_Pivolt.localScale.y, data.flip_Pivolt.localScale.z);
        } 
    }

    private void FixedUpdate()
    {
        if (_grounded)
        {
            data.rb.velocity = _moveDir * currentSpeed * Time.fixedDeltaTime;
            data.PlayAnimation(Enm_References.animations.walk, 0);
        }
    }
    public void Move()
    {

    }
    private void OnDrawGizmos()
    {
        switch (enemyType)
        {
            case EnemyType.PatrolJust:
                Gizmos.color = Color.green;
                Gizmos.DrawLine(data.grounded_Pivolt.position, data.grounded_Pivolt.position + Vector3.down * 0.5f);
                Gizmos.DrawLine(data.grounded_Pivolt.position, data.grounded_Pivolt.position + Vector3.right * Mathf.Clamp(data.rb.velocity.x, -1, 1) * 0.5f);
                break;
            case EnemyType.PatrolWRadialChase:
                Gizmos.color = Color.green;
                Gizmos.DrawLine(data.flip_Pivolt.position, data.flip_Pivolt.position + Vector3.down * 0.5f);
                Gizmos.DrawLine(data.grounded_Pivolt.position, data.grounded_Pivolt.position + Vector3.right * Mathf.Clamp(data.rb.velocity.x, -1, 1) * 0.5f);
                Gizmos.color = Color.red;
                Gizmos.DrawWireSphere(data.flip_Pivolt.position,radialRadious);
                break;
            default:
                break;
        }
    }
}



#if UNITY_EDITOR

[CustomEditor(typeof(Enm_Behaviour))]
public class Enm_Behaviour_Editor : Editor
{
    public override void OnInspectorGUI()
    {
        //base.OnInspectorGUI();
        _DefaultProperties();
        Enm_Behaviour _Behaviour = (Enm_Behaviour)target;
        switch (_Behaviour.enemyType)
        {
            case Enm_Behaviour.EnemyType.PatrolJust:
                
                break;
            case Enm_Behaviour.EnemyType.PatrolWRadialChase:
                _DrawProperty("radialRadious");
                _DrawProperty("hitBoxSize");
                break;
            default:
                break;
        }


        //save changes
        EditorUtility.SetDirty(_Behaviour);
        serializedObject.ApplyModifiedProperties();
    }
    private void _DrawProperty(string _propertyName)
    {
        var property = serializedObject.FindProperty(_propertyName);
        EditorGUILayout.PropertyField(property);
    }
    private void _DefaultProperties()
    {
        _DrawProperty("data");
        _DrawProperty("enemyType");
        _DrawProperty("damagePerHit");
        _DrawProperty("maxSpeed");
        _DrawProperty("groundMask");
    }
}




#endif