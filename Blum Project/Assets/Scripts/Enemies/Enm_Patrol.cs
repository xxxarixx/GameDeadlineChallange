using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using CustomEditorAssistance;
using System.Linq;
using UnityEngine.Events;
[RequireComponent(typeof(Enm_Behaviour))]
public class Enm_Patrol : MonoBehaviour
{
    #region enums
    public enum EnemyType
    {
        Patrol_Edge_Edge,
        Patrol_Edge_Edge_WHitBoxChase,
        Patrol_DeceleratedPoints
    }
    public EnemyType enemyType;
    #endregion
    [HideInInspector]public Enm_Behaviour _data;


    [Header("patrol Declerated points")]
    public Transform dec_patrolPointsHolder;
    public List<PatrolPoint> dec_patrolPoints = new List<PatrolPoint>();
    public List<Transform> _dec_lastSavedPatrolPointsTransfroms = new List<Transform>();
    [Range(0.01f,2f)]public float _dec_checkForKnockOutOfPatrol_UpdateRate = 0.3f;
    public bool _dec_activePathVisualizesion = true;
    private float _dec_kockOutOfPatrol_process = 0f;
    private int _dec_currentPoint;
    private float _dec_jumpDelay;
    private bool _dec_reversed = false;
    [System.Serializable]
    public class PatrolPoint 
    {
        public string PointName = "Point";
        public Transform pointTransform;
        public UnityEvent onReachedPatrolPoint; 
        public PatrolPoint (Transform pointTransform)
        {
            this.pointTransform = pointTransform;
        }
    }

    public PatrolPointsState dec_OnLastPointDo;
    public enum PatrolPointsState
    {
        nothing,
        reverse,
        loop
    }
    private int _dec_lastPatrolPointsCount;
    [Header("patrol Edge_Edge WHitBoxChase")]
    public float eehit_attackSpeed;
    public Vector3 eehit_chaseHitBox;
    public Vector3 eehit_hitBoxSize;
    public LayerMask eehit_groundAndPlayerMask;
    public float eehit_chaseSpeedMultiplayer = 2f;

    private float _eehit__speedChase;
    private float _eehit_attackSpeedCooldown;
    private RaycastHit2D _eehit_shouldPerformAttack;
    private bool _eehit_attackAnimationEnded;

    [Header("patrol Edge_Edge")]
    private float _ee_groundRaycastLength = 0.3f;
    private float _ee_groundPatrolDirecitonRaycastLength = 0f;
    private bool _ee_grounded;
    private bool _ee_groundedPatrolDireciton;
    
    

    #region Unity functions
    private void Awake()
    {
        _data = GetComponent<Enm_Behaviour>();
    }
    private void OnValidate()
    {
        switch (enemyType)
        {
            case EnemyType.Patrol_Edge_Edge:
                break;
            case EnemyType.Patrol_Edge_Edge_WHitBoxChase:
                eehit_attackSpeed = Mathf.Clamp(eehit_attackSpeed, 0f, float.MaxValue);
                break;
            case EnemyType.Patrol_DeceleratedPoints:
                break;
            default:
                break;
        }

    }
    private void Start()
    {
        _RaycastsLengthSetup();
        _SpeedSetup();
        _data.refer.healthSystem.OnGetHitted += _HealthSystem_OnGetHitted;
    }
    private void FixedUpdate()
    {
        switch (enemyType)
        {
            case EnemyType.Patrol_Edge_Edge:
                _PatrolMovement();
                break;
            case EnemyType.Patrol_Edge_Edge_WHitBoxChase:
                _AttackValidate();
                _ChaseMovement(out bool isChasing);
                if(!isChasing)_PatrolMovement();
                break;
            case EnemyType.Patrol_DeceleratedPoints:
                _PatrolMovementWDeceleratedPoints();
                break;
            default:
                break;
        }

    }
    private void OnDrawGizmos()
    {
        if(_data == null) _data = GetComponent<Enm_Behaviour>();
        void PatrolLeftRightFunctions()
        {
            _RaycastsLengthSetup();
            _Draw_Raycast_Grounded();
            _Draw_Raycast_GroundedPatrolDireciton();
            _Draw_Raycast_InFront(_ee_groundPatrolDirecitonRaycastLength);
        }
        switch (enemyType)
        {
            case EnemyType.Patrol_Edge_Edge:
                PatrolLeftRightFunctions();
                break;
            case EnemyType.Patrol_Edge_Edge_WHitBoxChase:
                PatrolLeftRightFunctions();
                _Draw_ChaseHitBox();
                _Draw_HitBox();
                _Draw_Raycast_AttackValidate();
                _Draw_CanChase_Raycast();
                break;
            case EnemyType.Patrol_DeceleratedPoints:
                _Draw_ActivePathVisualizesion();
                _Draw_PatrolDeceleratedPointsAndConnections();
                _Draw_Raycast_InFront(.75f);
                _Draw_Raycast_Grounded();
                _Draw_Raycast_GroundedPatrolDireciton();
                break;
            default:
                break;
        }

    }
    private void OnDrawGizmosSelected()
    {
        switch (enemyType)
        {
            case EnemyType.Patrol_Edge_Edge:
                break;
            case EnemyType.Patrol_Edge_Edge_WHitBoxChase:
                break;
            case EnemyType.Patrol_DeceleratedPoints:
                _PatrolDeceleratedPointsEditorUpdateRemoveAndAdd();
                break;
            default:
                break;
        }
    }
    #endregion

    #region Patrol Edge_Edge
    private void _PatrolMovement()
    {
        if (_data._stopMove) return;
        _ee_groundedPatrolDireciton = _HittingGroundedPatrolDireciton_Raycast();
        _ee_grounded = _HittingGrounded_Raycast();
        if (!_ee_groundedPatrolDireciton && _ee_grounded || _HittingInFront_Raycast() && _ee_grounded)
        {
            _data._Flip();
        }
        _data.Move(_data.currentSpeed, Enm_Behaviour._MoveAxis.Horizontal);
        if (_ee_grounded)
        {
            _data.refer.PlayAnimation(Enm_References.animations.walk, 1);
            _data._SetCurrentMoveSpeed(Enm_Behaviour.SpeedType.Ground);
        }
        else
        {
            _data._SetCurrentMoveSpeed(Enm_Behaviour.SpeedType.InAir);
        }
    }
    private void _RaycastsLengthSetup()
    {
        _ee_groundPatrolDirecitonRaycastLength = _data.refer.grounded_Pivolt.localPosition.y + _ee_groundRaycastLength + .1f;
    }
    private bool _HittingGrounded_Raycast() => Physics2D.Raycast(_data.refer.flip_Pivolt.position, Vector3.down, _ee_groundRaycastLength, _data.groundMask);
    private bool _HittingGroundedPatrolDireciton_Raycast() => Physics2D.Raycast(_data.refer.grounded_Pivolt.position, Vector3.down, _ee_groundPatrolDirecitonRaycastLength, _data.groundMask);
    private bool _HittingInFront_Raycast() => Physics2D.Raycast(_data.refer.grounded_Pivolt.position, _data._FrontDirectiong(), _ee_groundPatrolDirecitonRaycastLength, _data.groundMask);
    private void _Draw_Raycast_Grounded() => _data._Draw_Raycast(_data.refer.flip_Pivolt.position, Vector3.down, _ee_groundRaycastLength, Color.white);
    private void _Draw_Raycast_GroundedPatrolDireciton() => _data._Draw_Raycast(_data.refer.grounded_Pivolt.position, Vector3.down, _ee_groundPatrolDirecitonRaycastLength, Color.yellow);
    private void _Draw_Raycast_InFront(float length) => _data._Draw_Raycast(_data.refer.grounded_Pivolt.position, _data._FrontDirectiong(), length, Color.green);
    #endregion

    #region Patrol Edge_Edge W HitBox Chase
    private void _SpeedSetup()
    {
        _eehit__speedChase = _data._GetSpeedEnumValue(Enm_Behaviour.SpeedType.Ground) * eehit_chaseSpeedMultiplayer;
    }
    private void _AttackValidate()
    {
        var attackHitBox = Physics2D.OverlapBox(_data.refer.attack_Pivolt.position + (_data.refer.flip_Pivolt.localScale.x * (eehit_hitBoxSize.x / 2) * Vector3.right), eehit_hitBoxSize, 0f, 1 << 8);
        _eehit_shouldPerformAttack = _HittingAttackValidate_Raycast();
        if (attackHitBox != null)
        {
            if (_eehit_shouldPerformAttack.collider == null || _eehit_shouldPerformAttack.collider != null && _eehit_shouldPerformAttack.collider.gameObject.CompareTag("Player"))
            {
                _data.SetForceStopMovement(true);
                if (_eehit_attackSpeedCooldown <= 0f)
                {
                    // stop and can attack
                    _eehit_attackAnimationEnded = false;
                    _data.refer.PlayAnimation(Enm_References.animations.attack, 2);
                    _eehit_attackSpeedCooldown = 1f / eehit_attackSpeed;
                }
            }
        }
        else
        {

            //if in previous frame attack should be performed attack and was applied force stop movement and in next frame player went from attack validate raycast then unlock movement
            if (_data._stopMove && !_eehit_shouldPerformAttack && _eehit_attackAnimationEnded) _data.SetForceStopMovement(false);
        }
        if (_eehit_attackSpeedCooldown > 0f)
        {
            _eehit_attackSpeedCooldown -= Time.deltaTime;
        }

        //performing attack cooldown
    }
    private void _ChaseMovement(out bool _isChasing)
    {
        _isChasing = false;
        if (_data._stopMove) return;
        var chaseHitBoxHit = Physics2D.OverlapBox(_data.refer.flip_Pivolt.position + (_data.refer.flip_Pivolt.localScale.x * (eehit_chaseHitBox.x / 2) * Vector3.right) + (eehit_chaseHitBox.y / 2) * Vector3.up, eehit_chaseHitBox, 0f, 1 << 8);
        if (chaseHitBoxHit != null)
        {
            //detect ground and player
            var canChaseRaycastHit = Physics2D.Raycast(_data.refer.flip_Pivolt.position + (eehit_chaseHitBox.y / 2) * Vector3.up, _data._FrontDirectiong(), eehit_chaseHitBox.x, eehit_groundAndPlayerMask);
            if (canChaseRaycastHit.collider == null || canChaseRaycastHit.collider != null && canChaseRaycastHit.collider.gameObject.CompareTag("Player")) //if player hit first or nothing was hitted then chase player 
            {
                _data._SetCurrentMoveSpeed(_eehit__speedChase);
                _data.refer.PlayAnimation(Enm_References.animations.walk, 1);
                _data.Move(_data.currentSpeed, Enm_Behaviour._MoveAxis.Horizontal);
                _isChasing = true;
            }
            else
            {
                _isChasing = false;
            }
        }
        else
        {
            _isChasing = false;
        }
        if(!_isChasing) _data._SetCurrentMoveSpeed(Enm_Behaviour.SpeedType.Ground);
    }
    public void AttackAnimationEnded()
    {
        _data.refer.ResetAnimationPriority();
        _data.SetForceStopMovement(false);
        _eehit_attackAnimationEnded = true;
    }
    private void _Draw_ChaseHitBox()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireCube(_data.refer.flip_Pivolt.position + (_data.refer.flip_Pivolt.localScale.x * (eehit_chaseHitBox.x / 2) * Vector3.right) + (eehit_chaseHitBox.y / 2) * Vector3.up, eehit_chaseHitBox);
    }
    private void _Draw_CanChase_Raycast() => _data._Draw_Raycast(_data.refer.flip_Pivolt.position + (eehit_chaseHitBox.y / 2) * Vector3.up, _data._FrontDirectiong(), eehit_chaseHitBox.x, Color.red);
    private RaycastHit2D _HittingAttackValidate_Raycast() => Physics2D.Raycast(_data.refer.attack_Pivolt.position, _data._FrontDirectiong(), eehit_hitBoxSize.x, eehit_groundAndPlayerMask);
    private void _Draw_HitBox()
    {
        Gizmos.color = Color.gray;
        Gizmos.DrawWireCube(_data.refer.attack_Pivolt.position + (_data.refer.flip_Pivolt.localScale.x * (eehit_hitBoxSize.x / 2) * Vector3.right), eehit_hitBoxSize);
    }
    private void _Draw_Raycast_AttackValidate() => _data._Draw_Raycast(_data.refer.attack_Pivolt.position, _data._FrontDirectiong(), eehit_hitBoxSize.x, Color.gray);
    #endregion

    #region Patrol W decelerated Points
    private void _PatrolMovementWDeceleratedPoints()
    {
        if (dec_patrolPoints.Count == 0 || dec_patrolPoints.Count == 1)
        {
            _data.refer.PlayAnimation(Enm_References.animations.idle, 2);
            return;
        } 
        if (_data._stopMove) return;
        //if enemy is knocked out of platform then check if he can reach any point if not then change enemy type to patrol edge edge
        bool IsNearAnyPoint()
        {
            bool isNear = false;
            for (int i = 0; i < dec_patrolPoints.Count; i++)
            {
                var curPatrolPoint = dec_patrolPoints[i];
                //1.get distance between enemy and current patrol point
                //2.get hit ground in this distance
                //3.if hitted ground then check if distance from hit and patrol point is less then 0.5f can return to path but if not hitted then can return to path too
                //4.if in point 3. distance is too high then change enemy state to patrol
                var distance = Vector3.Distance(_data.refer.flip_Pivolt.position, curPatrolPoint.pointTransform.position);
                var direction = (curPatrolPoint.pointTransform.position - _data.refer.flip_Pivolt.position).normalized;
                var hittedGround = Physics2D.Raycast(_data.refer.flip_Pivolt.position, direction, distance, _data.groundMask);
                if(hittedGround.collider != null && Vector2.Distance(hittedGround.point, curPatrolPoint.pointTransform.position) < 1f || hittedGround.collider == null)
                {
                    isNear = true;
                    break;
                }
            }
            return isNear;
        }
        if(_data.refer.healthSystem.knockMultiplayer > 0f)
        {
            if(_dec_kockOutOfPatrol_process <= 0f)
            {
                if (!IsNearAnyPoint())
                {
                    enemyType = EnemyType.Patrol_Edge_Edge;
                    _dec_kockOutOfPatrol_process = _dec_checkForKnockOutOfPatrol_UpdateRate;
                    return;
                }
            }
            else
            {
                _dec_kockOutOfPatrol_process -= Time.deltaTime;
            }
        }
        //is near next point
        if (Vector3.Distance(_data.refer.flip_Pivolt.position, dec_patrolPoints[_dec_currentPoint].pointTransform.position) < .4f )
        {
            //reached destination
            dec_patrolPoints[_dec_currentPoint].onReachedPatrolPoint?.Invoke();
            _dec_currentPoint = _GetNextPointIndex(_dec_currentPoint);
        }
        
        var moveDir = (dec_patrolPoints[_dec_currentPoint].pointTransform.position - _data.refer.flip_Pivolt.position).normalized;
        var frontHit = Physics2D.Raycast(_data.refer.grounded_Pivolt.position, _data._FrontDirectiong(), .75f, _data.groundMask);
        bool pointIsHeigher = (dec_patrolPoints[_dec_currentPoint].pointTransform.position.y - _data.refer.flip_Pivolt.position.y) > .2f;
        float pointDistanceX = Mathf.Abs(_data.refer.flip_Pivolt.position.x - dec_patrolPoints[_dec_currentPoint].pointTransform.position.x);
        //automatic jump based on point height
        if ((frontHit || !frontHit && !_HittingGroundedPatrolDireciton_Raycast()) && pointIsHeigher && _HittingGrounded_Raycast() && _dec_jumpDelay <= 0f)
        {
            _data.Move(Mathf.Abs(_data.refer.flip_Pivolt.position.y - dec_patrolPoints[_dec_currentPoint].pointTransform.position.y) * 205f, Enm_Behaviour._MoveAxis.Vertical);
            _dec_jumpDelay = 0.75f;
        }
        //move and rotate torward destination if isnt close enough
        if(pointDistanceX > .1f)
        {
            _data.FaceTarget(moveDir);
            _data.Move(_data.currentSpeed, Enm_Behaviour._MoveAxis.Horizontal);
        }
        else
        {
            //just let enemy fall if is too close on X axis
            _data.refer.rb.velocity = new Vector2(0f,_data.refer.rb.velocity.y);
        }

        _data.refer.PlayAnimation(Enm_References.animations.walk, 0);
        if (_dec_jumpDelay > 0f)
        {
            _dec_jumpDelay -= Time.deltaTime;
        }
    }
    private PatrolPoint _GetNextPoint(int currentPoint)
    {
        int nextPointIndex = 0;
        switch (dec_OnLastPointDo)
        {
            case PatrolPointsState.reverse:
                if (currentPoint + 1 >= dec_patrolPoints.Count)
                {
                    nextPointIndex = currentPoint - 1;
                    _dec_reversed = true;
                }
                else
                {
                    if(_dec_reversed && currentPoint - 1 < 0)
                    {
                        _dec_reversed = !_dec_reversed;
                    }
                    if(_dec_reversed) nextPointIndex = currentPoint - 1; else nextPointIndex = currentPoint + 1;
                    
                }
                break;
            case PatrolPointsState.loop:
                if (currentPoint + 1 >= dec_patrolPoints.Count)
                {
                    nextPointIndex = 0;
                }
                else
                {
                    nextPointIndex = currentPoint + 1;
                }
                break;
            case PatrolPointsState.nothing:
                if (currentPoint + 1 >= dec_patrolPoints.Count)
                {
                    _data.SetForceStopMovement(true);
                    _data.refer.PlayAnimation(Enm_References.animations.idle, 0);
                }
                else
                {
                    nextPointIndex = currentPoint + 1;
                }
                break;
                    default:
                break;
        }
        return dec_patrolPoints[nextPointIndex];
    }
    private int _GetNextPointIndex(int currentPoint)
    {
        int nextPointIndex = 0;
        switch (dec_OnLastPointDo)
        {
            case PatrolPointsState.reverse:
                if (currentPoint + 1 >= dec_patrolPoints.Count)
                {
                    nextPointIndex = currentPoint - 1;
                    _dec_reversed = true;
                }
                else
                {
                    if (_dec_reversed && currentPoint - 1 < 0)
                    {
                        _dec_reversed = !_dec_reversed;
                    }
                    
                    if (_dec_reversed) nextPointIndex = currentPoint - 1; else nextPointIndex = currentPoint + 1;
                    
                }
                break;
            case PatrolPointsState.loop:
                if (currentPoint + 1 >= dec_patrolPoints.Count)
                {
                    nextPointIndex = 0;
                }
                else
                {
                    nextPointIndex = currentPoint + 1;
                }
                break;
            case PatrolPointsState.nothing:
                if (currentPoint + 1 >= dec_patrolPoints.Count)
                {
                    _data.SetForceStopMovement(true);
                    _data.refer.PlayAnimation(Enm_References.animations.idle, 0);
                }
                else
                {
                    nextPointIndex = currentPoint + 1;
                }
                break;
            default:
                break;
        }
        return nextPointIndex;
    }
    private void _HealthSystem_OnGetHitted(Vector3 _hitInvokerPosition)
    {
        //hitted from behind
        var dir = (_hitInvokerPosition - transform.position).normalized;
        var frontDir = _data._FrontDirectiong();
        if (Vector2.Dot(frontDir,dir) < 0)
        {
            
            switch (enemyType)
            {
                case EnemyType.Patrol_Edge_Edge:
                    _data._Flip();
                    break;
                case EnemyType.Patrol_Edge_Edge_WHitBoxChase:
                    _data._Flip();
                    _eehit_attackSpeedCooldown = (1f / eehit_attackSpeed) / 4;
                    break;
                case EnemyType.Patrol_DeceleratedPoints:
                    break;
                default:
                    break;
            }
        } 
    }
    private string _GetPointName(int currentPoint)
    {
        return (dec_patrolPoints[currentPoint].PointName != string.Empty) ? dec_patrolPoints[currentPoint].PointName : $"Point{currentPoint}";
    }
    private void _PatrolDeceleratedPointsEditorUpdateRemoveAndAdd()
    {
        if (dec_patrolPointsHolder == null || dec_patrolPoints.Count < 2) return;
        void CreateControlPointTransform(int index)
        {
            GameObject go = new GameObject();
            go.transform.SetParent(dec_patrolPointsHolder);
            go.transform.position = transform.position;
            int patrolPointIndex = index;
            go.name = _GetPointName(index);
            var selectedPoint = dec_patrolPoints[patrolPointIndex];
            selectedPoint.pointTransform = go.transform;
            _dec_lastSavedPatrolPointsTransfroms.Clear();
            foreach (var item in dec_patrolPoints)
            {
                _dec_lastSavedPatrolPointsTransfroms.Add(item.pointTransform);
            }
        }
        //check if someone remove point transform property or destroyed from Hierarchy manually
        if(dec_patrolPoints.Any(x => x.pointTransform == null) && dec_patrolPointsHolder.childCount != dec_patrolPoints.Count)
        {
            for (int i = 0; i < dec_patrolPoints.Count; i++)
            {
                if (dec_patrolPoints[i].pointTransform == null) CreateControlPointTransform(i);
            }
        }
        else if(dec_patrolPoints.Any(x => x.pointTransform == null) && dec_patrolPointsHolder.childCount == dec_patrolPoints.Count)
        {
            for (int i = 0; i < dec_patrolPoints.Count; i++)
            {
                if (dec_patrolPoints[i].pointTransform == null) dec_patrolPoints[i].pointTransform = _dec_lastSavedPatrolPointsTransfroms[i];
            }
        }
        //check for null transforms in last saved
        if (_dec_lastSavedPatrolPointsTransfroms != null)
        {
            for (int i = 0; i < _dec_lastSavedPatrolPointsTransfroms.Count; i++)
            {
                if (_dec_lastSavedPatrolPointsTransfroms[i] == null) _dec_lastSavedPatrolPointsTransfroms.RemoveAt(i);
            }
        }
        else
        {
            _dec_lastSavedPatrolPointsTransfroms = new List<Transform>();
        }
        //sibling index corrector and name corrector
        for (int i = 0; i < dec_patrolPoints.Count; i++)
        {
            if (dec_patrolPoints[i].pointTransform.GetSiblingIndex() != i) dec_patrolPoints[i].pointTransform.SetSiblingIndex(i);
            if (dec_patrolPoints[i].pointTransform.name != _GetPointName(i)) dec_patrolPoints[i].pointTransform.name = _GetPointName(i);
        }

        //update list remove input 
        if (_dec_lastSavedPatrolPointsTransfroms.Count > dec_patrolPoints.Count)
        {
            int indexToRemove = -1;
            for (int i = 0; i < _dec_lastSavedPatrolPointsTransfroms.Count; i++)
            {
                if (i >= dec_patrolPoints.Count)
                {
                    //last index removed
                    indexToRemove = _dec_lastSavedPatrolPointsTransfroms.Count - 1;
                    break;
                }
                else
                {
                    //removed something in middle
                    if (_dec_lastSavedPatrolPointsTransfroms[i] != dec_patrolPoints[i].pointTransform)
                    {
                        indexToRemove = i;
                        break;
                    }
                }
            }
            if (indexToRemove != -1) DestroyImmediate(_dec_lastSavedPatrolPointsTransfroms[indexToRemove].gameObject, true);
            _dec_lastSavedPatrolPointsTransfroms.RemoveAt(indexToRemove);
        }


      
        
        if (dec_patrolPointsHolder.childCount != dec_patrolPoints.Count)
        {
            if (dec_patrolPointsHolder.childCount < dec_patrolPoints.Count)
            {
                CreateControlPointTransform(dec_patrolPointsHolder.childCount - 1);
            }
        }
        
        
    }
    private void _Draw_PatrolDeceleratedPointsAndConnections()
    {
        if (dec_patrolPointsHolder == null) return;
        if (dec_patrolPoints.Count < 2) return;
        if (!_dec_activePathVisualizesion) return;
        
        foreach (Transform Child in dec_patrolPointsHolder)
        {
            if (!_dec_lastSavedPatrolPointsTransfroms.Contains(Child)) DestroyImmediate(Child.gameObject, true);
        }
        Gizmos.color = Color.white;
        for (int i = 0; i < dec_patrolPoints.Count; i++)
        {
            var point = dec_patrolPoints[i];
            if (point == null) continue;
            if (point.pointTransform == null) continue;
            if (_GetNextPoint(i) == null) continue;
            if (_GetNextPoint(i).pointTransform == null) continue;
            Gizmos.DrawSphere(point.pointTransform.position, .1f);
            Gizmos.DrawLine(point.pointTransform.position, _GetNextPoint(i).pointTransform.position);
        }
        
    }
    public void _OnReachedEnd_Wait(float waitTime)
    {
        StartCoroutine(waitAndIdle(waitTime));
    }
    private IEnumerator waitAndIdle(float waitTime)
    {
        _data.SetForceStopMovement(true,true,false);
        _data.refer.PlayAnimation(Enm_References.animations.idle, 0);
        yield return new WaitForSeconds(waitTime);
        _data.SetForceStopMovement(false);
    }
    private void _Draw_ActivePathVisualizesion()
    {
        if (_dec_activePathVisualizesion)
        {
            _data._Draw_Circle(_data.refer.flip_Pivolt.position + 1f * Vector3.up, .2f, Color.green);
        }
        else
        {
            _data._Draw_Circle(_data.refer.flip_Pivolt.position + 1f * Vector3.up, .2f, Color.red);
        }
    }
    #endregion
    
    
    

}



#if UNITY_EDITOR

[CustomEditor(typeof(Enm_Patrol))]
public class Enm_Patrol_Editor : Editor
{
    public override void OnInspectorGUI()
    {
        Enm_Patrol _Patrol = (Enm_Patrol)target;
        _DefaultProperties(_Patrol);
        switch (_Patrol.enemyType)
        {
            case Enm_Patrol.EnemyType.Patrol_Edge_Edge:
                
                break;
            case Enm_Patrol.EnemyType.Patrol_Edge_Edge_WHitBoxChase:
                CustomEditorAssistance_._DrawProperty(serializedObject, nameof(_Patrol.eehit_chaseSpeedMultiplayer));
                CustomEditorAssistance_._DrawProperty(serializedObject, nameof(_Patrol.eehit_chaseHitBox));
                CustomEditorAssistance_._DrawProperty(serializedObject, nameof(_Patrol.eehit_hitBoxSize));
                CustomEditorAssistance_._DrawProperty(serializedObject, nameof(_Patrol.eehit_attackSpeed));
                CustomEditorAssistance_._DrawProperty(serializedObject, nameof(_Patrol.eehit_groundAndPlayerMask));
                CustomEditorAssistance_._DrawText($"attack every {1f / _Patrol.eehit_attackSpeed} s ", Color.gray);
                break;
            case Enm_Patrol.EnemyType.Patrol_DeceleratedPoints:
                if(_Patrol.dec_patrolPoints.Count < 2) CustomEditorAssistance_._DrawText($"add {2 - _Patrol.dec_patrolPoints.Count} more patrol points to move between them otherwise he will play only idle animation", Color.red, 13, true);
                CustomEditorAssistance_._DrawProperty(serializedObject, nameof(_Patrol.dec_patrolPointsHolder));
                CustomEditorAssistance_._DrawProperty(serializedObject, nameof(_Patrol.dec_OnLastPointDo));
                CustomEditorAssistance_._DrawProperty(serializedObject, nameof(_Patrol.dec_patrolPoints));
                if (GUILayout.Button("Reverse List", GUILayout.Width(85f)))
                {
                    _Patrol.dec_patrolPoints.Reverse();
                    _Patrol._dec_lastSavedPatrolPointsTransfroms.Reverse();
                }
                CustomEditorAssistance_._DrawProperty(serializedObject, nameof(_Patrol._dec_activePathVisualizesion));
                if (_Patrol._data.refer.healthSystem.knockMultiplayer > 0f)CustomEditorAssistance_._DrawProperty(serializedObject, nameof(_Patrol._dec_checkForKnockOutOfPatrol_UpdateRate));
                if(_Patrol._data.refer.healthSystem.knockMultiplayer > 0f)CustomEditorAssistance_._DrawText($"tip1: enemy knock back multiplayer is heigher then 0 so you should set patrol point after every obstacle to avoid complications ", Color.yellow, 13, true);
                CustomEditorAssistance_._DrawText($"tip2: if point transfrom was removed then it will automatically repaint when pressed on object that holding this script, so dont worry :)", Color.gray, 13, true);
                //CustomEditorAssistance_._DrawProperty(serializedObject, nameof(_Patrol._dec_lastSavedPatrolPointsTransfroms));
                break;
            default:
                break;
        }


        //save changes
        serializedObject.ApplyModifiedProperties();
    }
    #region Custom Private Functions
    private void _DefaultProperties(Enm_Patrol _Patrol)
    {
        CustomEditorAssistance_._DrawProperty(serializedObject, nameof(_Patrol.enemyType));
    }
    #endregion
}




#endif