using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
public class Univ_DelayDestroy : MonoBehaviour
{
    public float delay;
    private float _delayProcess;
    public List<GameObject> goSpawnOnEnd = new List<GameObject>();
    public UnityEvent onDestroy;
    private void Start()
    {
        _delayProcess = delay;
    }
    private void Update()
    {
        if(_delayProcess > 0f)
        {
            _delayProcess -= Time.deltaTime;
        }
        else
        {
            //done delay
            onDestroy?.Invoke();
            foreach (var toSpawn in goSpawnOnEnd)
            {
                Instantiate(toSpawn, transform.position, Quaternion.identity);
            }
            Destroy(gameObject);
        }
    }
}
