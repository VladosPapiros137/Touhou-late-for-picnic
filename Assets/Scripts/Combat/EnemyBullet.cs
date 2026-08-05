using UnityEngine;

[RequireComponent(typeof(Attacker))]
public class EnemyBullet : MonoBehaviour
{
    [Header("Bullet Settings")]
    [SerializeField] private float _speed = 10f;
    [SerializeField] private float _maxDistance = 15f;

    private static int s_environmentLayer = -1;

    private GenericObjectPool _pool;
    private Vector2 _startPosition;
    private Attacker _attacker;
    private bool _isReturned;

    private void Awake()
    {
        _attacker = GetComponent<Attacker>();

        if (s_environmentLayer == -1)
        {
            s_environmentLayer = LayerMask.NameToLayer("Environment");
        }
    }

    public void Setup(GenericObjectPool pool, Vector2 spawnPosition, Quaternion rotation)
    {
        _pool = pool;
        _startPosition = spawnPosition;
        transform.position = spawnPosition;
        transform.rotation = rotation;
        _isReturned = false;
    }

    private void Update()
    {
        transform.Translate(Vector2.right * _speed * Time.deltaTime);

        if (Vector2.Distance(_startPosition, transform.position) >= _maxDistance)
        {
            ReturnToPool();
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (_isReturned) return;

        // Врезались в Игрока
        if (other.TryGetComponent<PlayerStats>(out var playerStats))
        {
            playerStats.TakeDamage(_attacker.Damage);
            ReturnToPool();
        }
        else if (other.TryGetComponent<Health>(out var health) && other.CompareTag("Player"))
        {
            health.TakeDamage(_attacker.Damage);
            ReturnToPool();
        }
        // Врезались в стены
        else if (other.gameObject.layer == s_environmentLayer)
        {
            ReturnToPool();
        }
    }

    private void ReturnToPool()
    {
        if (_isReturned) return;
        _isReturned = true;

        if (_pool != null)
        {
            _pool.Release(gameObject);
        }
        else
        {
            gameObject.SetActive(false);
        }
    }
}