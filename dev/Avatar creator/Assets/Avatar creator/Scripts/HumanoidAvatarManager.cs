using System.Collections;
using System.Collections.Generic;
using UMA;
using UMA.CharacterSystem;
using UnityEngine;
using UMA.CharacterSystem.Examples;
using static UMA.CharacterSystem.DynamicCharacterAvatar;
using System;
using Pixelplacement;
using UniRx;

/// <summary>
/// A facade+adapter that manages reading/writing characteristics for the avatar,
/// it's reflection and it's t-pose clone(which is for measurements)
/// 
/// Triple call for the methods of the same signature from DynamicCharacterAvatar
/// </summary>
public class HumanoidAvatarManager : Singleton<HumanoidAvatarManager>
{
    //Keep in mind that those 3 DCA are not completely equal
    //- they have different poses, components and different slot recepies( No head of virtual body for example)
    public DynamicCharacterAvatar virtualBodyAvatar, reflectionAvatar, tPoseAvatar;
    public Avatar lastSavedAvatar;

    private void Awake()
    {
        //virtualBodyAvatar.CharacterCreated.AddAction((d) => { if(d.animator.avatar)
        //                                                        lastSavedAvatar = d.animator.avatar;
        //                                                        d.animator.avatar = null; });
        //virtualBodyAvatar.CharacterDnaUpdated.AddAction((d) => { d.animator.avatar = null; });
    }

    //not used, just for experiments to fix The Main Bug ( see documentation)
    public void resetLastSavedAvatar()
    {
        virtualBodyAvatar.GetComponent<Animator>().avatar = lastSavedAvatar;
    }

    public void SetActive(bool active)
    {
        virtualBodyAvatar.gameObject.SetActive(active);
        reflectionAvatar.gameObject.SetActive(active);
        tPoseAvatar.gameObject.SetActive(active);
    }

    public void ChangeRace(string raceName)
    {
        virtualBodyAvatar.ChangeRace(raceName);
        reflectionAvatar.ChangeRace(raceName);
        tPoseAvatar.ChangeRace(raceName);
    }

    public void ReapplyWardrobeCollections()
    {
        virtualBodyAvatar.ReapplyWardrobeCollections();
        reflectionAvatar.ReapplyWardrobeCollections();
        tPoseAvatar.ReapplyWardrobeCollections();
    }

    //not working fix for The Main Bug
    #region iTriedACrutchFix
    CompositeDisposable disposables = new CompositeDisposable();
    public void disableAnimatorAvatarAfterRebuild()
    {
        var reflectionUpdatedObservable = reflectionAvatar.CharacterUpdated.AsObservable();
        var bodyatedObservable = virtualBodyAvatar.CharacterUpdated.AsObservable();
        //Skip T-Pose because t-pose need it's animator avatar state

        Observable.CombineLatest(reflectionUpdatedObservable, bodyatedObservable).Take(1).Subscribe(_ =>
         {
             turnOffAnimAvatarForDCA(reflectionAvatar, false);
             turnOffAnimAvatarForDCA(virtualBodyAvatar, false);

             Observable.Timer(TimeSpan.FromSeconds(0.5)).Subscribe(_ =>
             {
                 turnOffAnimAvatarForDCA(reflectionAvatar, true);
                 turnOffAnimAvatarForDCA(virtualBodyAvatar, true);
                 disposables = new CompositeDisposable();
             }).AddTo(disposables);

         }).AddTo(disposables);
    }

    private void turnOffAnimAvatarForDCA(DynamicCharacterAvatar dca, bool enabled)
    {
        reflectionAvatar.GetComponent<Animator>().enabled = enabled;
    }
    #endregion

    //Returns DNA's but in triplets. I am not sure if that is needed, maybe they are equal. I didn't try to figure that out
    //Keep in mind that those 3 DCA are not completely equal - they have different poses and different slot recepies( No head for example)
    public TripledDNABase[] GetAllDNA()
    {

        var a = virtualBodyAvatar.GetAllDNA();
        var b = reflectionAvatar.GetAllDNA();
        var c = tPoseAvatar.GetAllDNA();

        TripledDNABase[] result = new TripledDNABase[a.Length];

        for (int i = 0; i < a.Length; i++)
        {
            result[i] = new TripledDNABase(a[i], b[i], c[i]);
        }
        return result;
    }

    public RaceSetter activeRace { get { return virtualBodyAvatar.activeRace; } }

    public void ForceUpdate(bool DnaDirty, bool TextureDirty = false, bool MeshDirty = false)
    {
        virtualBodyAvatar.ForceUpdate(DnaDirty, TextureDirty, MeshDirty);
        reflectionAvatar.ForceUpdate(DnaDirty, TextureDirty, MeshDirty);
        tPoseAvatar.ForceUpdate(DnaDirty, TextureDirty, MeshDirty);
    }

    public void BuildCharacter(bool RestoreDNA = true, bool skipBundleCheck = false, bool useBundleParameter = true)
    {
        virtualBodyAvatar.BuildCharacter(RestoreDNA, skipBundleCheck, useBundleParameter);
        reflectionAvatar.BuildCharacter(RestoreDNA, skipBundleCheck, useBundleParameter);
        tPoseAvatar.BuildCharacter(RestoreDNA, skipBundleCheck, useBundleParameter);
    }

    public void ClearSlot(string slot)
    {
        virtualBodyAvatar.ClearSlot(slot);
        reflectionAvatar.ClearSlot(slot);
        tPoseAvatar.ClearSlot(slot);
    }

    public void ClearSlot(UMATextRecipe utr)
    {
        ClearSlot(utr.wardrobeSlot);
    }


    public bool SetSlot(UMATextRecipe utr)
    {
        virtualBodyAvatar.SetSlot(utr);
        tPoseAvatar.SetSlot(utr);
        return reflectionAvatar.SetSlot(utr);
    }

        public void SetSlot(string Slotname, string Recipename)
    {
        virtualBodyAvatar.SetSlot(Slotname, Recipename);
        reflectionAvatar.SetSlot(Slotname, Recipename);
        tPoseAvatar.SetSlot(Slotname, Recipename);
    }

    public void SetSlot(string slotName)
    {
        virtualBodyAvatar.SetSlot(slotName);
        reflectionAvatar.SetSlot(slotName);
        tPoseAvatar.SetSlot(slotName);
    }

    public Dictionary<string,UMATextRecipe> GetRecepies()
    {
        //crutch, but reflectionAvatar because it doesn't have NoHead, NoHair slots
        return reflectionAvatar.WardrobeRecipes;
    }

    public void SetColorAlbedo(string sharedColorName, OverlayColorData color)
    {
        virtualBodyAvatar.SetColor(sharedColorName, color);
        reflectionAvatar.SetColor(sharedColorName, color);
        tPoseAvatar.SetColor(sharedColorName, color);
    }

    public OverlayColorData GetColor(string sharedColorName)
    {
        return virtualBodyAvatar.GetColor(sharedColorName);
    }

    public Dictionary<string, List<UMATextRecipe>> AvailableRecipes
    {
        get
        {
            return virtualBodyAvatar.AvailableRecipes;
        }
    }

}

/// <summary>
/// Like a pair, but a triple, and facade for direct interaction with all of them by a single call
/// </summary>
public class TripledDNABase : UMADnaBase
{
    UMADnaBase dna1, dna2, dna3;
    public TripledDNABase(UMADnaBase dna1, UMADnaBase dna2, UMADnaBase dna3)
    {
        this.dna1 = dna1;
        this.dna2 = dna2;
        this.dna3 = dna3;
    }

    public override void SetValue(int idx, float value)
    {
        //HumanoidAvatarManager.Instance.resetLastSavedAvatar();

        dna1.SetValue(idx, value);
        dna2.SetValue(idx, value);
        dna3.SetValue(idx, value);
    }

    public override int Count => dna1.Count;
    public override int DNATypeHash { get => dna1.DNATypeHash; set { dna1.DNATypeHash = value; dna2.DNATypeHash = value;
            dna3.DNATypeHash = value; } }
    public override string[] Names => dna1.Names;
    public override float[] Values { get => dna1.Values; set { dna1.Values = value; dna2.Values = value; dna3.Values = value; } }
}