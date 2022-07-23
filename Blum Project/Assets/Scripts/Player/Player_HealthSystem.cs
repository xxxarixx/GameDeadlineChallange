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
    [SerializeField] private float invicibilityTimeAfterHit = 0.3f;
    [SerializeField] private float defaultPlayerKnockBack = 400f;
    private float invicibilityProcess = 0f;
    public delegate void nothingDele();
    public event nothingDele OnInvicibilityEnded;
    public event nothingDele OnInvicibilityStarted;
    public int currentHealth { get; private set; }
    private List<HealthContainer> healthContainers = new List<HealthContainer>();
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
        foreach (Transform child in Main_UiController.instance.healthContainerHolder)
        {
            Destroy(child.gameObject);
        }
        _AddHeartContainers(maxStartHealth);
    }
    void OnEnable()
    {
        OnInvicibilityStarted += Player_HealthSystem_OnInvicibilityStarted; 
        OnInvicibilityEnded += Player_HealthSystem_OnInvicibilityEnded;
    }

    void OnDisable()
    {
        OnInvicibilityStarted -= Player_HealthSystem_OnInvicibilityStarted;
        OnInvicibilityEnded -= Player_HealthSystem_OnInvicibilityEnded;
    }
    private void Player_HealthSystem_OnInvicibilityEnded()
    {
        refer.mainSprend.color = Color.white;
    }

    private void Player_HealthSystem_OnInvicibilityStarted()
    {
        refer.mainSprend.color = Color.yellow;
    }

    void Update()
    {
        if (invicibilityProcess > 0f)
        {
            invicibilityProcess -= Time.deltaTime;
            if (invicibilityProcess < 0f) OnInvicibilityEnded?.Invoke();
        }
    }
    private bool _CanBeHurted()
    {
        return invicibilityProcess <= 0f;
    }
    private void _SetInvicibilityTime(float time)
    {
        if (time > 0f) OnInvicibilityStarted?.Invoke();
        invicibilityProcess = time;
    }
    private void _AddHeartContainers(int HeartContainerCount, bool full = true)
    {
        for (int i = 0; i < HeartContainerCount; i++)
        {
            if (healthContainers.Count >= maxHealth) return;
            var spawnedContainer = Instantiate(refer.fullHealthContainerPrefab, Main_UiController.instance.healthContainerHolder.transform);
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
    public void OnHit(int _damage,Vector3 _invokerPosition, float _knockbackMultiplayer)
    {
        if (!_CanBeHurted()) return;
        currentHealth -= (currentHealth - _damage > 0)? _damage : currentHealth;
        _UpdateHeartContainer(_damage);
        _knockbackPlayer(_invokerPosition, _knockbackMultiplayer);
        _CheckForPlayerDeath();
    }
    private void _knockbackPlayer(Vector3 _invokerPosition, float _knockBackMultiplayer)
    {
        _SetInvicibilityTime(invicibilityTimeAfterHit);
        var knockDir = (refer.flip_Pivolt.position - _invokerPosition).normalized;
        StartCoroutine(_knockBackProcessing(knockDir, _knockBackMultiplayer));
    }
    private IEnumerator _knockBackProcessing(Vector3 knockDirection, float _knockBackMultiplayer)
    {
        refer.movement.SetMoveState(false);
        refer.movement.MoveIndependentOnPlayerInput(defaultPlayerKnockBack * _knockBackMultiplayer, knockDirection, false);
        yield return new WaitForSeconds(.2f);
        refer.movement.SetMoveState(true);

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
        if (!isDead()) return;
        refer.movement.SetMoveState(false);
        Main_UiController.instance.deadScreen.SetActive(true);
        refer.PlayAnimation(Player_References.animations.idle, 10);
        refer.rb.gravityScale = 0;
        refer.collision.enabled = false;
    }
    public bool isDead()
    {
        return currentHealth <= 0f;
    }
}
 