using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MoveAroundObject : MonoBehaviour
{
    [SerializeField]
    private float _mouseSensitivity = 3.0f;

    private float _rotationY;
    private float _rotationX;

    [SerializeField]
    private Transform _target;

    [SerializeField]
    private float _distanceFromTarget = 3.0f;

    private Vector3 _currentRotation;
    private Vector3 _smoothVelocity = Vector3.zero;

    [SerializeField]
    private float _smoothTime = 0.2f;

    [SerializeField]
    private Vector2 _rotationXMinMax = new Vector2(-40, 40);

    [Header("CameraSettings")]
    public float cameraSpeed = 1.0f;

    void Update()
    {
        float mouseX = Input.GetAxis("Mouse X") * _mouseSensitivity;
        float mouseY = Input.GetAxis("Mouse Y") * _mouseSensitivity;

        _rotationY += mouseX;
        _rotationX += mouseY;

        // Apply clamping for x rotation 
        _rotationX = Mathf.Clamp(_rotationX, _rotationXMinMax.x, _rotationXMinMax.y);

        Vector3 nextRotation = new Vector3(_rotationX, _rotationY);

        // Apply damping between rotation changes
        _currentRotation = Vector3.SmoothDamp(_currentRotation, nextRotation, ref _smoothVelocity, _smoothTime);
        transform.localEulerAngles = _currentRotation;

        // Substract forward vector of the GameObject to point its forward vector to the target
        transform.position = _target.position - transform.forward * _distanceFromTarget;
        CheckTranslation();
        _distanceFromTarget = (transform.position - _target.position).magnitude;
    }

    private void CheckTranslation()
    {
        Vector3 CameraPosition = transform.position;

        if (Input.GetKey(KeyCode.W))
        {
            CameraPosition.z -=  cameraSpeed / 50;
        }
        if (Input.GetKey(KeyCode.S))
        {
            CameraPosition.z += cameraSpeed / 50;
        }
        if (Input.GetKey(KeyCode.A))
        {
            CameraPosition.x += cameraSpeed / 50;
        }
        if (Input.GetKey(KeyCode.D))
        {
            CameraPosition.x -= cameraSpeed / 50;
        }

        transform.position = CameraPosition;
    }
}