# CLAUDE.md

Diese Datei hilft Claude Code (claude.ai/code) beim Arbeiten mit diesem Projekt.

## Projekt-Beschreibung

**GMA** ist ein Koop/Multiplayer-Spiel in Unity. Man leitet eine Umzugsfirma mit Freunden, nimmt Auftraege an, transportiert Moebel durch eine grosse Stadt und verdient Geld fuer Upgrades und neue Fahrzeuge. Schwere Moebel brauchen Teamarbeit, und beim Transport muss man aufpassen - Schaeden am Mobiliar werden vom Lohn abgezogen.

Dies ist ein Schulprojekt. Alle Dokumentation und Kommentare im Code sind auf **Deutsch**.

## Unity-Projekt

- **Engine:** Unity (C# Scripts)
- **Sprache:** Deutsch (Code-Kommentare, UI-Texte, Docs)
- Typische Unity-Ordnerstruktur: `Assets/`, `Packages/`, `ProjectSettings/`

## Entwicklungs-Phasen (Planung)

| Phase | Was wird gebaut |
|-------|----------------|
| 1 | Auto, Trage-System, Boxen-System, Boxen-Spawner |
| 2 | Geld-System, Haus-Erkennungssystem fuer Boxen |
| 3 | Auftrags-System, Schaden-System |
| 4 | Auto-Kauf-System |
| 5 | Karte erweitern, Moebel erweitern |

## Wichtige Spielsysteme

- **Trage-System:** Schwere Moebel brauchen mehrere Spieler
- **Box/Moebel-Physik:** Moebel muessen passend ins Fahrzeug gepackt werden, sonst fallen sie raus
- **Schadens-System:** Aufpralle mindern den Wert der Objekte, wird vom Lohn abgezogen
- **Auftrags-System:** Auftraege werden ueber einen Ingame-PC angenommen
- **Geld/Upgrade-System:** Verdientes Geld fuer Fahrzeuge und Upgrades ausgeben
- **Welten-System:** Mit genug Geld kann man in eine neue Stadt ziehen

## Hinweise fuer Claude

- Alle Antworten, Kommentare und Erklaerungen bitte auf **Deutsch** schreiben
- Einfache, verstaendliche Sprache verwenden (Schulprojekt, Alter ~14)
- C#-Skripte folgen den Unity-Konventionen (MonoBehaviour, SerializeField, etc.)
- Bei neuen Features immer pruefen, ob es schon ein bestehendes System gibt, das erweitert werden kann
