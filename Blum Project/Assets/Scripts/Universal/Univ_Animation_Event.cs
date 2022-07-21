using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
public class Univ_Animation_Event : MonoBehaviour
{
    public List<UnityEvent> events = new List<UnityEvent>();  
    public void PlayEvent(int _eventID)
    {
        events[_eventID].Invoke();
    }
}
