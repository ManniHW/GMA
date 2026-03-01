# 🚚 GMA - Umzugsfirma Simulator

Man leitet eine Umzugsfirma mit seinen Freunden und muss Haeuser aus- und einraeumen in einer grossen Stadt.

**Genre:** 🎮 Koop / Multiplayer

## 🌆 Die Spielwelt

- Eine grosse Stadt mit verschiedenen Vierteln
- Man muss zu Orten hinfahren, um Upgrades zu kaufen

## 🔄 Der Gameplay Loop

1. Man nimmt Auftraege via PC ingame an
2. Schwere Moebel zu tragen erfordert Teamarbeit
3. Die Moebel muessen passend in die Fahrzeuge geraeumt werden, da sie sonst rausfallen koennen
4. Man muss aufpassen, wie man faehrt, da es Hindernisse auf der Strasse gibt
5. Die Moebel muessen nach dem Ausraeumen ins neue Haus geraeumt werden
6. Vom Geld, das man bekommt, kann man sich Upgrades oder Fahrzeuge kaufen

## 💥 Schadens-System

Starke Aufpraelle koennen den Wert von Objekten mindern, was vom Lohn am Ende abgezogen wird.

## 🌍 Welten-System

Wenn man genuegend Geld angesammelt hat, kann man in eine andere Stadt ziehen und andere Auftraege annehmen.

---

## 📋 Planung

### 🚗 Phase 1 - Grundmechaniken

**Auto:**
- [x] 🏎️ Grundfahrphysik (Gas, Bremse, Lenken, Handbremse)
- [x] 🚪 Ein-/Aussteigen mit Kamera-Wechsel
- [x] 🕐 Tacho-Anzeige (km/h)
- [ ] 📦 Ladeflaeche: Boxen physik-basiert ins Auto laden/rausfallen lassen
- [ ] ⬅️ Rueckwaertsgang-Anzeige im Tacho ("R" bei negativer Geschwindigkeit)

**Trage-System:**
- [x] ✋ Aufheben, Halten, Werfen mit E / Linksklick
- [x] ⚖️ Gewichts-Klassen (Leicht/Mittel/Schwer/Zu schwer)
- [x] 🐌 Speed- und Maus-Abzug je nach Gewicht
- [x] 💡 Highlight-System (weiss = hebbar, rot = zu schwer)
- [x] 🪨 Schwere Objekte schleifen ueber den Boden
- [ ] 🧱 Gehaltene Objekte gleiten nicht durch Waende (Kollisions-Fix)

**Boxen & Spawner:**
- [x] 📦 Drei Box-Typen: Light (3kg), Medium (10kg), Heavy (16kg)
- [x] 🎲 Spawner mit zufaelliger Position, Radius und Boden-Erkennung
- [ ] 💰 Wert-Eigenschaft auf Boxen (Vorbereitung fuer Schadens-System)

**Spieler:**
- [x] 🏃 WASD-Bewegung + Maus-Steuerung
- [x] 🦘 Springen
- [ ] 🏃‍♂️ Sprint (Shift = schneller laufen)

### 💰 Phase 2 - Geld & Haeuser

- [ ] 🪙 Geld-Anzeige im UI (einfacher Zaehler)
- [ ] 🏠 Haeuser mit Lade-/Entladezonen (Trigger-Bereiche)
- [ ] ✅ Boxen-Zaehler: erkennt ob alle Boxen abgeliefert wurden
- [ ] 🧾 Auftrag abschliessen = Geld bekommen
- [ ] 🎯 Erster Test-Auftrag (ein Haus → anderes Haus)

### 📋 Phase 3 - Auftraege & Schaden

- [ ] 💻 Ingame-PC zum Auftraege annehmen
- [ ] 📝 Auftrags-Liste mit verschiedenen Schwierigkeiten
- [ ] 💥 Schadens-Berechnung bei Aufprall (mindert Box-Wert)
- [ ] 📊 Lohn-Abrechnung am Ende (Basis-Lohn minus Schaden)

### 🚙 Phase 4 - Auto-Kauf

- [ ] 🏪 Auto-Haendler (Ort auf der Karte)
- [ ] 🚐 Verschiedene Fahrzeuge (klein/mittel/gross, unterschiedliche Ladeflaeche)
- [ ] 💸 Kaufen mit verdientem Geld

### 🌟 Phase 5 - Erweiterung & Polish

- [ ] 🗺️ Karte erweitern (mehr Viertel, mehr Haeuser)
- [ ] 🛋️ Mehr Moebel-Typen (Sofa, Schrank, Kuehlschrank...)
- [ ] 🔊 Sound-Effekte (Motor, Aufheben, Werfen, Kollision)
- [ ] 🎵 Hintergrundmusik
- [ ] 👥 Koop-Tragen (zwei Spieler tragen ein schweres Objekt zusammen)
