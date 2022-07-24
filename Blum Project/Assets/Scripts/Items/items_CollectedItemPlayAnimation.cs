using System.Collections;
using System.Collections.Generic;
using UnityEngine;
/// <summary>
/// play animation dependly on what item was picked it work independly without setting anything on item list
/// </summary>
public class items_CollectedItemPlayAnimation : MonoBehaviour
{
    public Main_GameManager gameManager;
    public int itemID;
    public Animator anim;
    public AnimationClip clip;
    private void Start()
    {
        if (gameManager == null) gameManager = GetComponent<Main_GameManager>();
    }
    private void OnEnable()
    {
        gameManager.OnCollectedItem += Instance_OnCollectedItem;
    }
    private void OnDisable()
    {
        gameManager.OnCollectedItem -= Instance_OnCollectedItem;
    }
    private void Instance_OnCollectedItem(Main_GameManager.Item item, Vector3 position)
    {
        if (item.id != itemID) return;
        anim.Play(clip.name);
    }
}
