using UnityEngine.Events;
using UnityEngine;
using Pixelplacement;

//Used for cases, when the state should notify subscribers when it's being entered or exited
public class StateWithEvents : State
{
    public UnityEvent onEntered;
    public UnityEvent onExited;

    protected virtual void OnEnable()
    {
        onEntered?.Invoke();
    }

    protected virtual void OnDisable()
    {
        onExited?.Invoke();
    }
}
