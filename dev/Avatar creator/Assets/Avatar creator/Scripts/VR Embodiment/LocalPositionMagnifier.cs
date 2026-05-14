using System.Collections;
using System.Collections.Generic;
using UnityEngine;


/// <summary>
/// Magnifies this transform's local position
/// requires parent as center of coordinates
/// </summary>
public class LocalPositionMagnifier : MonoBehaviour
{
    public Transform target;
    public float magnifier = 1f;

    void Update()
    {
        //Center of coordinates
        Transform co = transform.parent;

        transform.localPosition = co.InverseTransformPoint(target.position) * magnifier;
        transform.rotation = target.rotation;
    }
}
