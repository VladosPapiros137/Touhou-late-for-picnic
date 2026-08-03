using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(LineRenderer))]
public class PlayerLaser : MonoBehaviour
{
    [Header("Laser Stats (ГДД: тиковый урон, не разовый хит)")]
    [SerializeField] private float _damagePerTick = 15f;
    [SerializeField] private float _tickInterval = 0.1f;
    [SerializeField] private float _laserDuration = 1.2f;
    [SerializeField] private float _cooldown = 8f;
    [SerializeField] private float _maxDistance = 50f;

    [Header("References")]
    [SerializeField] private LineRenderer _lineRenderer;
    [SerializeField] private Transform _firePoint;
    [SerializeField] private PlayerController _playerController;

    [Header("Layers")]
    [SerializeField] private LayerMask _obstacleMask;   // Environment
    [SerializeField] private LayerMask _damageableMask; // всё, что может иметь Health

    private bool _canFire = true;
    private Coroutine _laserRoutine;

    // Переиспользуемый буфер вместо RaycastAll — без GC-мусора каждый кадр
    private readonly RaycastHit2D[] _hitBuffer = new RaycastHit2D[16];

    private void Awake()
    {
        if (_lineRenderer == null) _lineRenderer = GetComponent<LineRenderer>();
        if (_playerController == null) _playerController = GetComponentInParent<PlayerController>();

        _lineRenderer.enabled = false;
    }

    private void Update()
    {
        if (Keyboard.current == null || !Keyboard.current.eKey.wasPressedThisFrame) return;

        if (_canFire && _playerController.CanFireLaser)
        {
            _laserRoutine = StartCoroutine(FireLaserRoutine());
        }
    }

    private void OnDisable()
    {
        // Защита от залипшего IsLaserFiring / кулдауна, если объект отключили посреди выстрела
        if (_laserRoutine != null)
        {
            StopCoroutine(_laserRoutine);
            _laserRoutine = null;
        }

        if (_playerController != null && _playerController.IsLaserFiring)
        {
            _playerController.IsLaserFiring = false;
        }

        _lineRenderer.enabled = false;
    }

    private IEnumerator FireLaserRoutine()
    {
        _canFire = false;
        _playerController.IsLaserFiring = true;
        _lineRenderer.enabled = true;

        float durationTimer = 0f;
        float tickTimer = 0f;

        while (durationTimer < _laserDuration)
        {
            // Если рывок каким-то образом всё же начался — обрываем лазер немедленно
            if (_playerController.IsDashing) break;

            durationTimer += Time.deltaTime;
            tickTimer += Time.deltaTime;

            float beamLength = UpdateBeamVisual();

            if (tickTimer >= _tickInterval)
            {
                ApplyTickDamage(beamLength);
                tickTimer = 0f;
            }

            yield return null;
        }

        _lineRenderer.enabled = false;
        _playerController.IsLaserFiring = false;

        yield return new WaitForSeconds(_cooldown);
        _canFire = true;
        _laserRoutine = null;
    }

    /// <summary>Считает длину луча до ближайшей стены и рисует его. Возвращает актуальную длину.</summary>
    private float UpdateBeamVisual()
    {
        Vector2 origin = _firePoint.position;
        Vector2 direction = _firePoint.right;

        RaycastHit2D wallHit = Physics2D.Raycast(origin, direction, _maxDistance, _obstacleMask);
        float beamLength = wallHit.collider != null ? wallHit.distance : _maxDistance;

        _lineRenderer.SetPosition(0, origin);
        _lineRenderer.SetPosition(1, origin + direction * beamLength);

        return beamLength;
    }

    private void ApplyTickDamage(float beamLength)
    {
        Vector2 origin = _firePoint.position;
        Vector2 direction = _firePoint.right;

        int hitCount = Physics2D.RaycastNonAlloc(origin, direction, _hitBuffer, beamLength, _damageableMask);

        for (int i = 0; i < hitCount; i++)
        {
            if (_hitBuffer[i].collider.TryGetComponent<Health>(out var health))
            {
                if (health.gameObject == _playerController.gameObject) continue; // не бьём себя
                health.TakeDamage(_damagePerTick);
            }
        }
    }
}