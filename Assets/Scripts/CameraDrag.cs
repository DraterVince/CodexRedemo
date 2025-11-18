using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;

public class CameraDrag : MonoBehaviour
{
    private Vector3 _origin;
    private Vector3 _difference;

    private Camera _mainCamera;

    private bool _isDragging;

    public bool bounds;
    public Vector3 minCameraPos;
    public Vector3 maxCameraPos;

    private void Awake()
    {
        _mainCamera = Camera.main;
    }

    public void OnDrag(InputAction.CallbackContext ctx)
    {
        if (ctx.started) _origin = _mainCamera.ScreenToWorldPoint(Mouse.current.position.ReadValue());
        _isDragging = ctx.started || ctx.performed;
    }

    private void LateUpdate()
    {
        if (!_isDragging) return;

        _difference = GetMousePosition - transform.position;
        transform.position = _origin - _difference;

        if (bounds)
        {
            transform.position = new Vector3(Mathf.Clamp(transform.position.x, minCameraPos.x, maxCameraPos.x),
                Mathf.Clamp(transform.position.y, minCameraPos.y, maxCameraPos.y),
                Mathf.Clamp(transform.position.z, minCameraPos.z, maxCameraPos.z));
        }
    }

    private Vector3 GetMousePosition => _mainCamera.ScreenToWorldPoint(Mouse.current.position.ReadValue());

}