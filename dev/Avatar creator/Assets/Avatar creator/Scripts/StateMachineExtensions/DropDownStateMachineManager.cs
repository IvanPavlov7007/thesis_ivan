using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using Pixelplacement;
using TMPro;
using System.Linq;

/// <summary>
/// A class that automatically updates dropdown after changes done for the state machine
/// </summary>
[ExecuteAlways]
public class DropDownStateMachineManager : MonoBehaviour
{
    [SerializeField]
    private StateMachine stateMachine;
    [SerializeField]
    private TMP_Dropdown dropdown;
    [Tooltip("whenever the default state of the state machine should be automatically picked by dropdown")]
    [SerializeField]
    bool stateMachineDefinesDropDownState = true;

    private void Start()
    {
        if(stateMachineDefinesDropDownState)
            setCurrentStateDropdownDisplayValue();
    }

    private void OnValidate()
    {
        if (dropdown != null && stateMachine != null)
        {
            connectToStateMachine();
            if (!isDropDownValid())
            {
                updateDropdownOptions();
            }
        }
        else
            Debug.LogWarning("Both references for dropdown and stateMachine must be set to synchronize them");

    }

    private void connectToStateMachine()
    {
        stateMachine.OnStateEntered.RemoveListener(onStateEntered);
        stateMachine.OnStateEntered.AddListener(onStateEntered);
    }

    public void onStateEntered(GameObject state)
    {
        if (dropdown == null)
            return;

        // Only update the visual choice, don't trigger the state change
        int newIndex = state.transform.GetSiblingIndex();
        if (dropdown.value != newIndex)
            dropdown.SetValueWithoutNotify(newIndex);
    }

    private bool isDropDownValid()
    {
        return dropdown.options.ConvertAll(x => x.text).SequenceEqual(getStates());
    }

    private void updateDropdownOptions()
    {
        
        dropdown.ClearOptions();
        dropdown.AddOptions(getStates());
#if UNITY_EDITOR
        //to actually make changes in the editor:
        EditorUtility.SetDirty(this);
#endif
        dropdown.onValueChanged.RemoveAllListeners();
        dropdown.onValueChanged.AddListener(SetState);
    }

    private void setCurrentStateDropdownDisplayValue()
    {
        if(stateMachine.defaultState != null)
            dropdown.SetValueWithoutNotify(stateMachine.defaultState.transform.GetSiblingIndex());
    }

    private List<string> getStates()
    {
        List<string> options = new List<string>();

        for (int i = 0; i < stateMachine.transform.childCount; i++)
        {
            options.Add(stateMachine.transform.GetChild(i).name);
        }

        return options;
    }

    public void SetState(int state)
    {
        stateMachine.ChangeState(state);
    }
}
