using UnityEngine;
using UnityEngine.InputSystem;

public class CustomCursor : MonoBehaviour
{
    private Camera _mainCamera;

    private void Start()
    {
        _mainCamera = Camera.main;
        Cursor.visible = false;
    }

    private void Update()
    {
        if (Mouse.current == null || _mainCamera == null) return;

        // Получаем позицию мыши через новый Input System
        Vector2 mouseScreenPos = Mouse.current.position.ReadValue();

        // Переводим экранные координаты в мировые
        Vector3 worldPos = _mainCamera.ScreenToWorldPoint(new Vector3(
            mouseScreenPos.x,
            mouseScreenPos.y,
            Mathf.Abs(_mainCamera.transform.position.z)
        ));

        transform.position = worldPos;
    }
}