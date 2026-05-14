using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//Not used, doesn't fix anything
//A crutch to connect crutches, that don't crutch at all
public class AnimatorAvatarFix : MonoBehaviour
{
    [SerializeField]
    HumanoidAvatarCreator HumanoidAvatarCreator;
    [SerializeField]
    HumanoidAvatarManager HumanoidAvatarManager;
    private void Start()
    {
        HumanoidAvatarCreator.onGenderChanged += _ => HumanoidAvatarManager.disableAnimatorAvatarAfterRebuild();
    }


}
