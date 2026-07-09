## The Rules of Dittle

Dittle is a two-player strategy game played on a 7x7 board. The goal is to move all seven of your dice into your opponent’s base row and win by having the highest combined face value at the end.

### Setup
- Seven dice per player, placed in their respective base rows.
- All dice start with the **6** facing up and the **4** facing toward the center of the board.

### Movement
Movement is restricted to **forward** or **sideways** (no backward or diagonal moves).
- **Tilt**: Roll a die one square. The upward-facing number changes physically.
- **Jump**: Leap over one or more dice in a straight line (Vertical or Horizontal).
    - **Requirement**: You must start by jumping over a die that is **immediately adjacent**.
    - **Landing**: You must land in an empty square. If there are multiple dice in a row with empty spaces between them, you can continue jumping over them in the same move (e.g., Die-Gap-Die-Gap).
    - **Gap Rule**: You cannot jump over two or more dice that are packed tightly together (Die-Die-Gap is illegal). There must be at least one empty space between each die being jumped.
- **Tilt + Jump**: Tilt the die once, then perform a jump (Straight or L-shaped). The upward number changes once during the tilt, then remains constant during the jump phase.
- **L-shaped Jump**: A combination of a vertical jump and a horizontal jump. Both segments must jump over at least one die, separated by a gap (e.g., Jump over a die to a gap, then jump over another die horizontally to a final gap).

### Jump Examples
`.` = Empty square, `X` = Other Die, `D` = Your Die, `L` = Legal Landing

**Legal Multi-Jump:**
`D X L X L`
(One or two jumps possible, landing in either `L`)

**Illegal Tight Cluster:**
`D X X L` 
(Illegal: Cannot jump two dice that are touching)

**Illegal Gap Start:**
`D . X L`
(Illegal: Must be immediately adjacent to `X` to start a jump)

**Legal L-Jump:**
```
. . L
. . X
D X .
```
(Jump Up over X to the gap, then jump Right over X to landing `L`)
- **Forced forward move**: After 4 consecutive horizontal moves, you are forced to move forward if any forward move is possible.

### Winning
The game ends immediately when one player has **all seven dice** in the opponent’s base row.
- **Winner**: The player with the higher sum of upward-facing numbers in the goal row wins.
- **Tie-breaker**: If scores are equal, the player who reached the goal row with all 7 dice wins.
