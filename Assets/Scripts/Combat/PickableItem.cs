using UnityEngine;

public class PickableItem : MonoBehaviour
{
    public enum LootKind { Coin, Exp }

    [Header("Loot Info (выставляется на префабе — Small/Medium/Large)")]
    [SerializeField] private LootKind _kind;
    [SerializeField] private int _value = 1;

    [Header("Settings")]
    [SerializeField] private float _flySpeed = 12f;
    [SerializeField] private float _minDistanceToPick = 0.3f;

    public LootKind Kind => _kind;
    public int Value => _value;

    private GenericObjectPool _pool;
    private Transform _targetPlayer;
    private bool _isFlying;
    private bool _isReturned;

    public void Setup(GenericObjectPool pool, Vector2 spawnPosition)
    {
        _pool = pool;
        transform.position = spawnPosition;
        _targetPlayer = null;
        _isFlying = false;
        _isReturned = false;
    }

    private void Update()
    {
        if (!_isFlying || _targetPlayer == null || _isReturned) return;

        transform.position = Vector3.MoveTowards(
            transform.position,
            _targetPlayer.position,
            _flySpeed * Time.deltaTime
        );

        if (Vector2.Distance(transform.position, _targetPlayer.position) <= _minDistanceToPick)
        {
            Collect();
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (_isFlying || _isReturned) return;

        if (other.TryGetComponent<PlayerController>(out var player))
        {
            _targetPlayer = player.transform;
            _isFlying = true;
        }
    }

    private void Collect()
    {
        // Задел под будущий PlayerWallet / LevelSystem, например:
        // PlayerWallet.Instance.AddLoot(_kind, _value);
        // Пока не подключаем — вне плана на сегодня.
        ReturnToPool();
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