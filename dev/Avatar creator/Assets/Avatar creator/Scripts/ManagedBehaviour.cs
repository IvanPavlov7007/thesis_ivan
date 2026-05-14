using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

/// <summary>
/// A generic component that is handy to add at to any object at a runtime and add some information or event-listening
/// </summary>
public class ManagedBehaviour : MonoBehaviour
{
    public Action<ManagedBehaviour> started;

    protected virtual void Start()
    {
        started?.Invoke(this);
    }

    protected virtual void Update()
    {
        
    }
}
