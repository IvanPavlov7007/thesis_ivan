using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// locates and rotates this transform directly under the head 
/// </summary>
public class StaticProjectionVRController : MonoBehaviour
{
    [SerializeField]
    Transform head;
    [SerializeField]
    LayerMask groundLayer;

    private void LateUpdate()
    {
        RaycastHit hit;
        Vector3 pos = head.position - Vector3.down * 1.5f;
        if (Physics.Raycast(head.transform.position, Vector3.down, out hit, 10f, groundLayer))
        {
            pos = hit.point;
        }
        transform.position = pos;

        Vector3 forward = Vector3.ProjectOnPlane(head.forward, Vector3.up);
        transform.forward = forward;
    }
}
