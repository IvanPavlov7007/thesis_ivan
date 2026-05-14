using UnityEngine;
using UnityEngine.UI;
using UMA.CharacterSystem;
using UMA;
using TMPro;
using System;
using UniRx;


// Ivan: originally script with the same name from UMA, but customized by me

/// <summary>
/// Slider that changes a DNA of humanoids
/// </summary>
public class DNASlider : MonoBehaviour
{
    public string _DNAName;
    int _Index;
    UMADnaBase _Owner;   // different DNA 
    HumanoidAvatarManager _Avatar;
    float _InitialValue;
    public DNARangeAsset _dnr;

    public Slider ValueSlider;
    public TextMeshProUGUI Label;
    public float settingCooldown = 0.7f; // needed so that changes doesn't happen to often

    public event Action<float> onValueChanged;

    // Use this for initialization
    void Start()
    {
        //setting an initial value without triggering any action
        ValueSlider.SetValueWithoutNotify( _InitialValue);
        Label.text = _DNAName;

        // observable (event collector) with a cooldown, for when a value of the slider is changed, starting with the first in the series
        var immediateChange = ValueSlider.OnValueChangedAsObservable().DistinctUntilChanged()
            .ThrottleFirst(TimeSpan.FromSeconds(settingCooldown));

        //observable with a cooldown - starts after cooldown, but ensures that the last change is emmited, even after time.
        var lastChange = ValueSlider.OnValueChangedAsObservable().DistinctUntilChanged()
            .Throttle(TimeSpan.FromSeconds(settingCooldown));

        //merging both observables and subscribing to an action. Distinct filtering for the situations, when both observables sending the same signals
        immediateChange.Merge(lastChange).DistinctUntilChanged()
            .Subscribe(setNewValue).AddTo(this);
    }

    public void SetUp(string name, int index, UMADnaBase owner, HumanoidAvatarManager avatar, float currentval)
    {
        _DNAName = name;
        _Index = index;
        _Owner = owner;
        _Avatar = avatar;
        _InitialValue = currentval;

        //getting a proper range
        DNARangeAsset[] dnaRangeAssets = avatar.activeRace.data.dnaRanges;
        for (int i = 0; i < dnaRangeAssets.Length; i++)
        {
            DNARangeAsset d = dnaRangeAssets[i];
            if (d.ContainsDNARange(_Index, _DNAName))
            {
                _dnr = d;
                break;
            }
        }

    }

    private void setNewValue(float value)
    {
        if (_dnr == null) //No specified DNA Range Asset for this DNA
        {
            _Owner.SetValue(_Index, value);
            _Avatar.ForceUpdate(true, false, false);
            return;
        }

        if (_dnr.ValueInRange(_Index, value))
        {
            _Owner.SetValue(_Index, value);
            _Avatar.ForceUpdate(true, false, false);
            return;
        }
        else
        {
            //Debug.LogWarning ("DNA Value out of range!");
        }

        if (onValueChanged != null)
        {
            onValueChanged(value);
        }
    }
}
