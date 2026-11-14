# Game Brief: Twofold

## In your words
create me a non-eculidean games where two lifes are merged a colorful life with red life and u can teleport like a portal between the lifes and with some simple puzzles u decide what to go with.

1–7. you pick

**Pitch:** One level exists twice, a colourful life and a red life laid over each other, and you step through portals between them to walk around what blocks you in the other.

**Genre:** puzzle-platformer

**Controls:** A/D or arrow keys to walk, W/Up/Space to jump, E to pick up or put down a block, Z to undo, R to restart. Walking into a portal takes you through. *(guessed)*

**Question:** Is crossing between two overlaid worlds to solve a room satisfying for ten minutes? *(guessed)*

**Hooks:** Portals show the other life through them before you step in. The same spot is a wall in one life and open air in the other, and what you carry comes with you.

**Intent:** soft or no failure *(provisional)*

## Look and references
- Look: *(provisional)* clean, minimal, orthographic shapes.
  - Mood: a quiet place with a second, wrong version of itself underneath.
  - Shapes: blocky, square world; a small rounded figure with legs and a scarf that trails.
  - Values: player and portals brightest; backgrounds held in a narrow range.
  - Palette: the colourful life has a pale sky, teal ground, yellow, blue and green switches and doors, and a white player. The red life is the same room in crimson ground on near-black with the player in pale pink, and switches keep their shape and pattern so they still read. Shadows cooler in the colourful life, warmer in the red.
  - Detail: on portals, switches and the player; none behind play.
  - Rules: flat fills, one dark outline weight, no gradients except the portal glow.
  - Signature: every portal is a window. Inside its frame you see the other life live, and the scarf changes colour as it passes through first.
  - Every figure drawn, no emoji, no plain circles.
- References: none named.
- View: 2D · Camera: side view; follows the player, still when the level fits the window. *(provisional)*

## First minute
You stand in a bright room with an exit door in sight and a wall too tall to jump in the way. Beside you is a glowing frame, and through it the same room shows in red, with no wall. You walk in, the world turns red, you walk past where the wall was, step through a second frame, and you are back in colour on the far side. The exit lights. Level 2 opens with a block and a switch.

## Challenge
*(provisional)* Level 3, around minute three, brings the first real puzzle. A switch in the colourful life needs a block that exists only in the red one, and the obvious portal puts it on the wrong side of a door. No death and nothing lost: undo takes back any move and restart resets the room. Levels 4 and 5 chain two crossings and combine the tricks already learned.

## Core loop
- Read the room in both lives through the portals.
- Cross, move a block or press a switch, and see what changed in the other life.
- The exit lights when the way is open; walk to it for the next room.
- A wrong idea costs one undo.

## Goal & fail state
- Reach the exit door of each of five short levels; after the fifth, an ending screen. *(provisional)*
- No fail state: undo and restart, and a quiet restart prompt if a room can no longer be solved. *(provisional)*

## Features
- [ ] Two overlaid lives per level, with different walls, gaps and doors
- [x] Portals you walk through, showing the other life inside their frame
- [x] Blocks you carry through portals
- [x] Switches and doors that share a colour and a mark, with the link animated
- [x] Five levels, one idea each: shown safe, then used, then twisted
- [x] Unlimited undo and a restart key, both taught on the title
- [ ] Forgiving jumps that can never skip a puzzle
- [ ] A clear sound and the exit lighting the moment a room is solved

## Your systems
- teleport like a portal between the lifes · in the slice
- simple puzzles · in the slice

## Keep
-

## Avoid
-

## Out of scope
- Hints that nudge without giving the answer: change it after play
- Hidden extras behind harder optional puzzles: change it after play
- A speedrun timer: change it after play
- Two-player co-op: change it after play
- A story told through the levels: grow it
- Many worlds of hand-made levels: grow it

## Build order
1. The slice: five levels from title to ending, with portals, carried blocks, switches and doors, undo and restart.
2. Hints and optional harder rooms.
3. More worlds and a story.

---
**Kill condition:** if crossing between lives still feels like a plain door after two tuning rounds. *(guessed)*

**Who it's for / what they feel:** solo puzzle fans who want a calm ten minutes and a few "oh!" moments. *(guessed)*

**Games you love / don't:** none named.
