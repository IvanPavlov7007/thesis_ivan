using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//currently not used
//Another crutch-fix for The Main Bug( see documentation)
public class RiggingFix : MonoBehaviour
{
    public Transform[] transformsToRestore;
    Vector3[] positions;
    Quaternion[] rotations;
    Vector3[] scales;

    public void StoreTransforms()
    {
        positions = new Vector3[transformsToRestore.Length];
        rotations = new Quaternion[transformsToRestore.Length];
        scales = new Vector3[transformsToRestore.Length];

        for(int i = 0; i < transformsToRestore.Length; i++)
        {
            positions[i] = transform.position;
            rotations[i] = transform.rotation;
            scales[i] = transform.localScale;
        }
    }

    public void RestoreTransform()
    {
        for (int i = 0; i < transformsToRestore.Length; i++)
        {
            transformsToRestore[i].position = positions[i];
            transformsToRestore[i].rotation = rotations[i];
            transformsToRestore[i].localScale = scales[i];
        }
    }
}
