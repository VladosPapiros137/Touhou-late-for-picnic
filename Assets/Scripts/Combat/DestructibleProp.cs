using UnityEngine;

[RequireComponent(typeof(Health), typeof(LootSpawner))]
public class DestructibleProp : MonoBehaviour
{
    private Health _health;
    private LootSpawner _lootSpawner;

    private void Awake()
    {
        _health = GetComponent<Health>();
        _lootSpawner = GetComponent<LootSpawner>();
    }

    private void OnEnable()
    {
        _health.OnDied += HandleDeath;
    }

    private void OnDisable()
    {
        _health.OnDied -= HandleDeath;
    }

    private void HandleDeath()
    {
        // Когда Health сообщает о смерти, мы заставляем LootSpawner выкинуть лут
        if (_lootSpawner != null)
        {
            _lootSpawner.ProcessDrop();
        }
    }
}