using UnityEngine;

/// <summary>
/// PlayerMovement – Character Controller Version
/// 
/// Setup:
/// 1. Rigidbody ENTFERNEN (falls vorhanden)
/// 2. Character Controller Komponente hinzufügen (Add Component → Character Controller)
/// 3. Ein leeres GameObject "CameraHolder" als Kind des Players erstellen (Y = 1.6)
/// 4. Main Camera in CameraHolder ziehen, Position auf 0,0,0
/// 5. Im Inspector: cameraHolder zuweisen
/// </summary>
[RequireComponent(typeof(CharacterController))]
public class PlayerMovement : MonoBehaviour
{
    [Header("Bewegung")]
    public float moveSpeed = 6f;

    [Header("Maus")]
    public Transform cameraHolder;
    public float mouseSensitivity = 2f;
    public float maxLookAngle = 80f;

    [Header("Springen & Schwerkraft")]
    public float jumpHeight = 1.5f;
    public float gravity = -20f;

    private CharacterController cc;
    private float yRotation = 0f;
    private float verticalRotation = 0f;
    private float velocityY = 0f;

    void Awake()
    {
        cc = GetComponent<CharacterController>();
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    void Update()
    {
        // Maus ein/ausrasten
        if (Input.GetKeyDown(KeyCode.Escape))
        { Cursor.lockState = CursorLockMode.None; Cursor.visible = true; }
        if (Input.GetMouseButtonDown(0) && Cursor.lockState != CursorLockMode.Locked)
        { Cursor.lockState = CursorLockMode.Locked; Cursor.visible = false; }

        HandleMouseLook();
        HandleMovement();
    }

    void HandleMouseLook()
    {
        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity;

        // Player horizontal drehen
        yRotation += mouseX;
        transform.rotation = Quaternion.Euler(0f, yRotation, 0f);

        // Kamera vertikal neigen
        verticalRotation = Mathf.Clamp(verticalRotation - mouseY, -maxLookAngle, maxLookAngle);
        if (cameraHolder != null)
            cameraHolder.localRotation = Quaternion.Euler(verticalRotation, 0f, 0f);
    }

    void HandleMovement()
    {
        // WASD Bewegung
        float forward = 0f;
        float strafe = 0f;
        if (Input.GetKey(KeyCode.W)) forward = 1f;
        if (Input.GetKey(KeyCode.S)) forward = -1f;
        if (Input.GetKey(KeyCode.A)) strafe = -1f;
        if (Input.GetKey(KeyCode.D)) strafe = 1f;

        Vector3 moveDir = (transform.forward * forward + transform.right * strafe).normalized;

        // Schwerkraft
        if (cc.isGrounded)
        {
            velocityY = -2f; // klein negativ damit isGrounded stabil bleibt

            if (Input.GetKeyDown(KeyCode.Space))
                velocityY = Mathf.Sqrt(jumpHeight * -2f * gravity);
        }
        else
        {
            velocityY += gravity * Time.deltaTime;
        }

        Vector3 velocity = moveDir * moveSpeed + Vector3.up * velocityY;
        cc.Move(velocity * Time.deltaTime);
    }
}