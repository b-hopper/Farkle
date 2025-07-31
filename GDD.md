# Game Design Document: Farkle Mobile

## Overview

**Title:** Farkle (Mobile)

**Platform:** Android (Unity 6)

**Genre:** Casual Dice Game

**Monetization:** None (Free, portfolio project)

**Developer:** Brad Hopper

**Primary Goals:**

- Showcase complete game development pipeline (design, engineering, polish)
- Implement full analytics and backend tracking for player stats
- Integrate Google Play APIs (including user authentication and tracking)
- Create a visually engaging, technically competent app for portfolio purposes

---

## Game Description

Farkle is a classic push-your-luck dice game. Players take turns rolling six dice and choosing which to keep for points. The goal is to reach a target score (default: 10,000 points). Players may continue rolling to accumulate more points during their turn — but risk losing everything if they Farkle (roll no scoring dice).

The game supports 2+ local players in a pass-and-play format, with persistent player profiles, animated dice rolls, and dynamic UI feedback.

---

## Core Gameplay

### Objective

Be the first player to reach the target score (default: 10,000 points).

### Turn Flow

1. Start turn → Roll all unheld dice
2. Select scoring dice
3. Choose to roll again or hold and end turn
4. If the roll results in no scoreable dice → Farkle → Lose unbanked points
5. Score carries over only if player banks points via "Hold Dice"
6. Once a player reaches the target score, all others get one final turn
7. Winner is the player with the highest score after final round

### Dice Scoring Rules

- Ones: 100 points each
- Fives: 50 points each
- Three of a kind: face value x100
  - (Three 1s = 1,000)
- Four of a kind: face value x200 (or 2,000 for 1s)
- Five of a kind: face value x300 (or 3,000 for 1s)
- Six of a kind: face value x400 (or 4,000 for 1s)
- Straight (1-6): 1,500 points
- Three pairs: 1,500 points
- Two triplets: 2,500 points

---

## Game Modes

### Local Multiplayer

- 2+ players
- Pass-and-play with screen flip animation between turns
- Each player uses a local profile (name, stats)

### AI Opponent (Bonus Feature)

- TBD — adjustable difficulty (conservative, aggressive, balanced)

---

## Feature Set

### ✅ Implemented

- Animated dice rolls using DOTween
- Player scoring and turn logic
- Game win condition and final round flow
- Bottom Feed mechanic
- Dynamic button states based on game phase
- Singleton architecture for managers (ScoreManager, TurnManager, DiceManager, etc.)

### 🟡 In Progress / Needs Completion

- Main Menu, Options Menu, and Pause Menu
- Game Over Screen with full scoreboard
- Game Settings UI (target score, scoring rules toggle)
- Sound and music integration
- Tutorial / onboarding

### 🔜 Planned Features (Portfolio Boost)

- 🔥 Backend stats tracking:
  - Wins, games played, avg score, Farkles, high turn
  - Send game session data to server on game end
  - Retrieve and display player stats
- 📊 Leaderboards:
  - Sortable by Wins, Avg Score, Total Points
  - Highlight current player
- 📅 Game History:
  - Timeline of past matches, scores, outcomes
- 🧠 AI opponent
- 🔓 Anonymous user identity via UUID or Google Play
- 🧾 **Google Play User Tracking:**
  - Use Google Play Games Services for login/auth
  - Associate player stats with authenticated Google user
  - Showcase integration with Google Play APIs (portfolio credential)

---

## Systems & Architecture

### Architecture Style

- Modular Singleton-based architecture for core systems (e.g., `TurnManager`, `ScoreManager`, `FarkleGame`, `UIManager`)
- Event-driven communication using `UnityEvent`s
- ScriptableObjects used for configuration (`GameSettings`, `PlayerSettings`)
- Backend communication via `UnityWebRequest` and JSON payloads
- Scene-based structure with additive loading for future menus and overlays

### Core Systems

#### `FarkleGame`

- Central event hub and game state coordinator
- Manages player input lock and main turn resolution loop
- Fires UnityEvents for all gameplay transitions (roll, select, hold, end turn, etc.)

#### `TurnManager`

- Finite State Machine handling turn phases: `START_TURN`, `ROLL_DICE`, `SELECT_DICE`, `END_TURN`, `GAME_OVER`, etc.
- Coordinates UI prompts and state transitions
- Determines final round and winner

#### `ScoreManager`

- Handles calculation and tracking of per-player scores
- Stores turn, selected, and total score
- Evaluates winning score condition and sets `BottomFeedScore`
- Pushes score data to backend when game ends

#### `DiceManager`

- Maintains state of six dice (rolled, held, selected)
- Implements scoring logic for all valid combinations
- Integrates with `ViewController` to animate dice updates

#### `PlayerManager`

- Manages list of `Player` objects (name, score, profile)
- Tracks current and next player based on `TurnManager`
- Handles persistent player stat storage (future: local and cloud sync)

#### `UIManager`

- Manages game UI, buttons, player panel, and splash messages
- Responds to turn state changes
- Controls screen flipping, debug info, and game over display

#### `ViewController`

- Maps dice model data (`Dice[]`) to visual prefab components (`DieView[]`)
- Handles animated dice rolling with variation via DOTween

### BackendService (Planned)

- Singleton MonoBehaviour for RESTful backend calls
- Posts JSON payloads at game end for session tracking
- Retrieves leaderboard, player profile, and stats history
- Integrates with Google Play Games user ID for identity/auth

### Google Play Integration

- Uses Google Play Games SDK to authenticate user
- Maps player profile to authenticated Google user ID
- Stores and retrieves cross-device stats securely
- Future: unlock achievements, sync cloud saves

### Data Flow (Simplified)

```
[User] → [Unity UI] → [FarkleGame] → [TurnManager / DiceManager / ScoreManager]
                                  ↓
                           [UIManager / ViewController]
                                  ↓
                            [GameOver] → [BackendService] → [Web API]
                                                     ↑
                                            [Google Play Auth]
```

### Persistence

- Player profile saved locally in `PlayerPrefs` or file
- Extended stats and match history pushed to backend via REST
- Backend stores game results in relational DB (e.g., PostgreSQL)
- Support for anonymous ID fallback or full Google Play account linkage..

