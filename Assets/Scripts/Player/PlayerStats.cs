using System;
using System.Collections;
using UnityEngine;

[RequireComponent(typeof(Health))]
public class PlayerStats : MonoBehaviour
{
    [Header("Shield Settings")]
    [SerializeField] private float _maxShield = 50f;
    [SerializeField] private float _shieldRegenRate = 10f;
    [SerializeField] private float _shieldRegenDelay = 3.0f;

    [Header("Invulnerability Settings")]
    [SerializeField] private float _iFrameDuration = 0.5f;

    public float CurrentShield { get; private set; }
    public float MaxShield => _maxShield;

    public bool IsHitInvulnerable { get; private set; }
    public bool IsDashInvulnerable => gameObject.layer == LayerMask.NameToLayer("Player_Invincible");

    // Для UI-полоски щита — без Update-опроса
    public event Action<float> OnShieldChanged; // передаёт CurrentShield
    public event Action OnPlayerDied;
    public event Action<bool> OnHitInvulnerabilityChanged; // для мигания спрайта во время i-frames

    private Health _health;
    private float _lastDamageTime;
    private Coroutine _iFrameCoroutine;

    private void Awake()
    {
        _health = GetComponent<Health>();
        CurrentShield = _maxShield;
    }

    private void OnEnable()
    {
        _health.OnDied += HandleDeath;
    }

    private void OnDisable()
    {
        _health.OnDied -= HandleDeath;
    }

    private void Update()
    {
        HandleShieldRegen();
    }

    public void TakeDamage(float damage)
    {
        if (IsDashInvulnerable || IsHitInvulnerable || _health.IsDead) return;

        _lastDamageTime = Time.time;

        if (CurrentShield > 0)
        {
            float absorbed = Mathf.Min(CurrentShield, damage);
            CurrentShield -= absorbed;
            damage -= absorbed;
            OnShieldChanged?.Invoke(CurrentShield);
        }

        if (damage > 0)
        {
            _health.TakeDamage(damage);
        }

        if (!_health.IsDead)
        {
            if (_iFrameCoroutine != null) StopCoroutine(_iFrameCoroutine);
            _iFrameCoroutine = StartCoroutine(StartIFrames());
        }
    }

    private void HandleShieldRegen()
    {
        if (Time.time - _lastDamageTime < _shieldRegenDelay || CurrentShield >= _maxShield) return;

        CurrentShield = Mathf.Min(CurrentShield + _shieldRegenRate * Time.deltaTime, _maxShield);
        OnShieldChanged?.Invoke(CurrentShield);
    }

    private void HandleDeath()
    {
        IsHitInvulnerable = true; // больше не тикаем i-frames после смерти
        if (_iFrameCoroutine != null) StopCoroutine(_iFrameCoroutine);
        OnPlayerDied?.Invoke();
    }

    private IEnumerator StartIFrames()
    {
        IsHitInvulnerable = true;
        OnHitInvulnerabilityChanged?.Invoke(true);

        yield return new WaitForSeconds(_iFrameDuration);

        IsHitInvulnerable = false;
        OnHitInvulnerabilityChanged?.Invoke(false);
    }
}