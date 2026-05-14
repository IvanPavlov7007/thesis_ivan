using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Pixelplacement;

[System.Serializable]
public struct BodyMeasures
{
    public float eyeSightHeight;
    public float armSpan;

    public static BodyMeasures operator /(BodyMeasures a, float value)
        => new BodyMeasures { eyeSightHeight = a.eyeSightHeight / value, armSpan = a.armSpan / value };
}

/// <summary>
/// Makes corrections for the controls so that the virtual body could be controlled and experienced, determined by it's characteristics
/// Also manages calibration
/// </summary>
public class EmbodimentManager : Singleton<EmbodimentManager>
{
    //TODO: Move the common data from CustomVRControllers here, such as mapping targets for IK-targets, i e headset and controllers transforms

    
    public Transform XR_offset; //actual direct controls
    public LocalPositionMagnifier leftHand, rightHand; //controls for hands
    public ChestCenterLocator sternum; // to read position of the sternum

    public TPoseMeasurementTracker measurementTracker; //measures of the T-Pose of the virtual body
    public TPoseMeasurementTracker userBodyTracker; //measures actual T-Pose of the user

    public float TPoseCalibrationTime = 3f;

    public Vector3 headBodyOffset { get; private set; }

    public BodyMeasures realMeasures = new BodyMeasures
    {
        //default values
        eyeSightHeight = 169f, // 10 cm lower than the height
        armSpan = 180f// 4 cm smaller than the actual
    };

    public BodyMeasures virtualMeasures;

    private float currentScale = 1f;

    public void recalculate()
    {
        currentScale = virtualMeasures.eyeSightHeight / realMeasures.eyeSightHeight;
        XR_offset.localScale = new Vector3(currentScale, currentScale, currentScale);

        headBodyOffset = new Vector3(0f, -virtualMeasures.eyeSightHeight, 0f);

        float userArmSpanScaledToVirtualBody = realMeasures.armSpan * currentScale;
        float armSpanMagnifier = virtualMeasures.armSpan / userArmSpanScaledToVirtualBody;
        leftHand.magnifier = armSpanMagnifier;
        rightHand.magnifier = armSpanMagnifier;
    }

    #region calibration

    Coroutine calibrationRoutine;
    public void startCalibration()
    {
        if (calibrationRoutine != null)
            StopCoroutine(calibrationRoutine);
        calibrationRoutine = StartCoroutine(calibration(TPoseCalibrationTime, calibrationTicked, calibrationEnded));
    }

    public event System.Action<float, BodyMeasures> calibrationTicked;
    public event System.Action<float, BodyMeasures> calibrationEnded;

    IEnumerator calibration(float time, System.Action<float, BodyMeasures> callback_timerTick = null,
        System.Action<float, BodyMeasures> callback_timerEnd = null)
    {
        BodyMeasures maxMeasures = new BodyMeasures { armSpan = 0f, eyeSightHeight = 0f };
        float timer = 0f;

        while (timer < time)
        {
            BodyMeasures currentMeasures = measureRealBody();
            if (currentMeasures.armSpan > maxMeasures.armSpan)
                maxMeasures.armSpan = currentMeasures.armSpan;
            if (currentMeasures.eyeSightHeight > maxMeasures.eyeSightHeight)
                maxMeasures.eyeSightHeight =currentMeasures.eyeSightHeight;

            callback_timerTick?.Invoke(timer, maxMeasures);
            yield return new WaitForEndOfFrame();
            timer += Time.deltaTime;
        }

        realMeasures = maxMeasures;
        calibrationRoutine = null;
        callback_timerEnd?.Invoke(time, realMeasures);
        yield return null;
    }

    private BodyMeasures measureRealBody()
    {
        return userBodyTracker.currentMeasures / currentScale;
    }

    #endregion
    private void Update()
    {
        virtualMeasures = measurementTracker.currentMeasures;
        recalculate();
    }
}
