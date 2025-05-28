using System;
using UnityEngine;

public class Rotate : MonoBehaviour
{
    [SerializeField] private RotateSpace rotateSpace = RotateSpace.Global;
    [SerializeField] private bool useX, useY, useZ; 
    [SerializeField] private float rotateSpeed = 1.0f;
    
    private Vector3 rotateVector = Vector3.zero;
    
    private void FixedUpdate()
    {
        if (useX)
        {
            rotateVector.x = 1;
        }

        if (useY)
        {
            rotateVector.y = 1;
        }

        if (useZ)
        {
            rotateVector.z = 1;
        }

        if (rotateVector != Vector3.zero)
        {
            if (rotateSpace == RotateSpace.Global)
            {
                transform.rotation *= Quaternion.Euler(rotateVector * rotateSpeed * Time.deltaTime);
            }
            else
            {
                transform.localRotation *= Quaternion.Euler(rotateVector * rotateSpeed * Time.deltaTime);
            }
        }
    }

    public enum RotateSpace
    {
        Global = 0,
        Local = 1
    }
}
