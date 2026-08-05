using System.Collections.Generic;
using UnityEngine;

public class LootSpawner : MonoBehaviour
{
    [System.Serializable]
    public class WeightedLootEntry
    {
        [Tooltip("ID пула в LootPoolRegistry, например 'Coin_Small', 'Coin_Large'")]
        public string poolId;
        [Min(0.01f)] public float weight = 1f;
    }

    [System.Serializable]
    public class WeightedDropConfig
    {
        [Tooltip("Общий шанс на то, что заспавнится группа предметов с рандомизацией типа")]
        [Range(0f, 1f)] public float dropChance = 0.5f;
        public int minCount = 1;
        public int maxCount = 3;
        [Tooltip("Список предметов, из которых выбирается один по весу для каждой единицы дропа")]
        public List<WeightedLootEntry> entries = new List<WeightedLootEntry>();
    }

    [System.Serializable]
    public class DirectDropEntry
    {
        [Tooltip("ID пула в LootPoolRegistry, например 'Exp_Medium', 'HealthPotion'")]
        public string poolId;
        [Range(0f, 1f)] public float dropChance = 1f;
        public int minAmount = 1;
        public int maxAmount = 1;
    }

    [Header("Weighted Drops (Один ролл количества, тип выбирается по весу: монеты и т.д.)")]
    [SerializeField] private WeightedDropConfig _weightedDrop = new WeightedDropConfig();

    [Header("Direct Drops (Точечные предметы со своим шансом: опыт, зелья и т.д.)")]
    [SerializeField] private List<DirectDropEntry> _directDrops = new List<DirectDropEntry>();

    [Header("Scatter Settings")]
    [SerializeField] private float _scatterRadius = 0.5f;

    // ВАЖНО: Мы убрали OnEnable/OnDisable с автоподпиской на Health.OnDied.
    // Теперь ProcessDrop() вызывается явно из EnemyBase при смерти или из Health коробки.

    /// <summary>
    /// Вызывает расчет и спавн всего лута. Должен вызываться контроллером при смерти объекта.
    /// </summary>
    public void ProcessDrop()
    {
        ProcessWeightedDrop();
        ProcessDirectDrops();
    }

    private void ProcessWeightedDrop()
    {
        if (_weightedDrop.entries.Count == 0) return;
        if (Random.value > _weightedDrop.dropChance) return;

        int count = Random.Range(_weightedDrop.minCount, _weightedDrop.maxCount + 1);
        for (int i = 0; i < count; i++)
        {
            string poolId = PickWeightedEntry(_weightedDrop.entries);
            SpawnItem(poolId);
        }
    }

    private string PickWeightedEntry(List<WeightedLootEntry> entries)
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

        return entries[entries.Count - 1].poolId; // Подстраховка
    }

    private void ProcessDirectDrops()
    {
        foreach (var drop in _directDrops)
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

        if (pool == null) return;

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