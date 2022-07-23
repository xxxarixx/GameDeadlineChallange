using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player_PickupItem : MonoBehaviour
{
    public Player_References refer;
    private void OnDrawGizmosSelected()
    {
        if(refer == null)
        {
            if(TryGetComponent(out Player_References references))
            {
                refer = references;
            }
        }
    }
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("DropItem"))
        {
            var dropItemPrefab = collision.GetComponent<Items_DropItemPrefab>();
            Main_GameManager.instance.AddItemToInventory(dropItemPrefab.itemID);
            Destroy(dropItemPrefab.gameObject);
        }
    }
}
