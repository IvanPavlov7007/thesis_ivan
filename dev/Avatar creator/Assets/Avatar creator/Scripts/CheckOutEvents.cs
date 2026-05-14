using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//Debug for testing UMA's DCA events
public class CheckOutEvents : MonoBehaviour
{
    public static int order = 0;
    public void EventTriggeredDebug(string message)
    {
        Debug.Log(message + " Order: " + order++.ToString());
    }
}
