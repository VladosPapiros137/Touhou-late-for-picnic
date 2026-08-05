using UnityEngine;

public class EnemyAggroDetector : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] private float _aggroRadius = 5f;
    [SerializeField] private LayerMask _playerLayer;

    private Health _health;

    public bool IsPlayerDetected { get; private set; }
    public Transform PlayerTransform { get; private set; }

    private void Awake()
    {
        _health = GetComponent<Health>();
    }

    private void OnEnable()
    {
        IsPlayerDetected = false;
        PlayerTransform = null;

        if (_health != null)
        {
            _health.OnHealthChanged += HandleHealthChanged;
        }
    }

    private void OnDisable()
    {
        if (_health != null)
        {
            _health.OnHealthChanged -= HandleHealthChanged;
        }
    }

    private void Update()
    {
        if (IsPlayerDetected) return;

        // 1. Поиск по слою
        Collider2D playerCollider = Physics2D.OverlapCircle(transform.position, _aggroRadius, _playerLayer);
        if (playerCollider != null)
        {
            SetAggro(playerCollider.transform);
            return;
        }

        // 2. Резервный поиск по тегу
        var allColliders = Physics2D.OverlapCircleAll(transform.position, _aggroRadius);
        foreach (var col in allColliders)
        {
            if (col.CompareTag("Player") || col.GetComponent<PlayerController>() != null)
            {
                SetAggro(col.transform);
                break;
            }
        }
    }

    private void HandleHealthChanged(float currentHealth)
    {
        if (!IsPlayerDetected && _health != null && currentHealth < _health.MaxHealth)
        {
            var player = FindFirstObjectByType<PlayerController>();
            if (player != null)
            {
                SetAggro(player.transform);
            }
        }
    }

    public void SetAggro(Transform playerTarget)
    {
        IsPlayerDetected = true;
        PlayerTransform = playerTarget;
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, _aggroRadius);
    }
}