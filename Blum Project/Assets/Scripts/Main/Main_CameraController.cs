using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[ExecuteInEditMode]
public class Main_CameraController : MonoBehaviour
{
    public static Main_CameraController instance;
    public Camera mainCam;
    [SerializeField] private Transform target;
    [SerializeField] private Vector3 offset;
    [SerializeField] private float smothnessVelocityCamera;
    [SerializeField] private float smothnessOverallCamera;
    private Vector3 lastAppliedVelocity;
    private Vector3 appliedVelocityOffset;
    [SerializeField] private Vector2 velocityCameraOffsetMultiplayer = Vector2.one;
    private void Awake()
    {
        instance = this;
    }
    private void FixedUpdate()
    {
        //camera movement tilt torward player velocity dependly on independent smoothness
        Vector3 finalPosition = target.position + offset + Vector3.Lerp(lastAppliedVelocity,appliedVelocityOffset, smothnessVelocityCamera * Time.deltaTime);
        //camera movement process dependly on independent smoothness
        _SetCameraPosition(Vector3.Lerp(mainCam.transform.position, finalPosition, smothnessOverallCamera * Time.deltaTime));
        lastAppliedVelocity = appliedVelocityOffset;
    }
    private void _SetCameraPosition(Vector3 position)
    {
        mainCam.transform.position = new Vector3(position.x, position.y, -10);
    }
    public void SetVelocityOffset(Vector3 velocityOffset, float xLimit = 1f,float yLimit = 1f)
    {
        appliedVelocityOffset = new Vector3(Mathf.Clamp(velocityOffset.x * velocityCameraOffsetMultiplayer.x, -xLimit, xLimit), Mathf.Clamp(velocityOffset.y * velocityCameraOffsetMultiplayer.y, -yLimit,yLimit));
    }
    void OnDrawGizmos()
    {
        if (!Application.isPlaying)
        {
            _SetCameraPosition(target.position + offset);
        }
    }
}
