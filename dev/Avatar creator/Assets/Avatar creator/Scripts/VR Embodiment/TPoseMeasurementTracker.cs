using System.Collections;
using System.Collections.Generic;
using UnityEngine;


/// <summary>
/// Measures T-Pose each frame
/// </summary>
public class TPoseMeasurementTracker : MonoBehaviour
{
    public Transform rightHand, leftHand, eye, root;

    public BodyMeasures currentMeasures;

    private void Update()
    {
        currentMeasures.armSpan = (rightHand.position - leftHand.position).magnitude;
        currentMeasures.eyeSightHeight = (eye.position - root.position).y;
    }
}
