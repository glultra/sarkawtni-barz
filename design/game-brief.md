# Game Brief: Seamwalk

## In your words
i played it, the mouse look feels not good the 2.5D was good since u just converted from 2D to 3D well that's why it is feels boring in 3D ! so i recommend we start a real new prototype fully in 3D with the idea ! and add a bit more non-ecludiean ideas to it !

1. View: i prefer going with first person camera in 3D like viewfinder and superliminial games ! others u decide

**Pitch:** You walk in first person through a small garden that exists twice, a colourful life and a red life, where arches are holes into the other life and the space itself does not add up.

**Genre:** first-person 3D puzzle

**Controls:** W A S D to walk, the mouse to look (click the game to capture it), E to pick up a cube or put it down, R to restart the space, P to pause, M to mute, Esc to free the mouse. No jump. *(guessed)*

**Question:** Is walking through impossible spaces between two lives fun for ten minutes in first person? *(guessed)*

**Hooks:** Every arch is a real hole: you see the other life through it from any angle and walk through with no cut, no flash and no loading. The space lies: a shed holds a hall, and a path round a corner does not come back to where it began.

**Intent:** soft or no failure *(provisional)*

## Look and references
- Look: *(provisional)*
  - Mood: a sunny toy garden with a red twin behind every arch; calm, a little uncanny.
  - Shapes: chunky rounded low-poly, built at eye level: hedges, walls and doorways taller than you, small sheds, stairs and a bridge.
  - Values: cubes, switches and arches are the brightest things; walls and ground stay quieter.
  - Palette: the colourful life has green grass, cream walls with pink tops, blue sky and blossom trees. The red life is the same place in crimson with dark walls, a deep red sky, crystals and mushrooms. Cubes are orange in both. A switch and its door share one colour and one mark.
  - Detail: on arches, cubes, switches and doors; scenery stays off the paths.
  - Signature: the arch's glowing rim, with the other life live inside it.
  - Every figure modelled, no emoji, no plain circles.
- References: Viewfinder and Superliminal (the first-person view, clean readable spaces, and surprise from space that does not behave).
- View: 3D · Camera: first person at eye height, mouse look, smoothed.

## First minute
You stand on a garden path facing a glowing arch, with red ground showing through it. You walk through and you are in the red life with no cut; looking back, the green garden is inside the arch. A wall blocks the path in red but not in green, so you cross back at a second arch and pass it. Then a tiny shed with one door: you step in and it is a long hall, far bigger than the shed. The exit gate at its end lights when you reach it.

## Challenge
*(provisional)* The first real puzzle is the third space, around minute three. A switch in the colourful life needs a cube that exists only in the red life, and the hedge path round a pillar leads somewhere new the second time round, so the short way is not the way. Nothing kills you and nothing is lost: R restarts the space, and a cube that falls out of reach returns to its start. Spaces four and five combine the bigger-inside room, the changing path, and a door that leads to a different place from each side.

## Core loop
- Look through arches and doors to read where they really lead.
- Walk through, carry a cube or press a switch, and see what opened in the other life.
- The gate lights when the way is open; walk through it to the next space.
- A wrong idea costs a short walk or one restart.

## Goal & fail state
- Reach the gate of five short spaces; after the fifth, an ending screen. *(provisional)*
- No fail state: restart with R, and cubes return when lost. *(provisional)*

## Features
- [x] Two lives of one place, colourful and red, with different walls and doors
- [x] Arches you see through and walk through with no cut
- [x] A room bigger inside than outside
- [x] A path round a corner that leads somewhere new the second time
- [x] A door that leads to a different place from each side
- [x] Cubes you carry through arches onto switches that open doors
- [x] Five spaces, one idea each: shown safe, then used, then twisted
- [ ] A clear sound and the gate lighting the moment a space is solved

## Your systems
- teleport like a portal between the lifes · in the slice
- a bit more non-ecludiean ideas · in the slice
- simple puzzles · in the slice *(guessed)*

## Keep
-

## Avoid
- the mouse look feels not good
- just converted from 2D to 3D: this one is built in first person from the first line
- Viewfinder's and Superliminal's names, art, characters and levels

## Out of scope
- An arch that makes you tiny or huge: change it after play
- Stairs that loop forever: change it after play
- Hints that nudge without giving the answer: change it after play
- A story told through the spaces: grow it
- Many worlds of hand-made spaces: grow it

## Build order
1. The slice: five spaces from title to ending, with see-through arches, the two lives, the three impossible-space tricks, cubes, switches and doors, and restart.
2. The scale arch, looping stairs and hints.
3. More worlds and a story.

---
**Kill condition:** if walking through an arch still feels like a plain door after two tuning rounds. *(guessed)*

**Who it's for / what they feel:** solo puzzle fans who want a calm ten minutes and a few "wait, what?" moments. *(guessed)*

**Games you love / don't:** Love Viewfinder and Superliminal (the first-person view).
