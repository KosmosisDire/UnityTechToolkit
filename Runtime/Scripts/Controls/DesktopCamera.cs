using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

// This script creates editor-like camera controls for the desktop.
public class DesktopCamera : MonoBehaviour
{
    [Header("Mouse Input")]
    public InputAction mouseLookButton;
    public InputAction mousePanButton;
    public InputAction zoomAction;

    [Header("Keyboard Input")]
    public InputAction strafeLeft;
    public InputAction strafeRight;
    public InputAction forward;
    public InputAction backward;

    public float lookSensitivity = 1f;
    public float panSensitivity = 1f;
    public float scrollSpeed = 1f;

    float XSensitivity;
    float YSensitivity;


    [Header("Options")]
    [SerializeField] private bool hideCursor;
    public LayerMask cantScrollInside;

    [Header("References")]
    public Transform cameraParent;


    new Camera camera;
    float horizontal, vertical;
    Quaternion cameraRotation, parentRotation;

    void OnEnable()
    {
        camera = GetComponent<Camera>();
        cameraRotation = transform.localRotation;
        parentRotation = cameraParent.localRotation;
        if (hideCursor)
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }

        mouseLookButton.Enable();
        mousePanButton.Enable();
        zoomAction.Enable();

        strafeLeft.Enable();
        strafeRight.Enable();
        forward.Enable();
        backward.Enable();

    }




    void OnDisable()
    {
        if (hideCursor)
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }

        mouseLookButton.Disable();
        mousePanButton.Disable();
        zoomAction.Disable();

        strafeLeft.Disable();
        strafeRight.Disable();
        forward.Disable();
        backward.Disable();
    }

    Vector2 mouseLookDelta = Vector2.zero;
    Vector2 mousePanDelta = Vector2.zero;
    Vector3 cameraStrafeDelta = Vector3.zero;
    float scrollDelta = 0;
    float falloff;
    float hitDistance;
    void LateUpdate()
    {
        if(mouseLookButton.IsPressed())
        {
            mouseLookDelta = Mouse.current.delta.ReadValue();
        }
        else
        {
            mouseLookDelta = Vector2.zero;
        }

        if(mousePanButton.IsPressed() && Mouse.current.delta.ReadValue() != Vector2.zero)
        {
            mousePanDelta = Mouse.current.delta.ReadValue();
        }
        else
        {
            mousePanDelta = Vector2.Lerp(mousePanDelta, Vector2.zero, Time.deltaTime * 20f);
        }

        if(Mouse.current.scroll.y.ReadValue() != 0)
        {
            scrollDelta = Mouse.current.scroll.y.ReadValue() * 0.95f + scrollDelta * 0.05f;
        }
        else
        {
            scrollDelta = Mathf.Lerp(scrollDelta, 0, Time.deltaTime * 30f);
        }

        var ratio = (float)Screen.width / (float)Screen.height;
        XSensitivity = lookSensitivity * ratio * 0.1f;
        YSensitivity = lookSensitivity * 0.1f;

        Physics.Raycast(camera.transform.position, camera.transform.forward, out RaycastHit hit, 20, cantScrollInside);
        if(!hit.collider) hit.distance = 20;
        falloff = Mathf.Max(1 - (1/Mathf.Pow((hit.distance/3.5f) + 1, 2)));
        hitDistance = hit.distance;

        MouseLook();
        MousePan();
        Zoom();
        Strafe();

    }

    
    void MouseLook()
    {
        transform.localRotation *= Quaternion.Euler(-mouseLookDelta.y * YSensitivity / 10f, 0f, 0f);
        cameraParent.localRotation *= Quaternion.Euler(0f, mouseLookDelta.x * XSensitivity / 10f, 0f);
    }

    void MousePan()
    {
        cameraParent.position -= cameraParent.right * mousePanDelta.x * panSensitivity * falloff * Time.deltaTime;
        cameraParent.position -= camera.transform.up * mousePanDelta.y * panSensitivity * falloff * Time.deltaTime;
    }

    void Zoom()
    {
        cameraParent.transform.position += (camera.transform.forward) * Mathf.Clamp(scrollDelta * scrollSpeed * Time.deltaTime * falloff, -hitDistance / 2f, hitDistance / 2f);
    }

    void Strafe()
    {
        cameraStrafeDelta = Vector3.Lerp(cameraStrafeDelta, Vector3.zero, Time.deltaTime * 15f);
        if(strafeLeft.IsPressed())
        {
            cameraStrafeDelta += camera.transform.right * -1 * panSensitivity * falloff * Time.deltaTime * 0.5f;
        }
        if(strafeRight.IsPressed())
        {
            cameraStrafeDelta += camera.transform.right * panSensitivity * falloff * Time.deltaTime * 0.5f;
        }
        if(forward.IsPressed())
        {
            cameraStrafeDelta += camera.transform.forward * panSensitivity * falloff * Time.deltaTime * 0.5f;
        }
        if(backward.IsPressed())
        {
            cameraStrafeDelta += camera.transform.forward * -1 * panSensitivity * falloff * Time.deltaTime * 0.5f;
        }

        cameraParent.position += cameraStrafeDelta;
    }
}
