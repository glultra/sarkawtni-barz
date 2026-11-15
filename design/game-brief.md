# Game Brief: Long Climb

## In your words
i am currenlty streaming in tiktok i want a challenging game which may attract more viewers please do u think of creating a game for me for that ? or maybe  a simple game for 1v1 so i say who wins against me i give a gift smth like that orr just leaning to a solo play and viewer would love to see it from my while streaming ! but the game or the idea shouldn't be short right ? otherwise the the viewers won't love it

before u choose which is demanding more u think ? especially in kurdish

well no idk idk anyy !! i just need a game for streaming in tiktok so duel maybe not makes sense does it ? gifts i say i give him with fib or smth else soo maybe we think of the game long and enjoyable parkour or smth like that what do u say ?

Pitch 1 (The Long Climb): you pick

**Pitch:** One tall tower, hard but fair, where a slip drops you back down; you climb it live while viewers race the same tower on their phones for your prize.

**Genre:** precision platformer, one long climb

**Controls:** A and D or the arrows to move, Space or W to jump (hold for a higher jump), R to start the climb again, P to pause, M to mute. On a phone: two arrow buttons on the left and a jump button on the right. *(guessed)*

**Question:** Is one long, punishing climb fun to play and to watch for an hour? *(guessed)*

**Hooks:** A fall is never a reset: you drop until something below catches you, so every metre is at risk and viewers watch the height bar. Finishing gives a code with your time, so a viewer can prove they beat you.

**Intent:** soft or no failure *(provisional)*

## Look and references
- Look: *(provisional)*
  - Mood: a bright toy tower rising out of a garden into the clouds, the sunset, the stars; cheerful, with real tension.
  - Camera and scale: side view, tall 9:16 screen for TikTok, following the climber with more view above than below.
  - Shapes: chunky and rounded; a small round climber with a red scarf; platforms with thick outlines.
  - Values: the climber and what you can stand on are the brightest things; the sky behind stays soft.
  - Palette: the sky changes every zone (green garden, white clouds, orange sunset, deep blue night, starry top), so height reads at a glance. Hazards are red; safe rest floors are gold.
  - Detail: on the climber, the platforms and the height bar; the background stays quiet.
  - Signature: a height bar down the side with two markers, your best and the record to beat.
  - Every figure drawn, no emoji, no plain circles.
- References: none named.
- View: 2D · Camera: side view, follows the climber upward.
- Text: Sorani Kurdish, right to left, with Western digits. *(the digits are guessed)*

## First minute
You stand in a garden at the foot of the tower with the height bar at zero. Wide, easy ledges teach the jump: tap for a hop, hold for a full jump. At about 15 metres the ledges narrow and you miss one, fall, and land a few ledges down instead of dying. You climb back past it, reach the first gold rest floor at 30 metres, a chime plays and a banner shows the height. Above it the first moving platform slides into view.

## Challenge
*(provisional)* Real trouble starts in the second zone, around minute three: narrow ledges, then moving platforms. Nothing kills you; a miss costs height, from a few metres to a whole zone, and gold rest floors every 30 metres or so catch long falls. Each of the ten zones adds one thing: crumbling ledges, ice, wind, bouncers, conveyor belts, red bumpers that knock you off, and a final zone that mixes them. A first clear should take most players an hour or more; a practised run about ten minutes.

## Core loop
- Read the next jump and commit to it.
- Land it and gain height, or miss and fall to whatever is below.
- Reach a gold rest floor: a chime, a banner, a safe place to breathe.
- Reach the top: your time, your falls and a code to share.

## Goal & fail state
- Reach the top of the tower; the ending shows your time, your falls and your finish code. *(provisional)*
- No death and no game over: a fall costs height. R starts the climb again. *(provisional)*

## Features
- [x] A tall hand-made tower of ten zones, one new idea each
- [x] Falls that drop you to what is below, with gold rest floors that catch long falls
- [x] Forgiving jump: late jumps, early presses and hold-to-jump-higher
- [x] Height, timer and fall counter always on screen, with a side height bar
- [x] Your best height and best time kept between sessions
- [x] A finish code with the time in it, and a screen to check a viewer's code against yours
- [x] Phone touch buttons
- [ ] Sound for jump, landing, a long fall and each rest floor

## Your systems
- parkour · in the slice
- who wins against me i give a gift · thin stand-in *(guessed)*

## Keep
-

## Avoid
- duel maybe not makes sense
- the game or the idea shouldn't be short

## Out of scope
- A shared online leaderboard: needs a server: not built yet
- Real TikTok chat or gift connection: needs a server: not built yet
- A ghost of your best run: change it after play
- A speedrun split timer per zone: change it after play
- New moves over time: grow it
- A daily course: change it after play

## Build order
1. The slice: the whole tower from the garden to the top, with falls, rest floors, the HUD, the finish code and the code checker.
2. A ghost of your best run and zone split times.
3. A shared leaderboard on a server.

---
**Kill condition:** if falling still feels unfair instead of funny after two tuning rounds. *(guessed)*

**Who it's for / what they feel:** your TikTok viewers and you; tension on every jump, laughter on every fall. *(guessed)*

**Games you love / don't:** none named.
