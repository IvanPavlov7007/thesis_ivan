using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

//currently out of order

/// <summary>
/// Utilities for faster set up of the scene for the dev
/// </summary>
public class DevUtilities : MonoBehaviour
{
    public HumanoidAvatarCreator avatarCreator;
    public int initialCreatureType = 1;
    public HumanoidAvatarCreator.Gender initialGender;
    void Start()
    {
        //avatarCreator.CreatureTypeDropDown.GetComponentInChildren<TMP_Dropdown>().value = initialCreatureType;
        //avatarCreator.GenderDropDown.GetComponentInChildren<TMP_Dropdown>().value = (int)initialGender;
    }
}
