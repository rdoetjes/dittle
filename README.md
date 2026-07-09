# Dittle

A digital implementation of the strategy board game **Dittle**, built using C#, RayLib, and .NET 10. This project features a 3D-accurate dice simulation and a Minimax AI.

*Note: This is a quick and dirty evening project.*

## The Rules of Dittle

Dittle is a two-player strategy game played on a 7x7 board. The goal is to move all seven of your dice into your opponent’s base row and win by having the highest combined face value at the end.

### Setup
- Seven dice per player, placed in their respective base rows.
- All dice start with the **6** facing up and the **4** facing toward the center of the board.

### Movement
Movement is restricted to **forward** or **sideways** (no backward or diagonal moves).
- **Tilt**: Roll a die one square. The upward-facing number changes physically.
- **Jump**: Leap over one or more dice in a straight line (Vertical or Horizontal).
    - **Requirement**: To initiate a jump, you must jump over a die that is **immediately adjacent**.
    - **Requirement**: You cannot land on an occupied square or skip over multiple empty spaces (must land in the first available gap).
- **Tilt + Jump**: Tilt the die once, then perform a jump (Straight or L-shaped). The upward number changes once during the tilt, then remains constant during the jump phase.
- **L-shaped Jump**: A combination of a vertical jump and a horizontal jump. Both segments must jump over at least one die.
- **Forced forward move**: After 4 consecutive horizontal moves, you are forced to move forward if any forward move is possible.

### Winning
The game ends immediately when one player has **all seven dice** in the opponent’s base row.
- **Winner**: The player with the higher sum of upward-facing numbers in the goal row wins.
- **Tie-breaker**: If scores are equal, the player who reached the goal row with all 7 dice wins.

## How to Build and Run

### Prerequisites
- [.NET 10 SDK](https://dotnet.microsoft.com/download)
- Raylib dependencies (handled via NuGet)

### Build
```bash
cd dittle
dotnet build
```

### Run
To play against the AI (Player 1 is White, AI is Black):
```bash
dotnet run -- -players 1 -depth 4
```

To watch the AI play against itself (Simulation mode):
```bash
dotnet run -- -players 0 -depth 3
```

To play against another human:
```bash
dotnet run -- -players 2
```

### Arguments
- `-players [0|1|2]`: Set number of human players (0 for AI vs AI).
- `-depth [1-6]`: Set the AI's search depth (higher is harder).

## Controls
- **Left Click**: Select a die. Legal moves will be highlighted with blue circles showing the future face value.
- **Left Click (Target)**: Click a highlighted square to move.
- **RESTART Button**: Resets the game state.
- **AI DEPTH +/-**: Adjust the difficulty in real-time.

## Tests
To run the xUnit test suite for rule validation:
```bash
dotnet test DittleTests/DittleTests.csproj
```
