# 🎲 Farkle (Mobile)

> A polished, push-your-luck dice game showcasing full-stack game development and backend analytics integration.

---

## 📖 Overview

**Farkle** is a casual dice game built for Android using Unity 6. Designed as a standout portfolio piece, this project highlights technical competence, visual polish, and backend integration using modern development practices.

---

## 🚀 Project Goals

- Showcase a full game development pipeline (design, implementation, polish)
- Implement comprehensive analytics and backend tracking for detailed player stats
- Integrate Google Play APIs for user authentication and cross-device tracking
- Create a visually appealing and engaging casual game suitable for a diverse audience

---

## 🎮 Gameplay

Players take turns rolling six dice, strategically selecting dice to keep for scoring. The objective is to reach 10,000 points first—but beware: if a roll yields no points (a "Farkle"), you lose all unbanked points for that turn!

**Turn Flow:**

1. Roll dice
2. Select scoring dice
3. Choose to roll again or bank points and end the turn
4. Avoid a Farkle to secure points
5. First to reach 10,000 triggers the final round
6. Highest scorer after the final round wins!

**Dice Scoring Rules:**
- Ones: 100 points each
- Fives: 50 points each
- Three-of-a-kind: face value x100 (1s = 1,000 points)
- Four-of-a-kind and higher: escalating multipliers
- Special combos (straight, three pairs, two triplets) award bonus points

---

## 🛠️ Feature Set

### ✅ Currently Implemented
- Core dice mechanics, animated rolls (using DOTween)
- Player scoring, turn management, game-end conditions
- Singleton and event-driven game architecture

### 🚧 In Progress
- Main Menu, Options, Pause and Game Over screens
- Detailed Game Settings interface
- Sound, music, and tutorial onboarding

### 🔥 Upcoming Features
- Backend analytics:
  - Game history, player stats, leaderboards
  - Persistent cross-device stats via Google Play
- AI opponents with adjustable difficulty
- Google Play Games authentication
- Visualized advanced player statistics

---

## 🖥️ Systems & Architecture

**Architecture:** Modular, singleton-driven, event-based, scriptable object-configured, REST API-backed, Google Play-integrated

**Core Components:**
- `FarkleGame`: Central gameplay coordinator
- `TurnManager`: Manages turn states via FSM
- `ScoreManager`: Handles all scoring logic and win conditions
- `DiceManager`: Manages dice state and scoring
- `PlayerManager`: Tracks players, profiles, and persistent stats
- `UIManager` & `ViewController`: Handles UI updates and animations
- `BackendService` (Planned): Syncs session data and retrieves player statistics via REST

**Data Persistence:**
- Local storage (`PlayerPrefs`)
- Backend storage (PostgreSQL)
- Google Play-linked profiles

---

## 🌐 Backend & Google Play Integration

- Backend RESTful service (FastAPI/Express)
- Persistent and comprehensive player stats
- Leaderboards sorted by multiple criteria (Wins, Average Score, Total Points)
- Secure Google Play Games integration for cross-device identity and profile management

---

## 📈 Portfolio Highlights

This project demonstrates expertise in:

- Full Unity development lifecycle
- UI/UX design and implementation
- Backend design, REST APIs, and analytics
- Integration with Google Play Services
- Animation and visual polish using DOTween

---

## 🧾 Installation & Setup

Clone the repository:

```bash
git clone https://github.com/b-hopper/farkle.git
```

Open in Unity (version 6+) and ensure Android build tools are installed. Follow the Unity build pipeline for Android deployment.

---

## 📜 License

This project is open-source and freely available for learning purposes. Attribution is appreciated!

---

Made with ❤️ for Game Dev Portfolio by **Brad Hopper**
