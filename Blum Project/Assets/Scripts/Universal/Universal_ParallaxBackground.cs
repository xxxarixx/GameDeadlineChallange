using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Universal_ParallaxBackground : MonoBehaviour
{
    private float spriteXLength;
    private float startPosX;
    public float parallaxEffectSpeed;
    private void Start()
    {
        startPosX = transform.position.x;
        spriteXLength = GetComponent<SpriteRenderer>().bounds.size.x;
    }
    private void Update()
    {
        var cam = Main_CameraController.instance.mainCam;
        float temp = (cam.transform.position.x * (1 - parallaxEffectSpeed));
        float distX = (cam.transform.position.x * parallaxEffectSpeed);

        transform.position = new Vector3(startPosX + distX, transform.position.y, transform.position.z);
        if (temp > startPosX + spriteXLength) startPosX += spriteXLength;
        else if (temp < startPosX - spriteXLength) startPosX -= spriteXLength;
    }
}
