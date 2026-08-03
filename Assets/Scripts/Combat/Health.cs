using System;
using UnityEngine;

public class Health : MonoBehaviour
{
    [Header("Health Settings")]
    [SerializeField] private float _maxHealth = 100f;

    [Header("Death Behaviour")]
    [Tooltip("Выключить для Игрока и переиспользуемых через пул врагов — они сами решают, что делать при смерти через OnDied.")]
    [SerializeField] private bool _destroyOnDeath = true;

    public float CurrentHealth { get; private set; }
    public float MaxHealth => _maxHealth;
    public bool IsDead { get; private set; }

    public event Action<float> OnHealthChanged; // передаёт CurrentHealth
    public event Action OnDied;

    private void Awake()
    {
        CurrentHealth = _maxHealth;
    }

    public virtual void TakeDamage(float amount)
    {
        if (amount <= 0 || IsDead) return;

        CurrentHealth = Mathf.Max(CurrentHealth - amount, 0f);
        OnHealthChanged?.Invoke(CurrentHealth);

        if (CurrentHealth <= 0f)
        {
            Die();
        }
    }

    public void Heal(float amount)
    {
        if (amount <= 0 || IsDead) return;

        CurrentHealth = Mathf.Min(CurrentHealth + amount, _maxHealth);
        OnHealthChanged?.Invoke(CurrentHealth);
    }

    protected virtual void Die()
    {
        if (IsDead) return;
        IsDead = true;

        OnDied?.Invoke();

        // Никакой завязки на теги/типы — подписчик (EnemyController, PlayerStats и т.д.)
        // сам решает, что делать: вернуть в пул, проиграть анимацию, вызвать game over и т.д.
        if (_destroyOnDeath)
        {
            Destroy(gameObject);
        }
    }

    /// <summary>Вызывать при повторной активации объекта из пула.</summary>
    public void ResetHealth()
    {
        IsDead = false;
        CurrentHealth = _maxHealth;
        OnHealthChanged?.Invoke(CurrentHealth);
    }
}