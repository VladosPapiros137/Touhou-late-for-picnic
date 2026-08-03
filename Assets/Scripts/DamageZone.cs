using UnityEngine;

public class DamageZone : MonoBehaviour
{
    [SerializeField] private float _damage = 15f;

    private void OnTriggerStay2D(Collider2D other)
    {
        // Если в зону зашел Игрок
        if (other.TryGetComponent<PlayerStats>(out var playerStats))
        {
            playerStats.TakeDamage(_damage);
        }
        // Если зашел обычный враг
        else if (other.TryGetComponent<Health>(out var health))
        {
            health.TakeDamage(_damage);
        }
    }
}