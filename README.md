# Oráculo + DYNASTY 🎮⚙️

This repository contains the development of two interconnected projects built individually using **Godot Engine**: 

1. **Oráculo**: A custom desktop tool designed for data management and workflow automation.
2. **DYNASTY**: A text-based narrative RPG featuring deep decision-making mechanics and complex branching storylines, inspired by titles like *The Life and Suffering of Sir Brante*.

---

## 🛠️ Oráculo: Automation & Data Design Tool

**Oráculo** was born out of the need to optimize the workflow of narrative-driven game development. Creating hundreds of events, characters, and relationship dependencies manually within the engine can severely slow down production. This tool solves that bottleneck by providing a clean, dedicated user interface built specifically for visual content creation.

### Key Features:
* **Entity Management:** A visual dashboard to create, edit, and catalog characters, factions, and world states.
* **Relationship Network:** A structured system to establish dynamic interactions between characters and events.
* **Hybrid Data Exporting:** 
  * Generates **JSON** files for clean structuring, serialization, and universal data portability.
  * Exports to native **Godot Resources (`.tres`)**, allowing the game to load data directly and efficiently, integrating seamlessly with the engine's core architecture.
* **Built with Godot:** Developed using Godot's UI and Control Nodes system, showcasing the engine's versatility for standalone desktop software (*Tools*).

---

## 📖 DYNASTY: Narrative RPG

A video game where the player's choices shape the history of the world. The game engine reads the structured database previously generated in **Oráculo** to build logic, branching paths, and consequences in real-time.

### Technical Specifications:
* **Logic in C# (.NET):** A robust, strongly-typed, object-oriented architecture designed to handle massive choice trees and complex data structures within Godot.
* **Secure Data Architecture:** Total separation between narrative data (generated in the tool) and core gameplay logic, ensuring scalability and easy maintenance.

---

## 🚀 Technologies & Applied Skills
* **Engine:** Godot Engine (C# / .NET).
* **Data Formats:** JSON and Godot Resources (`.tres`).
* **Design Patterns:** Object-Oriented Programming (OOP), code decoupling, dependency injection, and complex data stream management.
* **Engineering Mindset:** Tooling development focused on solving production bottlenecks.

---

## 📄 License

This project is licensed under the **MIT License** - see the [LICENSE](LICENSE) file for details.
---
---
# Oráculo + DYNASTY 🎮⚙️

Este repositorio contiene el desarrollo de dos proyectos interconectados realizados de forma individual utilizando **Godot Engine**: 

1. **Oráculo**: Una herramienta de escritorio personalizada para la gestión y automatización de datos de diseño.
2. **DYNASTY**: Un RPG narrativo de texto con mecánicas de toma de decisiones y ramificaciones complejas, inspirado en obras como *The Life and Suffering of Sir Brante*.

---

## 🛠️ Oráculo: Software de Automatización y Diseño de Datos

**Oráculo** nace de la necesidad de optimizar el flujo de trabajo del desarrollo de videojuegos narrativos. Crear cientos de eventos, personajes y dependencias de relaciones de forma manual directamente en el motor ralentiza la producción. Esta herramienta resuelve ese problema ofreciendo una interfaz de usuario limpia y dedicada a la creación de contenido de forma visual.

### Características Clave:
* **Gestión de Entidades:** Panel visual para crear, editar y catalogar personajes, facciones y estados del mundo.
* **Red de Relaciones:** Sistema estructurado para establecer cómo interactúan los personajes y eventos entre sí.
* **Exportación de Datos Híbrida:** 
  * Generación de archivos **JSON** para una estructuración limpia, serialización y portabilidad de datos universales.
  * Exportación a **Recursos nativos de Godot (`.tres`)**, permitiendo que el videojuego cargue los datos de manera directa y eficiente, integrándose a la perfección con la arquitectura del motor.
* **Desarrollo en Godot:** Desarrollado utilizando el sistema de UI y Control Nodes de Godot, demostrando la versatilidad del motor para herramientas de software clásico (*Tools*).

---

## 📖 DYNASTY: RPG Narrativo

Un videojuego donde las elecciones del jugador dan forma a la historia. El motor del juego lee la base de datos estructurada y generada previamente en **Oráculo** para construir la lógica y las consecuencias en tiempo real.

### Características Técnicas:
* **Lógica en C# (.NET):** Arquitectura de código robusta, fuertemente tipada y orientada a objetos para manejar árboles de decisiones masivos y estructuración de datos compleja en Godot.
* **Arquitectura de Datos Segura:** Separación total entre los datos narrativos (generados en la herramienta) y la lógica de ejecución del juego, facilitando la escalabilidad y el mantenimiento del proyecto.

---

## 🚀 Tecnologías y Habilidades Aplicadas
* **Motor:** Godot Engine (C# / .NET).
* **Formatos de Datos:** JSON y Recursos de Godot (`.tres`).
* **Patrones de Diseño:** Programación orientada a objetos (POO), desacoplamiento de código, inyección de dependencias y gestión de flujos de datos complejos.
* **Enfoque de Ingeniería:** Creación de herramientas (*tooling*) para resolver cuellos de botella en la producción.

---

## 📄 Licencia

Este proyecto está bajo la **Licencia MIT** - Consulta el archivo [LICENSE](LICENSE) para más detalles.
