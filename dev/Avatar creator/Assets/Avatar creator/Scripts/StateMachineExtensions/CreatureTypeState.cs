using UnityEngine.Events;
using UnityEngine;
using Pixelplacement;

[ExecuteAlways]
public class CreatureTypeState : State
{
    public GameObject linkedBody = null;
    public UnityEvent onEntered;
    public UnityEvent onExited;

    private void OnEnable()
    {
        if(linkedBody != null)
            linkedBody.SetActive(true);
        onEntered?.Invoke();
    }

    private void OnDisable()
    {
        if (linkedBody != null)
            linkedBody.SetActive(false);
        onExited?.Invoke();
    }
}
