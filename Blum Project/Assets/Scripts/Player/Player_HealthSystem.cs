using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;
public class Player_HealthSystem : MonoBehaviour,IDamagableByEnemy
{
    public Player_References refer;
    [SerializeField] private int maxStartHealth = 3;
    [SerializeField] private int maxHealth = 20;
    public int currentHealth;
    public List<HealthContainer> healthContainers = new List<HealthContainer>();
    [System.Serializable]
    public class HealthContainer 
    {
        public Image conatinerImg;
        public bool isEmpty = false;
        public HealthContainer(Image conatinerImg, bool isEmpty)
        {
            this.conatinerImg = conatinerImg;
            this.isEmpty = isEmpty;
        }
    }

    private void Start()
    {
        currentHealth = 0;
        foreach (Transform child in refer.healthContainerHolder)
        {
            Destroy(child.gameObject);
        }
        _AddHeartContainers(maxStartHealth);
    }

    void Update()
    {
        if (UnityEngine.InputSystem.Keyboard.current.pKey.wasPressedThisFrame)
        {
            OnHit(1);
        }
        if (UnityEngine.InputSystem.Keyboard.current.kKey.wasPressedThisFrame)
        {
            OnHit(4);
        }
        if (UnityEngine.InputSystem.Keyboard.current.nKey.wasPressedThisFrame)
        {
            _AddHeartContainers(1);
        }
        if (UnityEngine.InputSystem.Keyboard.current.mKey.wasPressedThisFrame)
        {
            _RemoveHeartContainers(1);
        }
        if (UnityEngine.InputSystem.Keyboard.current.oKey.wasPressedThisFrame)
        {
            Heal(1);
        }
    }
    private void _AddHeartContainers(int HeartContainerCount, bool full = true)
    {
        for (int i = 0; i < HeartContainerCount; i++)
        {
            if (healthContainers.Count >= maxHealth) return;
            var spawnedContainer = Instantiate(refer.fullHealthContainerPrefab, refer.healthContainerHolder.transform);
            healthContainers.Add(new HealthContainer(spawnedContainer.GetComponent<Image>(), true));
            _SetContainerState(healthContainers[healthContainers.Count - 1],true);
            if (full) Heal(1);
        }
    }

    private void _RemoveHeartContainers(int HeartContainerCount)
    {
        for (int i = healthContainers.Count - 1; i >= 0; i--)
        {
            if(healthContainers.Count == 0) return;
            if (HeartContainerCount <= 0) break;
            var heartContainer = healthContainers[i];
            Destroy(heartContainer.conatinerImg.gameObject);
            if (!healthContainers[i].isEmpty) currentHealth--;
            HeartContainerCount--;
            healthContainers.RemoveAt(i);
        }
        if (healthContainers.Count == 0) _CheckForPlayerDeath();
    }

    public void OnHit(int _damage)
    {
        currentHealth -= (currentHealth - _damage > 0)? _damage : currentHealth;
        _UpdateHeartContainer(_damage);
        _CheckForPlayerDeath();
    }
    public void Heal(int HeartsCount)
    {
        for (int i = currentHealth; i < currentHealth + HeartsCount; i++)
        {
            if (i > healthContainers.Count - 1) return;
            _SetContainerState(healthContainers[i], false);
        }
        currentHealth += HeartsCount;
    }
    private void _UpdateHeartContainer(int remainingDamageDealt)
    {
        for (int i = healthContainers.Count - 1; i >= 0; i--)
        {
            var heartContainer = healthContainers[i];
            if (heartContainer.isEmpty) continue;
            if (remainingDamageDealt <= 0f) break;
            _SetContainerState(heartContainer, true);
            remainingDamageDealt--;

        }
    }
    private void _SetContainerState(HealthContainer healthContainer,bool Destroyed)
    {
        healthContainer.conatinerImg.sprite = (Destroyed) ?  refer.emptyHealthSprite : refer.fullHealthSprite;
        healthContainer.isEmpty = Destroyed;
    }
    private void _CheckForPlayerDeath()
    {
        if (maxStartHealth > 0) return;
        refer.movement.SetMoveState(false);
        refer.PlayAnimation(Player_References.animations.idle, 10);
    }
}
