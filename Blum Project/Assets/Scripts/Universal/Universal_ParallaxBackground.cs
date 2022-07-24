using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Universal_ParallaxBackground : MonoBehaviour
{
    private float _spriteXLength;
    private float _startPosX;
    public float parallaxEffectSpeed;
    private void Start()
    {
        _startPosX = transform.position.x;
        _spriteXLength = GetComponent<SpriteRenderer>().bounds.size.x;
    }
    private void Update()
    {
        var cam = Main_CameraController.instance.mainCam;
        float temp = (cam.transform.position.x * (1 - parallaxEffectSpeed));
        float distX = (cam.transform.position.x * parallaxEffectSpeed);

        transform.position = new Vector3(_startPosX + distX, transform.position.y, transform.position.z);
        if (temp > _startPosX + _spriteXLength) _startPosX += _spriteXLength;
        else if (temp < _startPosX - _spriteXLength) _startPosX -= _spriteXLength;
    }
}
