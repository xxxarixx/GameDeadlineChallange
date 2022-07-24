using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
public class Ui_DamagePopup : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField]private AnimationCurve scale_OverTime;
    [SerializeField]private AnimationCurve velocity_Y_OverTime;
    [SerializeField]private Gradient color_OverTime;
    [Header("Other")]
    [SerializeField] private bool editor_InPlayModePlayAnimationAgain;
    [SerializeField] private bool editor_ForceStayAlive = false;
    [SerializeField] private Vector2 velocityRandom_X_MinMax;
    private RectTransform _rectTransform;
    private TextMeshProUGUI textMesh;
    private float _progress;
    private float _timeToFinish;
    private Vector2 _editor_StartingPosition;
    private float _choosedXVelocity;
    
    private void Start()
    {
        _SetupVeriables();
        _editor_StartingPosition = _rectTransform.position;
        _choosedXVelocity = Random.Range(velocityRandom_X_MinMax.x, velocityRandom_X_MinMax.y);
    }
    private void Update()
    {
        _ProcessAnimation();
    }
    private void _ProcessAnimation()
    {
        if (_progress <= _timeToFinish)
        {
            //in progress
            _progress += Time.deltaTime;
            var scale = scale_OverTime.Evaluate(_progress);
            var velocity = velocity_Y_OverTime.Evaluate(_progress);
            _rectTransform.localScale = new Vector3(scale, scale, scale);
            _rectTransform.Translate(new Vector3(_choosedXVelocity, velocity, 0f), Space.World);
            textMesh.color = color_OverTime.Evaluate(_progress);
        }
        else
        {
            //end
            if (!editor_ForceStayAlive) Destroy(gameObject);
        }
        if (editor_InPlayModePlayAnimationAgain)
        {
            editor_InPlayModePlayAnimationAgain = false;
            _rectTransform.position = _editor_StartingPosition;
            _SetupVeriables();
        }
    }
    private void _SetupVeriables()
    {
        textMesh = GetComponent<TextMeshProUGUI>();
        _rectTransform = GetComponent<RectTransform>();
        _progress = 0f;
        var scaleMaxTime = scale_OverTime.keys[scale_OverTime.length - 1].time;
        var velocityMaxTime = velocity_Y_OverTime.keys[velocity_Y_OverTime.length - 1].time;
        _timeToFinish = (scaleMaxTime > velocityMaxTime) ? scaleMaxTime : velocityMaxTime;
    }
}
