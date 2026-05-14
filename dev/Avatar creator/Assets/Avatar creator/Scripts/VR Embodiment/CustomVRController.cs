using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UMA;
using UMA.CharacterSystem;

//script taken and customized from https://www.youtube.com/watch?v=RaDSUd6GSjs

[System.Serializable]
public class MapTransform
{
    public Transform vrTarget;
    public Transform IKTarget;
    public Vector3 trackingPositionOffset;
    public Vector3 trackingRotationOffset;

    public void Map()
    {
        IKTarget.position = vrTarget.TransformPoint(trackingPositionOffset);
        IKTarget.rotation = vrTarget.rotation * Quaternion.Euler(trackingRotationOffset);
    }
}

/// <summary>
/// Maps IK targets to given Transforms with offsets
/// </summary>
public class CustomVRController : MonoBehaviour
{
    [SerializeField] private MapTransform head;
    [SerializeField] private MapTransform leftHand;
    [SerializeField] private MapTransform rightHand;

    [SerializeField] private float turnSmoothness;

    [SerializeField] private Transform IKHead;

    [Space]
    [Tooltip("Displays current offset, doesn't read from this value")]
    public Vector3 headBodyOffset;

    void LateUpdate()
    {
        //Read the offset from the current virtual body offset
        headBodyOffset = EmbodimentManager.Instance.headBodyOffset;
        
        transform.position = IKHead.position + headBodyOffset;
        transform.forward = Vector3.Lerp(transform.forward, Vector3.ProjectOnPlane(IKHead.forward, Vector3.up).normalized, Time.deltaTime * turnSmoothness); ;
        head.Map();
        leftHand.Map();
        rightHand.Map();
    }

    public void forceUpdate()
    {
        LateUpdate();
    }
}