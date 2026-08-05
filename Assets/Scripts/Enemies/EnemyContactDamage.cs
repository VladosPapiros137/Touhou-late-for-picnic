using UnityEngine;

[RequireComponent(typeof(Attacker))]
public class EnemyContactDamage : MonoBehaviour
{
    [SerializeField] private float _damageInterval = 0.5f; // Интервал между укусами/ударами

    private Attacker _attacker;
    private PlayerStats _targetPlayerStats;
    private Health _targetHealth;
    private float _nextDamageTime;

    private void Awake()
    {
        _attacker = GetComponent<Attacker>();
    }

    private void Update()
    {
        if (Time.time < _nextDamageTime) return;

        if (_targetPlayerStats != null)
        {
            _targetPlayerStats.TakeDamage(_attacker.Damage); // Передаем полный урон атаки
            _nextDamageTime = Time.time + _damageInterval;
        }
        else if (_targetHealth != null)
        {
            _targetHealth.TakeDamage(_attacker.Damage);
            _nextDamageTime = Time.time + _damageInterval;
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        CheckTarget(collision.gameObject);
    }

    private void OnCollisionStay2D(Collision2D collision)
    {
        // Подстраховка: если уткаемся в игрока, проверяем цель повторно
        if (_targetPlayerStats == null && _targetHealth == null)
        {
            CheckTarget(collision.gameObject);
        }
    }

    private void OnCollisionExit2D(Collision2D collision)
    {
        if (collision.gameObject.GetComponent<PlayerStats>() == _targetPlayerStats)
        {
            _targetPlayerStats = null;
        }

        if (collision.gameObject.GetComponent<Health>() == _targetHealth)
        {
            _targetHealth = null;
        }
    }

    private void CheckTarget(GameObject obj)
    {
        if (obj.TryGetComponent<PlayerStats>(out var playerStats))
        {
            _targetPlayerStats = playerStats;
        }
        else if (obj.CompareTag("Player") && obj.TryGetComponent<Health>(out var health))
        {
            _targetHealth = health;
        }
    }

    private void OnDisable()
    {
        _targetPlayerStats = null;
        _targetHealth = null;
    }
}