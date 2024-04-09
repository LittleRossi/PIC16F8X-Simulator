# PIC16F8X-Simulator

## Hintergrund
Dieses Repo beinhaltet einen PIC16F8X Simulator in c#. Dieser wurde im Rahmen der Vorlesung Systemnahe Programmierung 2 an der DHBW in Karlsruhe im Studiengang Informationstechnik im 4. Semester erstellt.

Der Verantwortliche Dozent ist: Lehmann, Stefan, Dipl.-Ing. (FH)

## Einleitung Simulator
Ein Simulator ist eine präzise Nachbildung, die eine möglichst realistische Umgebung schafft. Dadurch können sowohl grundlegende als auch spezielle Szenarien ohne den Einsatz des Originals simuliert werden. Der Simulator bietet die Möglichkeit, die Bedingungen kontrolliert zu variieren, um spezifische Situationen darzustellen. Dies ermöglicht detaillierte Studien zur Funktionsweise und anderen Aspekten. Simulatoren sind auch hilfreich für Tests und Lernzwecke. Allerdings ist eine Simulation oft eine vereinfachte Darstellung der Realität und kann diese nicht vollständig erfassen. Die Entwicklung von Simulatoren wird zunehmend komplexer, insbesondere bei komplexen Szenarien.

In einem Projekt wurde ein Simulator für den Microcontroller PIC16F84 in C# erstellt. Dabei war die Wahl der Programmiersprache frei. Obwohl nicht alle Funktionen des PIC16F84 im Simulator implementiert wurden, können doch alle grundlegenden Funktionen simuliert werden. Alle Instruktionen sind unterstützt, sodass einfache Programme ohne Abweichungen ausgeführt werden können.


## Funktionen der Oberfläche
Die Benutzeroberfläche ermöglicht eine detaillierte Überwachung des Microcontroller-Ablaufs. Hier können Befehle manuell ausgeführt und Register sowie Flags manipuliert werden.

Nach dem Starten der Anwendung muss zunächst ein Programm im LST-Format ausgewählt werden. Der File-Reader interpretiert dieses Programm, und der Simulator lädt einen neuen PIC mit dem Programm in den Speicher. Anschließend können Einstellungen wie die Quarzfrequenz angepasst werden. Der PIC befindet sich noch im Reset-Modus und wartet darauf, diesen sowie das zugehörige NOP auszuführen. Danach kann der Simulator mit Einzel-, N-Schritt- oder automatischen Modus gesteuert werden.