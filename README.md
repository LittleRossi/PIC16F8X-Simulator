# PIC16F8X-Simulator

## Hintergrund

Dieses Repo beinhaltet einen PIC16F8X Simulator in c#. Dieser wurde im Rahmen der Vorlesung Systemnahe Programmierung 2 an der DHBW in Karlsruhe im Studiengang Informationstechnik im 4. Semester erstellt.

Der Verantwortliche Dozent ist: Lehmann, Stefan, Dipl.-Ing. (FH)

## Einleitung Simulator

Ein Simulator ist eine präzise Nachbildung, die eine möglichst realistische Umgebung schafft. Dadurch können sowohl grundlegende als auch spezielle Szenarien ohne den Einsatz des Originals simuliert werden. Der Simulator bietet die Möglichkeit, die Bedingungen kontrolliert zu variieren, um spezifische Situationen darzustellen. Dies ermöglicht detaillierte Studien zur Funktionsweise und anderen Aspekten. Simulatoren sind auch hilfreich für Tests und Lernzwecke. Allerdings ist eine Simulation oft eine vereinfachte Darstellung der Realität und kann diese nicht vollständig erfassen. Die Entwicklung von Simulatoren wird zunehmend komplexer, insbesondere bei komplexen Szenarien.

In einem Projekt wurde ein Simulator für den Microcontroller PIC16F84 in C# erstellt. Dabei war die Wahl der Programmiersprache frei. Obwohl nicht alle Funktionen des PIC16F84 im Simulator implementiert wurden, können doch alle grundlegenden Funktionen simuliert werden. Alle Instruktionen sind unterstützt, sodass einfache Programme ohne Abweichungen ausgeführt werden können.

## Dokumentation

Für dieses Projekt wurde eine umfangreiche Dokumentation erstellt. Diese Dokumentation liegt im Unterorder _Dokumentation_

## Benutzeroberfläche

![Bild des Simulators](/Dokumentation/Simulator-UI.png)

## Testdateien

Im Unterordner _Testprogramme_ befinden sich alle assemblierten Programme, mit denen die unterschiedliche Funktionalitäten des Simulators getestet werden.

## Know Issues

Beim EEPROM funktioniert das Lesen aus dem EEPROM in die Register nicht. Ansonsten sind alle Funktionen voll funktionsfähig implementiert.
