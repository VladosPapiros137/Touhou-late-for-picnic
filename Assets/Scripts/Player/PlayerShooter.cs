using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerShooter : MonoBehaviour
{
    [Header("Shooting Settings")]
    [SerializeField] private float _fireRate = 0.15f;
    [SerializeField] private float _damage = 10f;

    [Header("References")]
    [SerializeField] private PlayerController _playerController;
    [SerializeField] private GenericObjectPool _bulletPool;
    [SerializeField] private Transform _firePoint;
    [SerializeField] private Transform _cursorTransform;
    [SerializeField] private SpriteRenderer _weaponSpriteRenderer;

    [Header("Rotation Settings")]
    [SerializeField] private float _distanceFromPlayer = 0.8f;
    [SerializeField] private float _normalRotationSpeed = 720f;  // deg/sec
    [SerializeField] private float _laserRotationSpeed = 60f;    // deg/sec, "тяжесть" во время лазера

    private Transform _playerTransform;
    private float _fireTimer;

    private void Awake()
    {
        if (_weaponSpriteRenderer == null)
            _weaponSpriteRenderer = GetComponent<SpriteRenderer>();

        if (_playerTransform == null && transform.parent != null)
            _playerTransform = transform.parent;

        if (_playerController == null)
            _playerController = GetComponentInParent<PlayerController>();
    }

    private void Update()
    {
        HandlePositionAndRotation();
        HandleShooting();
    }

    private void HandleShooting()
    {
        if (_fireTimer > 0)
            _fireTimer -= Time.deltaTime;

        // Единая проверка через PlayerController — рывок и лазер блокируют стрельбу
        bool wantsToShoot = Mouse.current != null && Mouse.current.leftButton.isPressed;
        if (wantsToShoot && _playerController.CanShoot && _fireTimer <= 0)
        {
            Shoot();
            _fireTimer = _fireRate;
        }
    }

    private void Shoot()
    {
        if (_bulletPool == null || _firePoint == null) return;

        GameObject bulletObj = _bulletPool.Get();

        bulletObj.transform.SetPositionAndRotation(_firePoint.position, transform.rotation);

        if (bulletObj.TryGetComponent<Attacker>(out var attacker))
            attacker.Damage = _damage;

        if (bulletObj.TryGetComponent<PlayerBullet>(out var bulletScript))
            bulletScript.Setup(_bulletPool, _firePoint.position);
    }

    private void HandlePositionAndRotation()
    {
        if (_playerTransform == null || _cursorTransform == null) return;

        Vector3 direction = (_cursorTransform.position - _playerTransform.position).normalized;

        if (direction != Vector3.zero)
        {
            float targetAngle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
            Quaternion targetRotation = Quaternion.Euler(0f, 0f, targetAngle);

            // Замедление вращения во время лазера — восстановленная "тяжесть" из старого HoldWeapon
            float currentSpeed = _playerController != null && _playerController.IsLaserFiring
                ? _laserRotationSpeed
                : _normalRotationSpeed;

            transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRotation, currentSpeed * Time.deltaTime);
        }

        transform.position = _playerTransform.position + transform.right * _distanceFromPlayer;

        if (_weaponSpriteRenderer != null)
        {
            float currentZ = transform.eulerAngles.z;
            if (currentZ > 180) currentZ -= 360;
            _weaponSpriteRenderer.flipY = (currentZ > 90 || currentZ < -90);
        }
    }
}