using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Object Spawner
///
/// Setup:
/// 1. Skript auf ein leeres GameObject ziehen
/// 2. In der Liste "Spawn Entries" Objekte hinzufügen:
///    - Prefab    = das Objekt das gespawnt wird
///    - Count     = wie viele davon gespawnt werden
///    - Radius    = in welchem Radius um den Spawner herum
/// 3. SpawnOnStart = true → spawnt automatisch beim Start
///    oder SpawnAll() per Button / anderem Skript aufrufen
/// </summary>
public class ObjectSpawner : MonoBehaviour
{
    [System.Serializable]
    public class SpawnEntry
    {
        public string label = "Objekt";        // Nur zur Übersicht im Inspector
        public GameObject prefab;              // Welches Objekt
        [Range(1, 100)]
        public int count = 1;                  // Wie viele
        public float spawnRadius = 3f;         // Radius um den Spawner
        public float heightOffset = 0f;        // Y-Versatz (z.B. 0.5 damit es nicht im Boden spawnt)
        public bool randomRotation = true;     // Zufällige Y-Rotation beim Spawn
    }

    [Header("Spawn Einstellungen")]
    public List<SpawnEntry> spawnEntries = new List<SpawnEntry>();

    [Header("Optionen")]
    public bool spawnOnStart = true;
    public bool spawnOnlyOnGround = true;   // Raycast nach unten, spawnt auf Bodenoberfläche
    public LayerMask groundLayer = ~0;

    [Header("Debug")]
    public bool showGizmos = true;

    // Alle gespawnten Objekte merken (zum späteren Löschen)
    private List<GameObject> spawnedObjects = new List<GameObject>();

    void Start()
    {
        if (spawnOnStart)
            SpawnAll();
    }

    // ── Alles spawnen ─────────────────────────────────────────────────────────

    [ContextMenu("Spawn All")]
    public void SpawnAll()
    {
        foreach (SpawnEntry entry in spawnEntries)
        {
            if (entry.prefab == null)
            {
                Debug.LogWarning($"[Spawner] '{entry.label}' hat kein Prefab zugewiesen!");
                continue;
            }

            for (int i = 0; i < entry.count; i++)
                SpawnOne(entry);
        }

        Debug.Log($"[Spawner] {spawnedObjects.Count} Objekte gespawnt.");
    }

    void SpawnOne(SpawnEntry entry)
    {
        // Zufällige Position im Kreis
        Vector2 randomCircle = Random.insideUnitCircle * entry.spawnRadius;
        Vector3 spawnPos = transform.position + new Vector3(randomCircle.x, entry.heightOffset, randomCircle.y);

        // Optional: auf Boden snappen
        if (spawnOnlyOnGround)
        {
            Vector3 rayOrigin = spawnPos + Vector3.up * 10f;
            if (Physics.Raycast(rayOrigin, Vector3.down, out RaycastHit hit, 20f, groundLayer))
                spawnPos = hit.point + Vector3.up * entry.heightOffset;
        }

        // Rotation
        Quaternion rot = entry.randomRotation
            ? Quaternion.Euler(0f, Random.Range(0f, 360f), 0f)
            : Quaternion.identity;

        GameObject obj = Instantiate(entry.prefab, spawnPos, rot);
        spawnedObjects.Add(obj);
    }

    // ── Alle gespawnten Objekte löschen ───────────────────────────────────────

    [ContextMenu("Clear All Spawned")]
    public void ClearAll()
    {
        foreach (GameObject obj in spawnedObjects)
            if (obj != null) Destroy(obj);

        spawnedObjects.Clear();
        Debug.Log("[Spawner] Alle gespawnten Objekte gelöscht.");
    }

    // ── Neu spawnen (erst löschen, dann neu) ──────────────────────────────────

    [ContextMenu("Respawn All")]
    public void RespawnAll()
    {
        ClearAll();
        SpawnAll();
    }

    // ── Gizmos: Spawn-Radius anzeigen ─────────────────────────────────────────

    void OnDrawGizmosSelected()
    {
        if (!showGizmos) return;

        foreach (SpawnEntry entry in spawnEntries)
        {
            Gizmos.color = new Color(0.2f, 0.8f, 1f, 0.3f);
            Gizmos.DrawSphere(transform.position, entry.spawnRadius);
            Gizmos.color = new Color(0.2f, 0.8f, 1f, 1f);
            Gizmos.DrawWireSphere(transform.position, entry.spawnRadius);
        }
    }
}