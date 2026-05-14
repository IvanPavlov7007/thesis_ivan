using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//a simple approach which is found anywhere in the internet


/// <summary>
/// Controls the "mirror camera" position and look-at direction so that it's view can be rendered
/// on a plane
/// </summary>
[ExecuteAlways]
public class MirrorCamera : MonoBehaviour
{
    public Transform mainCamera;
    public Transform mirrorOrigin;
    public Transform mirrorPlane;
    private void LateUpdate()
    {
        Vector3 localMainCamPos = mirrorOrigin.InverseTransformPoint(mainCamera.position);

        Vector3 lookAtTarget = mirrorOrigin.TransformPoint(new Vector3(-localMainCamPos.x, localMainCamPos.y, localMainCamPos.z));

        transform.localPosition = new Vector3(localMainCamPos.x, localMainCamPos.y, -localMainCamPos.z);
        transform.LookAt(lookAtTarget);

        Vector3 mirrorPos = mirrorPlane.position;
        mirrorPos.y = transform.position.y;
        mirrorPlane.position = mirrorPos;
    }
}
