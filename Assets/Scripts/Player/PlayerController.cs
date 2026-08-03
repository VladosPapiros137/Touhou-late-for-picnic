using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody2D))]
public class PlayerController : MonoBehaviour
{
    [Header("Base Movement")]
    [SerializeField] private float _moveSpeed = 6f;
    [SerializeField, Range(0f, 1f)] private float _laserMoveSpeedMultiplier = 0.5f; // ГДД: -50% скорости во время лазера

    [Header("References")]
    [SerializeField] private Transform _cursorTransform;
    [SerializeField] private SpriteRenderer _bodySpriteRenderer; // визуальный флип, НЕ через localScale

    // ЕДИНЫЙ ИСТОЧНИК ПРАВДЫ для состояния игрока.
    // PlayerDash, PlayerShooter и PlayerLaser читают/пишут СЮДА, а не заводят свои копии флагов.
    public bool IsDashing { get; set; }
    public bool IsLaserFiring { get; set; }

    public bool CanStartDash => !IsDashing && !IsLaserFiring;
    public bool CanFireLaser => !IsDashing && !IsLaserFiring;
    public bool CanShoot => !IsDashing && !IsLaserFiring;

    // Заменяет проверку transform.localScale.x > 0, убранную вместе с негативным scale-флипом
    public bool FacingRight => _bodySpriteRenderer == null || !_bodySpriteRenderer.flipX;

    public Vector2 MovementDirection { get; private set; }

    private Rigidbody2D _rb;

    private void Awake()
    {
        _rb = GetComponent<Rigidbody2D>();

        if (_bodySpriteRenderer == null)
            _bodySpriteRenderer = GetComponent<SpriteRenderer>();
    }

    private void Update()
    {
        // Во время рывка вводом не управляем — физику ведёт PlayerDash
        if (IsDashing) return;

        ReadMovementInput();
        HandleSpriteRotation();
    }

    private void FixedUpdate()
    {
        if (IsDashing) return;

        float speedMultiplier = IsLaserFiring ? _laserMoveSpeedMultiplier : 1f;
        _rb.linearVelocity = MovementDirection * _moveSpeed * speedMultiplier;
    }

    private void ReadMovementInput()
    {
        Vector2 input = Vector2.zero;

        if (Keyboard.current != null)
        {
            if (Keyboard.current.wKey.isPressed) input.y += 1f;
            if (Keyboard.current.sKey.isPressed) input.y -= 1f;
            if (Keyboard.current.aKey.isPressed) input.x -= 1f;
            if (Keyboard.current.dKey.isPressed) input.x += 1f;
        }

        MovementDirection = input.normalized;
    }

    private void HandleSpriteRotation()
    {
        if (_cursorTransform == null || _bodySpriteRenderer == null) return;

        // ВАЖНО: флип через flipX, а НЕ через отрицательный transform.localScale.
        // Отрицательный scale у родителя ломает мировое вращение дочерних объектов
        // (в т.ч. PlayerShooter) — именно это вызывало "скачок" оружия на 12/6 часах.
        _bodySpriteRenderer.flipX = _cursorTransform.position.x < transform.position.x;
    }
}