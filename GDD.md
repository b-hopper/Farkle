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

### 🟡 In Progress

- Main Menu, Options Menu, and Pause Menu
- Game Over Screen with full scoreboard
- Game Settings UI (target score, scoring rules toggle)
- Sound and music integration
- Tutorial / onboarding

### 🔜 Planned Features

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

## 🛡️ Backend Architecture

### 🔧 Backend Overview

The backend handles persistent player data for statistics, game history, and leaderboards. While gameplay is limited to local pass-and-play, each user (Google account or anonymous ID) can maintain multiple local player profiles. These profiles track individual stats and are tied to a single user account.

The backend system is built around a RESTful API with a PostgreSQL database. Communication between the Unity client and the server is done via JSON over HTTPS.

---

### 👥 User and Player Model

- **User**
  - Represents the logged-in Google account or an anonymous installation
  - Identified by Google ID or locally-generated UUID
  - Can manage multiple `PlayerProfiles`

- **PlayerProfile**
  - Represents a single local player (e.g., “Dad”, “Friend 2”)
  - Stores persistent stats like score history, win count, and Farkles
  - Associated with a single User

---

### 🗓️ Data Models

#### `Users` Table

| Field      | Type              | Description                         |
|-----------|-------------------|-------------------------------------|
| user_id   | UUID / Google ID  | Primary key                         |
| login_type| string            | `"google"` or `"anonymous"`         |
| created_at| timestamp         | Timestamp when user was registered  |

#### `PlayerProfiles` Table

| Field        | Type   | Description                              |
|--------------|--------|------------------------------------------|
| player_id    | UUID   | Primary key                              |
| user_id      | FK     | References a `Users` row                 |
| display_name | string | Name of the local player                 |
| created_at   | timestamp | Timestamp of creation                |

#### `Games` Table

| Field     | Type     | Description                      |
|-----------|----------|----------------------------------|
| game_id   | UUID     | Unique game session ID           |
| user_id   | FK       | User who played the session      |
| played_at | timestamp| When the game was completed      |

#### `GameResults` Table

| Field      | Type     | Description                        |
|------------|----------|------------------------------------|
| result_id  | UUID     | Primary key                        |
| game_id    | FK       | References a `Games` row           |
| player_id  | FK       | References a `PlayerProfiles` row  |
| score      | int      | Final score for the player         |
| turns_taken| int      | Number of turns taken              |
| farkles    | int      | Number of Farkles                  |
| won        | bool     | True if this player won            |

---

### 🔌 API Endpoints

#### `POST /game-result`
Submits a completed match with player results.

```json
{
  "user_id": "abc123",
  "game": {
    "played_at": "2025-08-05T12:00:00Z",
    "results": [
      {
        "player_id": "p1",
        "score": 9800,
        "turns": 8,
        "farkles": 2,
        "won": true
      },
      {
        "player_id": "p2",
        "score": 8450,
        "turns": 9,
        "farkles": 3,
        "won": false
      }
    ]
  }
}
```

#### `GET /player-stats?player_id=p1`
Returns persistent stats for a single player profile.

#### `GET /leaderboard?sort=avg_score`
Returns a leaderboard sorted by `avg_score`, `wins`, or `total_points`.

#### `GET /user-players?user_id=abc123`
Returns all `PlayerProfiles` associated with the user.

#### `POST /create-player`
Creates a new player profile under a user account.

```json
{
  "user_id": "abc123",
  "display_name": "Grandma"
}
```

---

### 🔐 Identity Management

- Uses **Google Play Games Services** if available
- Falls back to a locally-stored **UUID** for anonymous users
- All data is tied to the `user_id` (either UUID or Google ID)
- No personally identifiable info (PII) is stored
- The Unity client is responsible for persisting and reusing the `user_id`

---

### ⚖️ Security and Best Practices

- All backend requests require HTTPS
- Input validation and schema checks are applied to all endpoints
- Rate limiting per `user_id` to prevent abuse
- All UUIDs are generated on the client or server using secure random generators
- Sensitive operations (e.g., deleting players) require additional verification tokens

---

### 🌐 Hosting Considerations

The backend can be deployed to a lightweight PaaS such as:
- **Render** or **Railway** for Node.js/FastAPI
- **Google Cloud Run** if leveraging Google Auth
- **Supabase** or **PocketBase** for integrated auth + DB as a quick backend

Database hosting can use:
- **Supabase PostgreSQL**
- **Neon.tech**
- **ElephantSQL**

---

### ☁️ Data Flow

```
[Game Ends]
   ↓
[Unity BackendService builds payload]
   ↓
[POST /game-result]
   ↓
[Backend stores session + results]
   ↓
[Client fetches player stats, history, and leaderboards]
```

---

### 🔮 Future Extensions

- Add `achievements` table for milestone tracking
- Implement JWT-based sessions for Google users
- Add `/delete-player` or `/rename-player` endpoints
- Add filtering to game history (`GET /player-history?player_id=p1&page=2`)
- Add weekly/monthly leaderboards with `created_at` filtering

