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
    private List<HealthContainer> _healthContainers = new List<HealthContainer>();
    [System.Serializable]
    public class HealthContainer 
    {
        public Image conatinerImg;
        public bool isEmpty = false;
        public HealthContainer(Image _conatinerImg, bool _isEmpty)
        {
            this.conatinerImg = _conatinerImg;
            this.isEmpty = _isEmpty;
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
    void Update()
    {
        if (isDead()) refer.rb.velocity = Vector2.zero;
        if (invicibilityProcess > 0f)
        {
            invicibilityProcess -= Time.deltaTime;
            if (invicibilityProcess < 0f) OnInvicibilityEnded?.Invoke();
        }
    }
    private void OnEnable()
    {
        OnInvicibilityStarted += Player_HealthSystem_OnInvicibilityStarted; 
        OnInvicibilityEnded += Player_HealthSystem_OnInvicibilityEnded;
    }
    private void OnDisable()
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
    private bool _CanBeHurted()
    {
        return invicibilityProcess <= 0f;
    }
    private void _SetInvicibilityTime(float _time)
    {
        if (_time > 0f) OnInvicibilityStarted?.Invoke();
        invicibilityProcess = _time;
    }
    private void _knockbackPlayer(Vector3 _invokerPosition, float _knockBackMultiplayer)
    {
        _SetInvicibilityTime(invicibilityTimeAfterHit);
        var knockDir = (refer.flip_Pivolt.position - _invokerPosition).normalized;
        StartCoroutine(_knockBackProcessing(knockDir, _knockBackMultiplayer));
    }
    private IEnumerator _knockBackProcessing(Vector3 _knockDirection, float _knockBackMultiplayer)
    {
        refer.movement.SetMoveState(false);
        refer.movement.MoveIndependentOnPlayerInput(defaultPlayerKnockBack * _knockBackMultiplayer, _knockDirection, false);
        yield return new WaitForSeconds(.2f);
        refer.movement.SetMoveState(true);

    }
    private void _UpdateHeartContainer(int _remainingDamageDealt)
    {
        for (int i = _healthContainers.Count - 1; i >= 0; i--)
        {
            var heartContainer = _healthContainers[i];
            if (heartContainer.isEmpty) continue;
            if (_remainingDamageDealt <= 0f) break;
            _SetContainerState(heartContainer, true);
            _remainingDamageDealt--;

        }
    }
    private void _SetContainerState(HealthContainer _healthContainer,bool _destroyed)
    {
        _healthContainer.conatinerImg.sprite = (_destroyed) ?  refer.emptyHealthSprite : refer.fullHealthSprite;
        _healthContainer.isEmpty = _destroyed;
    }
    private void _CheckForPlayerDeath()
    {
        if (!isDead()) return;
        refer.movement.SetMoveState(false);
        Main_UiController.instance.deadScreen.SetActive(true);
        refer.rb.velocity = new Vector2(0f, 0f);
        refer.PlayAnimation(Player_References.animations.idle, 10);
        refer.rb.gravityScale = 0;
        refer.collision.enabled = false;
    }

    public void _RemoveHeartContainers(int _heartContainerCount)
    {
        for (int i = _healthContainers.Count - 1; i >= 0; i--)
        {
            if(_healthContainers.Count == 0) return;
            if (_heartContainerCount <= 0) break;
            var heartContainer = _healthContainers[i];
            Destroy(heartContainer.conatinerImg.gameObject);
            if (!_healthContainers[i].isEmpty) currentHealth--;
            _heartContainerCount--;
            _healthContainers.RemoveAt(i);
        }
        if (_healthContainers.Count == 0) _CheckForPlayerDeath();
    }
    public void OnHit(int _damage,Vector3 _invokerPosition, float _knockbackMultiplayer)
    {
        if (!_CanBeHurted()) return;
        currentHealth -= (currentHealth - _damage > 0)? _damage : currentHealth;
        _UpdateHeartContainer(_damage);
        _knockbackPlayer(_invokerPosition, _knockbackMultiplayer);
        _CheckForPlayerDeath();
    }
    public void Hit(int _damage)
    {
        OnHit(_damage, refer.flip_Pivolt.position, 1f);
    }
    public void Heal(int _heartsCount)
    {
        for (int i = currentHealth; i < currentHealth + _heartsCount; i++)
        {
            if (i > _healthContainers.Count - 1) return;
            _SetContainerState(_healthContainers[i], false);
        }
        currentHealth += _heartsCount;
    }
    public void _AddHeartContainers(int _heartContainerCount, bool _full = true)
    {
        for (int i = 0; i < _heartContainerCount; i++)
        {
            //if there is too much containers then just ignore
            if (_healthContainers.Count >= maxHealth) return;
            var spawnedContainer = Instantiate(refer.fullHealthContainerPrefab, Main_UiController.instance.healthContainerHolder.transform);
            _healthContainers.Add(new HealthContainer(spawnedContainer.GetComponent<Image>(), true));
            //add empty container
            _SetContainerState(_healthContainers[_healthContainers.Count - 1],true);
            if (_full) Heal(1);
        }
    }
    //this version of add heart containers mainly used for build in unity event (f.ex button)
    public void _AddHeartContainers(int _heartContainerCount)
    {
        for (int i = 0; i < _heartContainerCount; i++)
        {
            //if there is too much containers then just ignore
            if (_healthContainers.Count >= maxHealth) return;
            var spawnedContainer = Instantiate(refer.fullHealthContainerPrefab, Main_UiController.instance.healthContainerHolder.transform);
            _healthContainers.Add(new HealthContainer(spawnedContainer.GetComponent<Image>(), true));
            //add empty container
            _SetContainerState(_healthContainers[_healthContainers.Count - 1], true);
             Heal(1);
        }
    }
    public bool isDead()
    {
        return currentHealth <= 0f;
    }
}
 