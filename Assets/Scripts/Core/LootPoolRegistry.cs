using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Синглтон-реестр пулов лута. Решает проблему: префабы (Crate, враги) не могут
/// хранить прямую ссылку на GenericObjectPool, лежащий в сцене — эта ссылка
/// "живёт" только на конкретной сцене и не сериализуется в Asset-префаб.
/// Вместо этого префабы хранят просто string id ("Coin_Small" и т.д.),
/// а реальный пул ищется здесь в рантайме.
/// </summary>
public class LootPoolRegistry : MonoBehaviour
{
    [System.Serializable]
    private class PoolEntry
    {
        public string id;
        public GenericObjectPool pool;
    }

    public static LootPoolRegistry Instance { get; private set; }

    [Header("Регистрация пулов (id -> GenericObjectPool)")]
    [SerializeField] private List<PoolEntry> _pools = new List<PoolEntry>();

    private readonly Dictionary<string, GenericObjectPool> _registry = new Dictionary<string, GenericObjectPool>();

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Debug.LogWarning("[LootPoolRegistry] В сцене уже есть другой экземпляр — уничтожаю дубликат.", this);
            Destroy(gameObject);
            return;
        }

        Instance = this;
        BuildRegistry();
    }

    private void OnDestroy()
    {
        if (Instance == this) Instance = null;
    }

    private void BuildRegistry()
    {
        _registry.Clear();

        foreach (var entry in _pools)
        {
            if (string.IsNullOrEmpty(entry.id) || entry.pool == null)
            {
                Debug.LogWarning("[LootPoolRegistry] Пустая запись в списке пулов — проверь настройку в инспекторе.", this);
                continue;
            }

            if (_registry.ContainsKey(entry.id))
            {
                Debug.LogWarning($"[LootPoolRegistry] Дублирующийся id '{entry.id}' — используется первая запись, вторую проверь.", this);
                continue;
            }

            _registry.Add(entry.id, entry.pool);
        }
    }

    public GenericObjectPool GetPool(string id)
    {
        if (_registry.TryGetValue(id, out var pool)) return pool;

        Debug.LogWarning($"[LootPoolRegistry] Пул с id '{id}' не зарегистрирован. Опечатка в id или забыл добавить в реестр?", this);
        return null;
    }
}