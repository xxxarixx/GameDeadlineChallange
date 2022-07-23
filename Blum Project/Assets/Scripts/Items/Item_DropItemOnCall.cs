using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Item_DropItemOnCall : MonoBehaviour
{
    public Calls call;
    public int itemID;
    public int count;
    [Range(0f, 1f)] public float dropEverythingInXseconds;
    public enum Calls
    {
        Start,
        Custom
    }

    public void DropIt(bool DestroyAfterDrop)
    {
        Main_GameManager.instance.DropItem(itemID, transform.position, count, dropEverythingInXseconds);
        if (DestroyAfterDrop) Destroy(gameObject);
    }
    private void Start()
    {
        if(call == Calls.Start)
        {
            DropIt(true);
        }
    }
}
