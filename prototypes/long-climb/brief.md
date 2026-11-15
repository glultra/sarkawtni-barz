# Game Brief: Long Climb

## In your words
i am currenlty streaming in tiktok i want a challenging game which may attract more viewers please do u think of creating a game for me for that ? or maybe  a simple game for 1v1 so i say who wins against me i give a gift smth like that orr just leaning to a solo play and viewer would love to see it from my while streaming ! but the game or the idea shouldn't be short right ? otherwise the the viewers won't love it

before u choose which is demanding more u think ? especially in kurdish

well no idk idk anyy !! i just need a game for streaming in tiktok so duel maybe not makes sense does it ? gifts i say i give him with fib or smth else soo maybe we think of the game long and enjoyable parkour or smth like that what do u say ?

Pitch 1 (The Long Climb): you pick

After play: dude i asked the game is short yet i want maybe 20-50 levels with more difficulty levels ! and each several levels plays a kurdish song ! and if it is possible and nice can u go with fully 3D as the current tokens left ? like chained together ! and  so on

**Pitch:** One tall tower, hard but fair, where a slip drops you back down; you climb it live while viewers race the same tower on their phones for your prize.

**Genre:** 3D precision platformer, one long climb of thirty levels

**Controls:** W A S D or the arrows to move (up is towards the tower), Space to jump (hold for a higher jump), hold R to start the climb again, P to pause, M to mute, C on the title to check a viewer's code, 1 2 3 or the arrows on the title to pick the difficulty, B to continue. The camera turns by itself. On a phone: four arrow buttons on the left and a jump button on the right. *(guessed)*

**Question:** Is one long, punishing climb fun to play and to watch for an hour? *(guessed)*

**Hooks:** A fall is never a reset: you drop until something below catches you, so every metre is at risk and viewers watch the height bar. Finishing gives a code with your time, so a viewer can prove they beat you.

**Intent:** soft or no failure *(provisional)*

## Look and references
- Look: *(provisional)*
  - Mood: a bright toy tower rising out of a garden into the clouds, the sunset, the stars; cheerful, with real tension.
  - Camera and scale: 3D, tall 9:16 screen for TikTok; the camera stands outside the tower above the climber and swings round it by itself.
  - Shapes: chunky and rounded; a small round climber with a red scarf; platforms with thick outlines.
  - Values: the climber and what you can stand on are the brightest things; the sky behind stays soft.
  - Palette: the sky changes every zone (green garden, white clouds, orange sunset, deep blue night, starry top), so height reads at a glance. Hazards are red; safe rest floors are gold.
  - Detail: on the climber, the platforms and the height bar; the background stays quiet.
  - Signature: a height bar down the side with every level marked, where you are and your best; a gold marker floats over the ledge to jump to next.
  - Every figure drawn, no emoji, no plain circles.
- References: Chained Together (a tall 3D climb where a fall costs height; nothing of its chain, art or levels).
- View: 3D · Camera: outside the tower, above and behind the climber, turning by itself.
- Text: Sorani Kurdish, right to left, with Western digits. *(the digits are guessed)*

## First minute
You pick a difficulty and stand in a garden at the foot of the tower with the height bar at zero. A gold marker floats over the first ledge. Wide, easy ledges wind round the pillar and teach the jump: tap for a hop, hold for a full jump. You miss one, fall, and land on what is below instead of dying. At about 12 metres you land on the first gold floor: a chime plays, a banner shows level 1 of 30, and the climb is kept from there.

## Challenge
*(provisional)* Thirty levels in six worlds of five, each level narrower and wider-gapped than the last. Real trouble starts in the second world, level 6: moving ledges and lifts. Then crumbling ledges and wind, ice, bouncers and conveyor belts, and a last world that mixes them under red bumpers. Nothing kills you; a miss costs height. Three difficulties: easy has wider ledges and a gold floor after every level, normal has the floors, hard has narrower ledges and a floor only every fifth level, so a miss can cost a whole world. How long a first clear takes is not known yet.

## Core loop
- Read the next jump and commit to it.
- Land it and gain height, or miss and fall to whatever is below.
- Reach a gold rest floor: a chime, a banner, a safe place to breathe.
- Reach the top: your time, your falls and a code to share.

## Goal & fail state
- Reach the top of the tower; the ending shows your time, your falls and your finish code. *(provisional)*
- No death and no game over: a fall costs height. R starts the climb again. *(provisional)*

## Features
- [x] A tall tower of thirty levels in six worlds, each world adding its own hazards
- [x] Three difficulties, picked on the title: easy, normal, hard
- [x] A tune for each world, made in code, with a slot for your own song in each
- [x] The climb is kept at every level, to continue after closing the game
- [x] Falls that drop you to what is below, with gold rest floors that catch long falls
- [x] Forgiving jump: late jumps, early presses and hold-to-jump-higher
- [x] Height, timer and fall counter always on screen, with a side height bar
- [x] Your best height and best time kept between sessions
- [x] A finish code with the time in it, and a screen to check a viewer's code against yours
- [x] Phone touch buttons: four arrows and a jump
- [ ] Sound for jump, landing, a long fall and each rest floor

## Your systems
- parkour · in the slice
- 20-50 levels with more difficulty levels · in the slice
- each several levels plays a kurdish song · thin stand-in
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
- A speedrun split timer per level: change it after play
- Real Kurdish songs: yours to add, since the game ships none; put the files in assets/music/ and name them in config.js
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
