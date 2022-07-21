using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Univ_ActiveAfterDelay : MonoBehaviour
{
    public float delay;
    public List<GameObject> toActive = new List<GameObject>();

    private void Start()
    {
        StartCoroutine(WaitAndActive());
    }
    private IEnumerator WaitAndActive()
    {
        yield return new WaitForSeconds(delay);
        foreach (var active in toActive)
        {
            active.SetActive(true);
        }
        yield return null;
    }
}
