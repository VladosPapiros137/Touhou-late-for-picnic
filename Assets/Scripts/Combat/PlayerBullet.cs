using UnityEngine;

[RequireComponent(typeof(Attacker))]
public class PlayerBullet : MonoBehaviour
{
    [Header("Bullet Settings")]
    [SerializeField] private float _speed = 20f;
    [SerializeField] private float _maxDistance = 12f;

    private static int s_environmentLayer = -1; // -1 = ещё не закешировано

    private GenericObjectPool _pool;
    private Vector2 _startPosition;
    private Attacker _attacker;
    private bool _isReturned; // защита от двойного Release() в один кадр

    private void Awake()
    {
        _attacker = GetComponent<Attacker>();

        // LayerMask.NameToLayer нельзя вызывать в инициализаторе static-поля —
        // движок ещё не готов на этом этапе. Кешируем лениво здесь, один раз на весь домен.
        if (s_environmentLayer == -1)
        {
            s_environmentLayer = LayerMask.NameToLayer("Environment");
        }
    }

    public void Setup(GenericObjectPool pool, Vector2 spawnPosition)
    {
        _pool = pool;
        _startPosition = spawnPosition;
        transform.position = spawnPosition;
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
        if (_isReturned) return; // уже возвращены в этом кадре — игнорируем повторный триггер

        if (other.TryGetComponent<Health>(out var health))
        {
            health.TakeDamage(_attacker.Damage);
            ReturnToPool();
        }
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