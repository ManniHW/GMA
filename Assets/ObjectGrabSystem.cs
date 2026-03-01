using UnityEngine;

/// <summary>
/// R.E.P.O Style Objekt-Trage-System + Gewichtssystem
///
/// Setup:
/// 1. Skript auf den Player ziehen
/// 2. Im Inspector: cameraTransform = Main Camera
/// 3. playerMovement = dein PlayerMovement Skript zuweisen
/// 4. Aufhebbare Objekte brauchen: Rigidbody + Collider + Tag "Grabbable"
///    Die Masse des Rigidbody bestimmt das Gewicht!
///
/// Steuerung:
///   E                    = Aufheben / Loslassen
///   Linksklick           = Werfen
///   Rechtsklick (halten) = Objekt rotieren
///   Scrollrad            = Abstand anpassen
///
/// Gewicht-Effekte (basieren auf Rigidbody.mass):
///   Leicht  (< lightThreshold) : Normal schweben, volle Geschwindigkeit
///   Mittel  (< heavyThreshold) : Langsamer laufen, langsamere Kamera
///   Schwer  (>= heavyThreshold): Schleift über Boden, sehr langsam
///   Zu schwer (> maxLiftMass)  : Kann nicht aufgehoben werden
/// </summary>
public class ObjectGrabSystem : MonoBehaviour
{
    [Header("Referenzen")]
    public Transform cameraTransform;
    public PlayerMovement playerMovement; // Für Geschwindigkeits-Reduktion

    [Header("Aufheben")]
    public float pickupRange = 3f;
    public string grabbableTag = "Grabbable";

    [Header("Halten")]
    public float holdDistance = 2f;
    public float minHoldDistance = 1f;
    public float maxHoldDistance = 4f;
    public float holdSmoothing = 50f;
    public float maxHoldVelocity = 12f;

    [Header("Werfen")]
    public float throwForce = 18f;

    [Header("Rotieren")]
    public float rotateSpeed = 4f;

    [Header("Physik-Interaktion")]
    public float pushForce = 10f;

    [Header("Gewichtssystem")]
    [Tooltip("Unter dieser Masse: normales Schweben")]
    public float lightThreshold = 5f;
    [Tooltip("Über dieser Masse: schleift über den Boden")]
    public float heavyThreshold = 15f;
    [Tooltip("Über dieser Masse: kann nicht aufgehoben werden")]
    public float maxLiftMass = 30f;
    [Tooltip("Maximale Geschwindigkeitsreduktion bei schwerem Objekt (0.2 = 80% langsamer)")]
    public float minSpeedMultiplier = 0.25f;
    [Tooltip("Maximale Kamera-Reduktion bei schwerem Objekt (0.3 = 70% langsamer)")]
    public float minMouseMultiplier = 0.3f;

    // Intern
    private Rigidbody heldObject;
    private float savedDrag;
    private float savedAngularDrag;
    private bool savedGravity;
    private bool isRotating = false;
    private Quaternion grabRotationOffset;

    // Gewicht
    private float weightFactor = 0f; // 0 = leicht, 1 = maximal schwer
    private bool isDragging = false;  // Schleift über Boden

    // Gespeicherte Original-Werte vom PlayerMovement
    private float originalMoveSpeed;
    private float originalMouseSensitivity;

    // Highlight
    private Renderer lastHighlighted;
    private Color originalColor;

    void Awake()
    {
        if (cameraTransform == null && Camera.main != null)
            cameraTransform = Camera.main.transform;

        // Automatisch suchen falls nicht zugewiesen
        if (playerMovement == null)
            playerMovement = GetComponent<PlayerMovement>();
        if (playerMovement == null)
            playerMovement = FindFirstObjectByType<PlayerMovement>();

        if (playerMovement != null)
        {
            originalMoveSpeed = playerMovement.moveSpeed;
            originalMouseSensitivity = playerMovement.mouseSensitivity;
            Debug.Log($"[GrabSystem] PlayerMovement gefunden! Speed:{originalMoveSpeed} Mouse:{originalMouseSensitivity}");
        }
        else
        {
            Debug.LogError("[GrabSystem] PlayerMovement NICHT gefunden! Bitte manuell zuweisen.");
        }
    }

    void Update()
    {
        HandleHighlight();

        if (Input.GetKeyDown(KeyCode.E))
        {
            if (heldObject != null) Drop();
            else TryGrab();
        }

        if (Input.GetMouseButtonDown(0) && heldObject != null)
            Throw();

        if (Input.GetMouseButtonDown(1) && heldObject != null) isRotating = true;
        if (Input.GetMouseButtonUp(1)) isRotating = false;

        float scroll = Input.GetAxis("Mouse ScrollWheel");
        holdDistance = Mathf.Clamp(holdDistance + scroll * 1.5f, minHoldDistance, maxHoldDistance);

        if (heldObject != null && isRotating)
            HandleRotation();

        // Kontinuierlich Spieler-Speed anpassen (smooth)
        if (heldObject != null)
            ApplyWeightToPlayer();
    }

    void FixedUpdate()
    {
        if (heldObject != null)
            MoveHeldObject();
    }

    // ── Highlight ─────────────────────────────────────────────────────────────

    void HandleHighlight()
    {
        if (heldObject != null) { ClearHighlight(); return; }

        if (Physics.Raycast(cameraTransform.position, cameraTransform.forward,
            out RaycastHit hit, pickupRange))
        {
            if (hit.collider.CompareTag(grabbableTag))
            {
                // Zu schwer zum Heben? Rot einfärben als Hinweis
                Rigidbody rb = hit.collider.GetComponent<Rigidbody>();
                bool tooHeavy = rb != null && rb.mass > maxLiftMass;

                Renderer rend = hit.collider.GetComponentInChildren<Renderer>();
                if (rend != null && rend != lastHighlighted)
                {
                    ClearHighlight();
                    lastHighlighted = rend;
                    originalColor = rend.material.color;
                    Color highlight = tooHeavy
                        ? Color.Lerp(originalColor, Color.red, 0.4f)   // Rot = zu schwer
                        : Color.Lerp(originalColor, Color.white, 0.35f); // Weiß = hebbar
                    rend.material.color = highlight;
                }
                return;
            }
        }
        ClearHighlight();
    }

    void ClearHighlight()
    {
        if (lastHighlighted == null) return;
        lastHighlighted.material.color = originalColor;
        lastHighlighted = null;
    }

    // ── Aufheben ──────────────────────────────────────────────────────────────

    void TryGrab()
    {
        if (!Physics.Raycast(cameraTransform.position, cameraTransform.forward,
            out RaycastHit hit, pickupRange)) return;
        if (!hit.collider.CompareTag(grabbableTag)) return;

        Rigidbody rb = hit.collider.GetComponent<Rigidbody>();
        if (rb == null) return;

        // Zu schwer? Nicht aufheben
        if (rb.mass > maxLiftMass)
        {
            Debug.Log($"Objekt zu schwer! Masse: {rb.mass}kg / Max: {maxLiftMass}kg");
            return;
        }

        heldObject = rb;

        holdDistance = Mathf.Clamp(
            Vector3.Distance(cameraTransform.position, hit.point),
            minHoldDistance, maxHoldDistance);

        savedDrag = rb.linearDamping;
        savedAngularDrag = rb.angularDamping;
        savedGravity = rb.useGravity;
        rb.linearDamping = 10f;
        rb.angularDamping = 15f;

        // Gewichtsfaktor berechnen (0 = leicht, 1 = maximal schwer)
        weightFactor = Mathf.InverseLerp(lightThreshold, maxLiftMass, rb.mass);
        isDragging = rb.mass >= heavyThreshold;

        if (isDragging)
        {
            // Schweres Objekt: Schwerkraft bleibt an, schleift über Boden
            rb.useGravity = true;
            rb.linearDamping = 5f; // Weniger Drag damit es sich am Boden bewegen kann
        }
        else
        {
            rb.useGravity = false;
        }

        // Spieler verlangsamen
        ApplyWeightToPlayer();

        grabRotationOffset = Quaternion.Inverse(cameraTransform.rotation) * heldObject.rotation;
    }

    // ── Gewicht auf Spieler anwenden ──────────────────────────────────────────

    void ApplyWeightToPlayer()
    {
        if (playerMovement == null) return;

        float speedMult = Mathf.Lerp(1f, minSpeedMultiplier, weightFactor);
        float mouseMult = Mathf.Lerp(1f, minMouseMultiplier, weightFactor);

        float targetSpeed = originalMoveSpeed * speedMult;
        float targetMouse = originalMouseSensitivity * mouseMult;

        // Direkt setzen - kein Lerp, sofort wirksam
        playerMovement.moveSpeed = targetSpeed;
        playerMovement.mouseSensitivity = targetMouse;
        Debug.Log($"[Weight] Factor:{weightFactor:F2} | Speed:{targetSpeed:F2} | Mouse:{targetMouse:F2}");
    }

    void ResetPlayerStats()
    {
        if (playerMovement == null) return;
        // Direkt zurücksetzen beim Loslassen
        playerMovement.moveSpeed = originalMoveSpeed;
        playerMovement.mouseSensitivity = originalMouseSensitivity;
    }

    // ── Loslassen ─────────────────────────────────────────────────────────────

    void Drop()
    {
        if (heldObject == null) return;
        heldObject.useGravity = savedGravity;
        heldObject.linearDamping = savedDrag;
        heldObject.angularDamping = savedAngularDrag;
        heldObject.linearVelocity = Vector3.ClampMagnitude(heldObject.linearVelocity, 4f);
        heldObject = null;
        isRotating = false;
        isDragging = false;
        weightFactor = 0f;
        ResetPlayerStats();
    }

    // ── Werfen ────────────────────────────────────────────────────────────────

    void Throw()
    {
        Rigidbody rb = heldObject;
        float savedWeight = weightFactor;
        Drop();
        rb.useGravity = true;
        rb.linearDamping = savedDrag;

        // Je schwerer, desto kürzer und langsamer fliegt es
        // weightFactor 0 = volle Kraft, weightFactor 1 = nur 20% Kraft
        float throwMultiplier = Mathf.Lerp(1f, 0.2f, savedWeight);
        rb.AddForce(cameraTransform.forward * throwForce * throwMultiplier, ForceMode.VelocityChange);

        Debug.Log($"[Throw] WeightFactor:{savedWeight:F2} | Kraft:{throwForce * throwMultiplier:F1} von max {throwForce}");
    }

    // ── Objekt bewegen ────────────────────────────────────────────────────────

    void MoveHeldObject()
    {
        Vector3 targetPos;

        if (isDragging)
        {
            // Schweres Objekt: Zielposition auf Bodenhöhe (schleift)
            Vector3 horizontalTarget = cameraTransform.position
                + new Vector3(cameraTransform.forward.x, 0, cameraTransform.forward.z).normalized
                * holdDistance;

            // Raycast von oben nach unten, aber Objekt selbst ignorieren
            int originalLayer = heldObject.gameObject.layer;
            heldObject.gameObject.layer = Physics.IgnoreRaycastLayer;

            if (Physics.Raycast(horizontalTarget + Vector3.up * 3f, Vector3.down, out RaycastHit groundHit, 8f))
                targetPos = groundHit.point + Vector3.up * 0.3f;
            else
                targetPos = new Vector3(horizontalTarget.x, heldObject.position.y, horizontalTarget.z);

            heldObject.gameObject.layer = originalLayer;
        }
        else
        {
            // Leichtes Objekt: normal schweben
            targetPos = cameraTransform.position + cameraTransform.forward * holdDistance;
        }

        Vector3 dir = targetPos - heldObject.position;
        float dist = dir.magnitude;

        // Geschwindigkeit je nach Gewicht reduzieren
        float velocityLimit = Mathf.Lerp(maxHoldVelocity, maxHoldVelocity * 0.4f, weightFactor);

        if (isDragging)
        {
            // Nur horizontal bewegen, Y komplett ignorieren
            Vector3 horizontalDir = new Vector3(dir.x, 0f, dir.z);
            Vector3 horizontalVel = Vector3.ClampMagnitude(
                horizontalDir * holdSmoothing * Time.fixedDeltaTime, velocityLimit * 0.5f);
            // Y-Velocity von Physik/Gravity übernehmen lassen, nicht überschreiben
            heldObject.linearVelocity = new Vector3(horizontalVel.x, heldObject.linearVelocity.y, horizontalVel.z);
        }
        else
        {
            heldObject.linearVelocity = Vector3.ClampMagnitude(
                dir * holdSmoothing * Time.fixedDeltaTime, velocityLimit);
        }

        // Rotation beibehalten (nur wenn nicht am rotieren)
        if (!isRotating)
        {
            Quaternion targetRot = cameraTransform.rotation * grabRotationOffset;
            heldObject.MoveRotation(Quaternion.Slerp(heldObject.rotation, targetRot, 15f * Time.fixedDeltaTime));
            heldObject.angularVelocity = Vector3.zero;
        }

        // Physik-Interaktion
        Collider[] nearby = Physics.OverlapSphere(heldObject.position, 0.6f);
        foreach (Collider col in nearby)
        {
            if (col.gameObject == heldObject.gameObject) continue;
            Rigidbody otherRb = col.GetComponent<Rigidbody>();
            if (otherRb == null) continue;
            Vector3 pushDir = (col.transform.position - heldObject.position).normalized;
            otherRb.AddForce(pushDir * pushForce, ForceMode.Impulse);
        }

        if (dist > maxHoldDistance + 2.5f)
            Drop();
    }

    // ── Rotieren ─────────────────────────────────────────────────────────────

    void HandleRotation()
    {
        // Schwere Objekte langsamer rotieren
        float effectiveRotateSpeed = Mathf.Lerp(rotateSpeed, rotateSpeed * 0.3f, weightFactor);

        float mouseX = Input.GetAxis("Mouse X") * effectiveRotateSpeed;
        float mouseY = Input.GetAxis("Mouse Y") * effectiveRotateSpeed;

        heldObject.transform.Rotate(cameraTransform.up, -mouseX, Space.World);
        heldObject.transform.Rotate(cameraTransform.right, mouseY, Space.World);
        heldObject.angularVelocity = Vector3.zero;

        grabRotationOffset = Quaternion.Inverse(cameraTransform.rotation) * heldObject.rotation;
    }

    // ── Gizmos ────────────────────────────────────────────────────────────────

    void OnDrawGizmosSelected()
    {
        if (cameraTransform == null) return;
        Gizmos.color = Color.yellow;
        Gizmos.DrawLine(cameraTransform.position,
            cameraTransform.position + cameraTransform.forward * pickupRange);
        Gizmos.DrawWireSphere(
            cameraTransform.position + cameraTransform.forward * pickupRange, 0.1f);
    }
}