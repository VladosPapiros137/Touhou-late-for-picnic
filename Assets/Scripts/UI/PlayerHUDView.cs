using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Отображает полоски HP и Shield Игрока через UI Image (Fill).
/// Работает на событиях — 0% нагрузки в Update.
/// </summary>
public class PlayerHUDView : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Health _playerHealth;
    [SerializeField] private PlayerStats _playerStats;

    [Header("UI Elements")]
    [SerializeField] private Image _healthFillImage;
    [SerializeField] private Image _shieldFillImage;

    private void OnEnable()
    {
        if (_playerHealth != null)
            _playerHealth.OnHealthChanged += HandleHealthChanged;

        if (_playerStats != null)
            _playerStats.OnShieldChanged += HandleShieldChanged;

        RefreshAll();
    }

    private void OnDisable()
    {
        if (_playerHealth != null)
            _playerHealth.OnHealthChanged -= HandleHealthChanged;

        if (_playerStats != null)
            _playerStats.OnShieldChanged -= HandleShieldChanged;
    }

    private void RefreshAll()
    {
        if (_playerHealth != null) HandleHealthChanged(_playerHealth.CurrentHealth);
        if (_playerStats != null) HandleShieldChanged(_playerStats.CurrentShield);
    }

    private void HandleHealthChanged(float currentHealth)
    {
        if (_healthFillImage == null || _playerHealth == null) return;
        _healthFillImage.fillAmount = currentHealth / _playerHealth.MaxHealth;
    }

    private void HandleShieldChanged(float currentShield)
    {
        if (_shieldFillImage == null || _playerStats == null) return;
        _shieldFillImage.fillAmount = currentShield / _playerStats.MaxShield;
    }
}