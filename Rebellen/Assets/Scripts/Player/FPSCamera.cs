using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FPSCamera : MonoBehaviour
{
    [SerializeField] private string mouseXInputName, mouseYInputName;
    [SerializeField] private float mouseXsensitivity, mouseYsensitivity;

    [SerializeField] private Transform playerBody;

    private float xAxisClamp;

    private void Awake()
    {
        LockCursor();
        xAxisClamp = 0.0f;
    }

    private void LockCursor()
    {
        Cursor.lockState = CursorLockMode.Locked;
    }

    private void Update()
    {
        CameraRotation();
    }

    private void CameraRotation()
    {
        float mouseX = Input.GetAxis(mouseXInputName) * mouseXsensitivity * Time.deltaTime;
        float mouseY = Input.GetAxis(mouseYInputName) * mouseYsensitivity * Time.deltaTime;

        xAxisClamp += mouseY;

        if(xAxisClamp > 90)
        {
            xAxisClamp = 90;
            mouseY = 0.0f;
            ClampXAxisRotationToFloat(270.0f);
        }
        else if(xAxisClamp < -90)
        {
            xAxisClamp = -90;
            mouseY = 0.0f;
            ClampXAxisRotationToFloat(90.0f);
        }

        transform.Rotate(Vector3.left * mouseY);
        playerBody.Rotate(Vector3.up * mouseX);
    }

    private void ClampXAxisRotationToFloat(float value)
    {
        Vector3 eulerRotation = transform.eulerAngles;
        eulerRotation.x = value;
        transform.eulerAngles = eulerRotation;
    }
}
