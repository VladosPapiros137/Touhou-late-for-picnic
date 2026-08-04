using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Монеты: один "ролл" шанса, дающий N монет, где номинал каждой монеты
/// выбирается взвешенным рандомом (медь/серебро/золото).
/// Опыт: точечные записи без ротации типов — сколько и какого опыта
/// может уронить конкретный враг, задаётся явно в инспекторе.
/// </summary>
public class LootSpawner : MonoBehaviour
{
    [System.Serializable]
    public class WeightedCoinEntry
    {
        [Tooltip("id пула в LootPoolRegistry, например 'Coin_Small'")]
        public string poolId;
        [Min(0.01f)] public float weight = 1f;
    }

    [System.Serializable]
    public class CoinDropConfig
    {
        [Range(0f, 1f)] public float dropChance = 0.5f;
        public int minCount = 1;
        public int maxCount = 3;
        public List<WeightedCoinEntry> denominations = new List<WeightedCoinEntry>();
    }

    [System.Serializable]
    public class ExpDropEntry
    {
        [Tooltip("id пула в LootPoolRegistry, например 'Exp_Medium'")]
        public string poolId;
        [Range(0f, 1f)] public float dropChance = 1f;
        public int minAmount = 1;
        public int maxAmount = 1;
    }

    [Header("Coin Drops (взвешенный случайный выбор номинала)")]
    [SerializeField] private CoinDropConfig _coinDrop = new CoinDropConfig();

    [Header("Exp Drops (точечно, без ротации типов)")]
    [SerializeField] private List<ExpDropEntry> _expDrops = new List<ExpDropEntry>();

    [Header("Scatter Settings")]
    [SerializeField] private float _scatterRadius = 0.5f;

    private Health _health;

    private void Awake()
    {
        _health = GetComponent<Health>();
    }

    private void OnEnable()
    {
        if (_health != null) _health.OnDied += ProcessDrop;
    }

    private void OnDisable()
    {
        if (_health != null) _health.OnDied -= ProcessDrop;
    }

    private void ProcessDrop()
    {
        ProcessCoinDrop();
        ProcessExpDrops();
    }

    private void ProcessCoinDrop()
    {
        if (_coinDrop.denominations.Count == 0) return;
        if (Random.value > _coinDrop.dropChance) return;

        int count = Random.Range(_coinDrop.minCount, _coinDrop.maxCount + 1);
        for (int i = 0; i < count; i++)
        {
            string poolId = PickWeightedDenomination(_coinDrop.denominations);
            SpawnItem(poolId);
        }
    }

    private string PickWeightedDenomination(List<WeightedCoinEntry> entries)
    {
        float totalWeight = 0f;
        foreach (var entry in entries) totalWeight += entry.weight;

        float roll = Random.value * totalWeight;
        float cumulative = 0f;

        foreach (var entry in entries)
        {
            cumulative += entry.weight;
            if (roll <= cumulative) return entry.poolId;
        }

        return entries[entries.Count - 1].poolId; // подстраховка от погрешности float
    }

    private void ProcessExpDrops()
    {
        foreach (var drop in _expDrops)
        {
            if (string.IsNullOrEmpty(drop.poolId)) continue;
            if (Random.value > drop.dropChance) continue;

            int count = Random.Range(drop.minAmount, drop.maxAmount + 1);
            for (int i = 0; i < count; i++)
            {
                SpawnItem(drop.poolId);
            }
        }
    }

    private void SpawnItem(string poolId)
    {
        if (string.IsNullOrEmpty(poolId)) return;

        GenericObjectPool pool = LootPoolRegistry.Instance != null
            ? LootPoolRegistry.Instance.GetPool(poolId)
            : null;

        if (pool == null) return; // предупреждение уже выведет сам реестр

        GameObject spawnedItem = pool.Get();
        if (spawnedItem == null) return;

        Vector2 randomOffset = Random.insideUnitCircle * _scatterRadius;
        Vector2 spawnPosition = (Vector2)transform.position + randomOffset;

        if (spawnedItem.TryGetComponent<PickableItem>(out var pickable))
        {
            pickable.Setup(pool, spawnPosition);
        }
        else
        {
            spawnedItem.transform.position = spawnPosition;
        }
    }
}