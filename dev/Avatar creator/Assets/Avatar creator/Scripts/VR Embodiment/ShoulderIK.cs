using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//Not used class, intended for expanding Rigging to shoulder,
//doesn't work because of Animator constraints or something else

public class ShoulderIK : MonoBehaviour
{
    public Transform tip;
    public Transform IK_Hand_target;
    public Transform IK_Shoulder_target;
    public Transform shoulder;
    public float toleranceDist;

    private void Update()
    {
        Vector3 delta = IK_Hand_target.position - tip.position;
        //TODO Rotate the shoulder towards and stretch, not translate
        if (delta.magnitude > toleranceDist)
            IK_Shoulder_target.position += delta.normalized * toleranceDist;
        else
            IK_Shoulder_target.position = shoulder.position;
    }
}
