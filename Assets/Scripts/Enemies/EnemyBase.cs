using UnityEngine;

[RequireComponent(typeof(Health))]
public class EnemyBase : MonoBehaviour
{
    private Health _health;
    private LootSpawner _lootSpawner;
    private GenericObjectPool _ownPool;

    public Health Health => _health;
    public LootSpawner LootSpawner => _lootSpawner;

    private void Awake()
    {
        _health = GetComponent<Health>();
        _lootSpawner = GetComponent<LootSpawner>(); // Может быть null, если у врага нет лута
    }

    private void OnEnable()
    {
        if (_health != null)
        {
            _health.OnDied += HandleDied;
        }
    }

    private void OnDisable()
    {
        if (_health != null)
        {
            _health.OnDied -= HandleDied;
        }
    }

    /// <summary>
    /// Вызывается спавнером/пулом при инициализации врага на сцене.
    /// </summary>
    public void Setup(GenericObjectPool ownPool, Vector2 spawnPosition)
    {
        _ownPool = ownPool;
        transform.position = spawnPosition;

        if (_health != null)
        {
            _health.ResetHealth();
        }
    }

    private void HandleDied()
    {
        // 1. Спавним лут перед исчезновением
        if (_lootSpawner != null)
        {
            _lootSpawner.ProcessDrop();
        }

        // 2. Возвращаем врага в его пул
        if (_ownPool != null)
        {
            _ownPool.Release(gameObject);
        }
        else
        {
            gameObject.SetActive(false);
        }
    }
}