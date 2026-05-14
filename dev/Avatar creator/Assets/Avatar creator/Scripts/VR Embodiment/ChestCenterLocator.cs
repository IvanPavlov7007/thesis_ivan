using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Locates this transform at the sternum (or a chest center) position and the root's transform underneath it
/// </summary>
public class ChestCenterLocator : MonoBehaviour
{
    public Transform headsetTransform;  // Drag the XR Rig camera here
    public Transform rootTransform;
    public Vector3 sternumOffset = new Vector3(0, -0.3f, -0.2f);  // Customize based on the user height

    void Update()
    {
        Vector3 sternumPosition = headsetTransform.position + headsetTransform.TransformVector(sternumOffset);
        transform.position = sternumPosition;

        Vector3 localPost = transform.localPosition;
        localPost.y = 0f;

        rootTransform.localPosition = localPost;
    }
}
