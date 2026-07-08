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
- **Jump**: Leap over one or more dice in a straight line.
    - **Requirement**: You must be immediately adjacent to a die to jump it.
    - **Requirement**: There must be at least one empty space between each die being jumped.
- **Tilt + Jump**: Tilt the die once, then perform a jump (Straight or L-shaped). The upward number remains constant during the jump phase.

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

To play against another human:
```bash
dotnet run -- -players 2
```

### Arguments
- `-players [1|2]`: Set number of human players.
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
