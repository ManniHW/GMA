using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Auto Controller + Einsteigen/Aussteigen
///
/// Setup:
/// 1. Car GameObject:
///    - Rigidbody (Mass: 1200, Drag: 0.05)
///    - Dieses Skript
///    - 4 Kind-Objekte mit WheelCollider (wheelFL, wheelFR, wheelRL, wheelRR)
///    - 4 Kind-Objekte als visuelle Rad-Meshes (optional)
///    - 1 Kind-Objekt "EnterPoint" = wo der Spieler sitzt wenn er einfährt
///    - 1 Kind-Objekt "ExitPoint" = wo der Spieler hinkommt beim Aussteigen
///
/// 2. Player GameObject:
///    - PlayerMovement Skript
///    - CharacterController
///    - Im Inspector: player = Player zuweisen
///
/// Steuerung:
///   E            = Einsteigen / Aussteigen (wenn nah genug am Auto)
///   W/S          = Gas / Bremsen
///   A/D          = Lenken
///   Leertaste    = Handbremse
/// </summary>
public class CarController : MonoBehaviour
{
    [Header("Referenzen")]
    public Transform player;                // Der Spieler
    public PlayerMovement playerMovement;   // PlayerMovement Skript
    public Transform enterPoint;            // Wo der Spieler sitzt (Kind des Autos)
    public Transform exitPoint;             // Wo der Spieler rauskommt
    public Camera playerCamera;             // Spieler-Kamera (wird deaktiviert beim Einsteigen)
    public Camera carCamera;                // Auto-Kamera (Kind vom Car, wird aktiviert beim Einsteigen)

    [Header("Wheel Colliders")]
    public WheelCollider wheelFL;
    public WheelCollider wheelFR;
    public WheelCollider wheelRL;
    public WheelCollider wheelRR;

    [Header("Rad Meshes (optional, für Rotation)")]
    public Transform meshFL;
    public Transform meshFR;
    public Transform meshRL;
    public Transform meshRR;

    [Header("Motor")]
    public float motorTorque = 1500f;
    public float brakeTorque = 3000f;
    public float maxSteerAngle = 30f;
    public float maxSpeed = 120f;

    [Header("Stabilität")]
    public float downforce = 100f;          // Drückt Auto auf den Boden
    public float steerSpeedLimit = 60f;     // Ab dieser km/h wird Lenkwinkel reduziert

    [Header("Einsteigen")]
    public float enterDistance = 2.5f;      // Wie nah muss der Spieler sein

    [Header("Speedometer")]
    public Text speedText;                  // UI Text Element (oder leer lassen = auto erstellt)
    public string speedUnit = "km/h";

    // Intern
    private Rigidbody rb;
    private bool isOccupied = false;
    private CharacterController playerCC;
    private float currentSteerAngle = 0f; // Aktueller Lenkwinkel (smooth)

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        rb.centerOfMass = new Vector3(0, -0.4f, 0);

        // Auto-erstelle Speedometer UI falls keines zugewiesen
        if (speedText == null)
            CreateSpeedometerUI();

        // Automatisch suchen falls nicht zugewiesen
        if (player == null && playerMovement == null)
            playerMovement = FindFirstObjectByType<PlayerMovement>();
        if (playerMovement != null)
        {
            player = playerMovement.transform;
            playerCC = player.GetComponent<CharacterController>();
        }
    }

    void Update()
    {
        // E drücken = einsteigen oder aussteigen
        if (Input.GetKeyDown(KeyCode.E))
        {
            if (!isOccupied)
            {
                float dist = Vector3.Distance(player.position, transform.position);
                if (dist <= enterDistance)
                    EnterCar();
            }
            else
            {
                ExitCar();
            }
        }

        // Speedometer updaten
        if (speedText != null)
        {
            float speedKmh = rb.linearVelocity.magnitude * 3.6f;
            speedText.text = isOccupied ? $"{Mathf.RoundToInt(speedKmh)} {speedUnit}" : "";
        }

        // Rad-Meshes aktualisieren
        if (meshFL) UpdateWheel(wheelFL, meshFL);
        if (meshFR) UpdateWheel(wheelFR, meshFR);
        if (meshRL) UpdateWheel(wheelRL, meshRL);
        if (meshRR) UpdateWheel(wheelRR, meshRR);

        // Spieler ans Auto heften wenn drin
        if (isOccupied && enterPoint != null)
        {
            player.position = enterPoint.position;
            player.rotation = enterPoint.rotation;
        }
    }

    void FixedUpdate()
    {
        if (!isOccupied)
        {
            // Kein Input wenn niemand drin → sanft abbremsen
            wheelFL.motorTorque = 0;
            wheelFR.motorTorque = 0;
            wheelRL.motorTorque = 0;
            wheelRR.motorTorque = 0;
            return;
        }

        float gas = Input.GetAxis("Vertical");
        float steer = Input.GetAxis("Horizontal");
        bool brake = Input.GetKey(KeyCode.Space);
        bool handbrake = Input.GetKey(KeyCode.Space);

        float speedKmh = rb.linearVelocity.magnitude * 3.6f;

        // Lenkwinkel bei höherer Geschwindigkeit reduzieren (stabiler)
        float speedFactor = Mathf.Clamp01(speedKmh / steerSpeedLimit);
        float currentMaxSteer = Mathf.Lerp(maxSteerAngle, maxSteerAngle * 0.3f, speedFactor);

        // D(+1) = rechts = negativer steerAngle wegen Mesh-Ausrichtung
        float targetSteer = -steer * currentMaxSteer;
        float steerSpeed = steer == 0f ? 8f : 4f;
        currentSteerAngle = Mathf.Lerp(currentSteerAngle, targetSteer, steerSpeed * Time.fixedDeltaTime);

        wheelFL.steerAngle = currentSteerAngle;
        wheelFR.steerAngle = currentSteerAngle;

        // Motor (Hinterrad)
        float torque = 0f;
        float engineBrake = 0f;

        if (handbrake)
        {
            // Handbremse: nur Hinterräder blockieren, Vorderräder frei
            torque = 0f;
        }
        else if (Mathf.Abs(gas) > 0.01f)
        {
            torque = speedKmh < maxSpeed ? gas * motorTorque : 0f;
        }
        else
        {
            engineBrake = 300f;
        }

        wheelRL.motorTorque = torque;
        wheelRR.motorTorque = torque;

        // Handbremse: nur Hinterräder, Vorderräder bleiben frei (echte Handbremse)
        wheelFL.brakeTorque = 0f;
        wheelFR.brakeTorque = 0f;
        wheelRL.brakeTorque = handbrake ? brakeTorque : engineBrake;
        wheelRR.brakeTorque = handbrake ? brakeTorque : engineBrake;

        // Downforce: drückt Auto auf den Boden je schneller es fährt
        rb.AddForce(-transform.up * downforce * speedKmh);
    }

    // ── Einsteigen ────────────────────────────────────────────────────────────

    void EnterCar()
    {
        isOccupied = true;

        // Player-Bewegung deaktivieren
        if (playerMovement != null) playerMovement.enabled = false;
        if (playerCC != null) playerCC.enabled = false;

        // Kamera umschalten: Spieler-Cam aus, Auto-Cam an
        if (playerCamera != null) playerCamera.gameObject.SetActive(false);
        if (carCamera != null) carCamera.gameObject.SetActive(true);

        // Spieler unsichtbar machen
        SetPlayerVisible(false);

        // Speedometer anzeigen
        if (speedometerCanvas != null) speedometerCanvas.SetActive(true);

        // Maus sperren
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        Debug.Log("[Car] Eingestiegen!");
    }

    // ── Aussteigen ────────────────────────────────────────────────────────────

    void ExitCar()
    {
        isOccupied = false;

        // Kamera umschalten: Auto-Cam aus, Spieler-Cam an
        if (carCamera != null) carCamera.gameObject.SetActive(false);
        if (playerCamera != null) playerCamera.gameObject.SetActive(true);

        // Spieler wieder sichtbar machen
        SetPlayerVisible(true);

        // Speedometer verstecken
        if (speedometerCanvas != null) speedometerCanvas.SetActive(false);

        // Spieler an ExitPoint teleportieren
        if (exitPoint != null)
        {
            if (playerCC != null) playerCC.enabled = false;
            player.position = exitPoint.position;
            player.rotation = exitPoint.rotation;
        }

        // Player-Bewegung wieder aktivieren
        if (playerCC != null) playerCC.enabled = true;
        if (playerMovement != null) playerMovement.enabled = true;

        Debug.Log("[Car] Ausgestiegen!");
    }

    // ── Rad-Mesh mit WheelCollider synchronisieren ────────────────────────────

    void UpdateWheel(WheelCollider wc, Transform mesh)
    {
        wc.GetWorldPose(out Vector3 pos, out Quaternion rot);
        mesh.position = pos;
        // 90° Offset damit Zylinder-Meshes korrekt als Rad stehen
        mesh.rotation = rot * Quaternion.Euler(0f, 0f, 90f);
        // Lenkrichtung der Meshes korrigieren - Y 180 dreht die Lenkvisualisation um
        // Falls Räder falsch lenken: ändere 0f zu 180f oder -180f
    }

    // ── Speedometer UI erstellen ─────────────────────────────────────────────

    void CreateSpeedometerUI()
    {
        // Canvas erstellen
        GameObject canvasObj = new GameObject("SpeedometerCanvas");
        Canvas canvas = canvasObj.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvasObj.AddComponent<CanvasScaler>().uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        canvasObj.AddComponent<GraphicRaycaster>();

        // Hintergrund Panel
        GameObject panel = new GameObject("SpeedPanel");
        panel.transform.SetParent(canvasObj.transform, false);
        Image bg = panel.AddComponent<Image>();
        bg.color = new Color(0f, 0f, 0f, 0.5f);
        RectTransform panelRect = panel.GetComponent<RectTransform>();
        panelRect.anchorMin = new Vector2(0.5f, 0f);
        panelRect.anchorMax = new Vector2(0.5f, 0f);
        panelRect.pivot = new Vector2(0.5f, 0f);
        panelRect.anchoredPosition = new Vector2(0f, 30f);
        panelRect.sizeDelta = new Vector2(160f, 60f);

        // Speed Text
        GameObject textObj = new GameObject("SpeedText");
        textObj.transform.SetParent(panel.transform, false);
        speedText = textObj.AddComponent<Text>();
        speedText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        speedText.fontSize = 28;
        speedText.fontStyle = FontStyle.Bold;
        speedText.alignment = TextAnchor.MiddleCenter;
        speedText.color = Color.white;
        RectTransform textRect = textObj.GetComponent<RectTransform>();
        textRect.anchorMin = Vector2.zero;
        textRect.anchorMax = Vector2.one;
        textRect.offsetMin = Vector2.zero;
        textRect.offsetMax = Vector2.zero;

        canvasObj.SetActive(false); // Erst beim Einsteigen sichtbar
        // Canvas merken um es ein/auszublenden
        speedometerCanvas = canvasObj;
    }

    private GameObject speedometerCanvas;

    // ── Spieler sichtbar/unsichtbar ───────────────────────────────────────────

    void SetPlayerVisible(bool visible)
    {
        // Alle Renderer auf dem Player und seinen Kindern ein/ausschalten
        foreach (Renderer rend in player.GetComponentsInChildren<Renderer>())
            rend.enabled = visible;
    }

    // ── Gizmos ────────────────────────────────────────────────────────────────

    void OnDrawGizmosSelected()
    {
        // Einsteig-Radius anzeigen
        Gizmos.color = new Color(0f, 1f, 0f, 0.3f);
        Gizmos.DrawSphere(transform.position, enterDistance);
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, enterDistance);
    }
}