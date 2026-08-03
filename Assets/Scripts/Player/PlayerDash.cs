using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody2D), typeof(PlayerController))]
public class PlayerDash : MonoBehaviour
{
    [Header("Dash Settings")]
    [SerializeField] private float _dashSpeed = 15f;
    [SerializeField] private float _dashDuration = 0.2f;
    [SerializeField] private float _dashCooldown = 2.5f;

    private static int s_invincibleLayer = -1; // -1 = ещё не закешировано

    private bool _canDash = true;
    private int _originalLayer;
    private Coroutine _dashRoutine;

    private PlayerController _playerController;
    private Rigidbody2D _rb;

    private void Awake()
    {
        _playerController = GetComponent<PlayerController>();
        _rb = GetComponent<Rigidbody2D>();

        // LayerMask.NameToLayer нельзя вызывать в инициализаторе static-поля —
        // кешируем лениво здесь, один раз на весь домен.
        if (s_invincibleLayer == -1)
        {
            s_invincibleLayer = LayerMask.NameToLayer("Player_Invincible");
        }
    }

    private void Update()
    {
        if (Keyboard.current == null || !Keyboard.current.spaceKey.wasPressedThisFrame) return;

        // Единая точка проверки: рывок нельзя начать во время рывка ИЛИ лазера
        if (_canDash && _playerController.CanStartDash)
        {
            _dashRoutine = StartCoroutine(PerformDash());
        }
    }

    private void OnDisable()
    {
        // Защита от "залипшего" состояния, если объект отключили/уничтожили посреди рывка
        if (_dashRoutine != null)
        {
            StopCoroutine(_dashRoutine);
            _dashRoutine = null;
        }

        if (_playerController.IsDashing)
        {
            gameObject.layer = _originalLayer;
            _playerController.IsDashing = false;
        }
    }

    private IEnumerator PerformDash()
    {
        _canDash = false;
        _playerController.IsDashing = true;

        _originalLayer = gameObject.layer;
        if (s_invincibleLayer != -1)
        {
            gameObject.layer = s_invincibleLayer;
        }

        Vector2 dashDirection = _playerController.MovementDirection;
        if (dashDirection == Vector2.zero)
        {
            dashDirection = new Vector2(_playerController.FacingRight ? 1f : -1f, 0f);
        }

        _rb.linearVelocity = dashDirection.normalized * _dashSpeed;

        yield return new WaitForSeconds(_dashDuration);

        // Плавный докат
        float inertiaTimer = 0f;
        const float inertiaDuration = 0.1f;
        Vector2 velocityAtEnd = _rb.linearVelocity;

        while (inertiaTimer < inertiaDuration)
        {
            inertiaTimer += Time.deltaTime;
            _rb.linearVelocity = Vector2.Lerp(velocityAtEnd, Vector2.zero, inertiaTimer / inertiaDuration);
            yield return null;
        }

        _rb.linearVelocity = Vector2.zero;
        gameObject.layer = _originalLayer;
        _playerController.IsDashing = false;

        yield return new WaitForSeconds(_dashCooldown);
        _canDash = true;
        _dashRoutine = null;
    }
}