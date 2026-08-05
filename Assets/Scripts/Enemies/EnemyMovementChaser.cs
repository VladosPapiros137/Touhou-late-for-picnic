using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class EnemyMovementChaser : MonoBehaviour
{
    [Header("Movement Settings")]
    [SerializeField] private float _moveSpeed = 3.5f;

    [Header("Distance Control")]
    [Tooltip("Дистанция, на которой враг останавливается перед игроком")]
    [SerializeField] private float _stoppingDistance = 0f;

    [Tooltip("Если игрок ближе этой дистанции, враг начинает отступать назад (кайтить). 0 — не отступает.")]
    [SerializeField] private float _retreatDistance = 0f;

    private EnemyAggroDetector _aggroDetector;
    private Rigidbody2D _rb;

    private void Awake()
    {
        _rb = GetComponent<Rigidbody2D>();
        _aggroDetector = GetComponent<EnemyAggroDetector>();
    }

    private void FixedUpdate()
    {
        if (_aggroDetector == null || !_aggroDetector.IsPlayerDetected || _aggroDetector.PlayerTransform == null)
        {
            _rb.linearVelocity = Vector2.zero;
            return;
        }

        Vector2 targetPosition = _aggroDetector.PlayerTransform.position;
        float distanceToPlayer = Vector2.Distance(_rb.position, targetPosition);

        // 1. Игрок слишком близко — ОТСТУПАЕМ назад
        if (_retreatDistance > 0f && distanceToPlayer < _retreatDistance)
        {
            Vector2 retreatDirection = (_rb.position - targetPosition).normalized;
            Vector2 newPosition = _rb.position + retreatDirection * _moveSpeed * Time.fixedDeltaTime;
            _rb.MovePosition(newPosition);
        }
        // 2. Игрок дальше требуемой дистанции — ПРИБЛИЖАЕМСЯ
        else if (distanceToPlayer > _stoppingDistance)
        {
            Vector2 newPosition = Vector2.MoveTowards(_rb.position, targetPosition, _moveSpeed * Time.fixedDeltaTime);
            _rb.MovePosition(newPosition);
        }
        // 3. Игрок на идеальной дистанции — СТОИМ на месте и стреляем
        else
        {
            _rb.linearVelocity = Vector2.zero;
        }
    }
}