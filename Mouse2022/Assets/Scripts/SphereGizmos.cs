using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SphereGizmos : MonoBehaviour
{
    [SerializeField, Range(0f, 5f)] float sphereRadius = 1f;// Radius of the sphere
    public Color gizmoColor = Color.red;// Color of the sphere

    void OnDrawGizmos()
    {
        // Set the color of the gizmo
        Gizmos.color = gizmoColor;

        // Draw the sphere at the specified position with the specified radius
        Gizmos.DrawSphere(transform.position, sphereRadius);
    }
}
