# CLAUDE.md

Diese Datei hilft Claude Code (claude.ai/code) beim Arbeiten mit diesem Projekt.

## Projekt-Beschreibung

**GMA** ist ein Koop/Multiplayer-Spiel in Unity 6 (6000.0.29f1). Man leitet eine Umzugsfirma mit Freunden, nimmt Auftraege an, transportiert Moebel durch eine grosse Stadt und verdient Geld fuer Upgrades und neue Fahrzeuge. Schwere Moebel brauchen Teamarbeit, und beim Transport muss man aufpassen - Schaeden am Mobiliar werden vom Lohn abgezogen.

Dies ist ein Schulprojekt. Alle Dokumentation und Kommentare im Code sind auf **Deutsch**.

## Technik-Stack

- **Engine:** Unity 6 mit URP (Universal Render Pipeline)
- **Input:** New Input System (`InputSystem_Actions.inputactions`)
- **Sprache:** Deutsch (Code-Kommentare, UI-Texte, Docs)
- **Pakete:** Multiplayer Center (noch nicht aktiv), AI Navigation, Timeline, Visual Scripting

## Kern-Skripte

| Datei | Zeilen | Was es macht |
|-------|--------|-------------|
| `Assets/ObjectGrabSystem.cs` | ~390 | Objekte aufheben, tragen, werfen, drehen. Gewichts-System mit Geschwindigkeits-Abzuegen |
| `Assets/CarController.cs` | ~325 | Auto fahren, ein-/aussteigen, Tacho-UI, Lenkung, Bremse |
| `Assets/Spawner.cs` | ~130 | Objekte spawnen (Boxen etc.) mit Radius, Boden-Erkennung, Zufalls-Rotation |
| `Assets/PlayerMovement.cs` | ~95 | Ego-Perspektive: WASD-Bewegung, Maus-Steuerung, Springen |

## Wie die Systeme zusammenhaengen

```
PlayerMovement (Spieler-Steuerung)
    |
    +---> ObjectGrabSystem (am Spieler)
    |       - Raycast von der Kamera zum Erkennen von Objekten
    |       - Aendert Spieler-Speed je nach Gewicht des Objekts
    |       - Tag "Grabbable" = aufhebbar
    |
    +---> CarController (am Auto)
            - Spieler kommt nah ran -> E druecken -> einsteigen
            - PlayerMovement wird deaktiviert, Auto-Kamera aktiviert
            - Beim Aussteigen: Spieler an ExitPoint, alles zuruecksetzen
```

## Gewichts-Klassen (ObjectGrabSystem)

| Klasse | Masse | Effekt |
|--------|-------|--------|
| Leicht | < 5 kg | Schwebt vor Spieler, volle Geschwindigkeit |
| Mittel | 5-15 kg | 75% Speed, 70% Maus-Empfindlichkeit |
| Schwer | >= 15 kg | Wird am Boden geschleift, 25% Speed, 30% Maus |
| Zu schwer | > 30 kg | Kann nicht aufgehoben werden (rote Markierung) |

## Prefabs

| Name | Masse | Beschreibung |
|------|-------|-------------|
| `Light.prefab` | 3 kg | Leichte Box |
| `Medium.prefab` | 10 kg | Mittlere Box |
| `Heavy.prefab` | 16 kg | Schwere Box |

Alle Prefabs haben: BoxCollider, Rigidbody, MeshRenderer und den Tag `Grabbable`.

## Steuerung

- **WASD** - Laufen
- **Maus** - Umschauen
- **Leertaste** - Springen / Handbremse (im Auto)
- **E** - Objekt aufheben/ablegen / Auto ein-/aussteigen
- **Linke Maustaste** - Objekt werfen
- **Rechte Maustaste (halten)** - Objekt drehen
- **Mausrad** - Halte-Abstand aendern
- **ESC** - Maus-Cursor freigeben

## Szenen

- `Assets/Scenes/SampleScene.unity` - Hauptszene mit Spieler, Auto, Spawner und Spielfeld

## Entwicklungs-Phasen

| Phase | Was wird gebaut | Status |
|-------|----------------|--------|
| 1 | Auto, Trage-System, Boxen-System, Boxen-Spawner | Fertig |
| 2 | Geld-System, Haus-Erkennungssystem fuer Boxen | In Arbeit |
| 3 | Auftrags-System, Schaden-System | Geplant |
| 4 | Auto-Kauf-System | Geplant |
| 5 | Karte erweitern, Moebel erweitern | Geplant |

## Hinweise fuer Claude

- Alle Antworten, Kommentare und Erklaerungen bitte auf **Deutsch** schreiben
- Einfache, verstaendliche Sprache verwenden (Schulprojekt, Alter ~14)
- C#-Skripte folgen den Unity-Konventionen (MonoBehaviour, SerializeField, etc.)
- Physik ist zentral: Rigidbody, WheelCollider, Raycasts - nicht aendern ohne guten Grund
- Bei neuen Features immer pruefen, ob es schon ein bestehendes System gibt, das erweitert werden kann
- Multiplayer ist geplant (Paket ist installiert), aber noch nicht eingebaut
- Rendering: URP mit getrennten Render-Profilen fuer PC und Mobile
