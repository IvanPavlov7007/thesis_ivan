using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class CalibrationDisplayer : MonoBehaviour
{
    [SerializeField]
    private TextMeshProUGUI calibrationDataDisplayer;

    private void Start()
    {
        EmbodimentManager.Instance.calibrationTicked += onCalibrationTicked;
        EmbodimentManager.Instance.calibrationEnded += onCalibrationTicked;
    }

    private void onCalibrationTicked(float time, BodyMeasures bodyMeasures)
    {
        calibrationDataDisplayer.text = time.ToString() + System.Environment.NewLine
                                        + "Arm span: " + bodyMeasures.armSpan.ToString() + System.Environment.NewLine
                                        + "Eyes' height: " + bodyMeasures.eyeSightHeight.ToString();
    }
}
