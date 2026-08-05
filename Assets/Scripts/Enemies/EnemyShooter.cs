using System.Collections;
using UnityEngine;

public enum ShootingPattern
{
    Single,     // Одиночный выстрел
    Burst,      // Очередь из N пуль с паузой между ними
    Ring,       // Кольцо из пуль во все стороны (360°)
    Shotgun     // Веер пуль (дробовик) под углом
}

[RequireComponent(typeof(EnemyAggroDetector))]
public class EnemyShooter : MonoBehaviour
{
    [Header("General Settings")]
    [SerializeField] private ShootingPattern _pattern = ShootingPattern.Single;
    [SerializeField] private float _fireRate = 1.5f; // Пауза между атаками
    [SerializeField] private Transform _firePoint;
    [SerializeField] private GenericObjectPool _bulletPool;

    [Header("Burst Settings (для очереди)")]
    [SerializeField] private int _burstCount = 3;
    [SerializeField] private float _burstDelay = 0.1f;

    [Header("Ring & Shotgun Settings (для веера / кольца)")]
    [SerializeField] private int _bulletsInRing = 8;
    [SerializeField] private float _shotgunSpreadAngle = 30f; // Угол разлета для дробовика

    private EnemyAggroDetector _aggroDetector;
    private float _nextFireTime;
    private bool _isFiring;

    private void Awake()
    {
        _aggroDetector = GetComponent<EnemyAggroDetector>();
        if (_firePoint == null) _firePoint = transform;
    }

    private void Update()
    {
        if (_isFiring) return;
        if (!_aggroDetector.IsPlayerDetected || _aggroDetector.PlayerTransform == null) return;

        if (Time.time >= _nextFireTime)
        {
            StartCoroutine(ExecuteShootingPattern());
            _nextFireTime = Time.time + _fireRate;
        }
    }

    private IEnumerator ExecuteShootingPattern()
    {
        _isFiring = true;

        Vector2 targetPos = _aggroDetector.PlayerTransform.position;
        Vector2 firePos = _firePoint.position;
        Vector2 dirToPlayer = (targetPos - firePos).normalized;
        float baseAngle = Mathf.Atan2(dirToPlayer.y, dirToPlayer.x) * Mathf.Rad2Deg;

        switch (_pattern)
        {
            case ShootingPattern.Single:
                SpawnBullet(firePos, baseAngle);
                break;

            case ShootingPattern.Burst:
                for (int i = 0; i < _burstCount; i++)
                {
                    // Пересчитываем направление к игроку на случай, если он сместился
                    if (_aggroDetector.PlayerTransform != null)
                    {
                        Vector2 currentDir = ((Vector2)_aggroDetector.PlayerTransform.position - (Vector2)_firePoint.position).normalized;
                        baseAngle = Mathf.Atan2(currentDir.y, currentDir.x) * Mathf.Rad2Deg;
                    }

                    SpawnBullet(_firePoint.position, baseAngle);
                    yield return new WaitForSeconds(_burstDelay);
                }
                break;

            case ShootingPattern.Ring:
                float angleStepRing = 360f / _bulletsInRing;
                for (int i = 0; i < _bulletsInRing; i++)
                {
                    SpawnBullet(firePos, i * angleStepRing);
                }
                break;

            case ShootingPattern.Shotgun:
                float halfSpread = _shotgunSpreadAngle / 2f;
                float angleStepShotgun = _bulletsInRing > 1 ? _shotgunSpreadAngle / (_bulletsInRing - 1) : 0;

                for (int i = 0; i < _bulletsInRing; i++)
                {
                    float currentAngle = baseAngle - halfSpread + (i * angleStepShotgun);
                    SpawnBullet(firePos, currentAngle);
                }
                break;
        }

        _isFiring = false;
    }

    private void SpawnBullet(Vector2 position, float angle)
    {
        if (_bulletPool == null) return;

        Quaternion rotation = Quaternion.Euler(0, 0, angle);
        GameObject bulletObj = _bulletPool.Get();

        if (bulletObj.TryGetComponent<EnemyBullet>(out var bullet))
        {
            bullet.Setup(_bulletPool, position, rotation);
        }
    }

    public void SetBulletPool(GenericObjectPool bulletPool)
    {
        _bulletPool = bulletPool;
    }
}