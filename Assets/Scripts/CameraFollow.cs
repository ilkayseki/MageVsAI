using System;
using UnityEngine;

public class CameraFollow : MonoBehaviour {

    public Transform target;

    public float smoothSpeed = 0.125f;
    private Vector3 _offset;


    private void Awake()
    {
        _offset = transform.position;
    }

    void FixedUpdate ()
    {
        Vector3 desiredPosition = target.position + _offset;
        Vector3 smoothedPosition = Vector3.Lerp(transform.position, desiredPosition, smoothSpeed);
        transform.position = smoothedPosition;

        //transform.LookAt(target);
    }

}