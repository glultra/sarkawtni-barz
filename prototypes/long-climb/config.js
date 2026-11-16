// PROTOTYPE - NOT FOR PRODUCTION
// Question: Is one long, punishing climb fun to play and to watch for an hour?
// Date: 2026-11-15
// Every tuning value lives here, each with value, min, max and step. The game reads them live
// through tune.<name> (index.html); the tweak panel, an add-on, writes them back here.
// Distances are pixels: 40 pixels is one metre on the height bar.
window.CONFIG = {
  seed: 1, // the tower is the same for everyone; this seeds only what is random in play (none yet)

  // Your own songs, one per world (a world is 5 levels, so 10 in all). Put the files in
  // assets/music/ and write their names here, for example "assets/music/1.mp3".
  // An empty name plays the tune the game makes itself.
  songs: ['', '', '', '', '', '', '', '', '', ''],

  // The jump: the first things to tune if the climb feels wrong
  jump_speed: { value: 760, min: 500, max: 1000, step: 10 },     // pixels per second upward at take-off
  gravity: { value: 2100, min: 1200, max: 3200, step: 50 },      // pixels per second per second while rising
  fall_mult: { value: 1.35, min: 1, max: 2.5, step: 0.05 },      // falling is this many times heavier than rising
  jump_cut: { value: 0.45, min: 0.2, max: 1, step: 0.05 },       // letting go early keeps this share of the rise
  coyote: { value: 0.09, min: 0, max: 0.25, step: 0.01 },        // seconds a jump still counts after leaving a ledge
  jump_buffer: { value: 0.11, min: 0, max: 0.25, step: 0.01 },   // seconds an early press is remembered
  max_fall: { value: 1150, min: 600, max: 2000, step: 50 },      // fastest fall, pixels per second

  hang_speed: { value: 110, min: 0, max: 300, step: 10 },         // near the top of a held jump (slower than this) you hang a moment
  hang_mult: { value: 0.55, min: 0.2, max: 1, step: 0.05 },       // gravity is this much weaker while you hang
  edge: { value: 1, min: -8, max: 8, step: 1 },                   // pixels of a ledge's edge that do not hold you; below 0 is forgiving

  // Running
  move_speed: { value: 260, min: 150, max: 420, step: 10 },      // pixels per second
  ground_accel: { value: 2600, min: 800, max: 6000, step: 100 }, // to speed and to rest on a ledge
  air_accel: { value: 2000, min: 400, max: 4000, step: 100 },    // steering in the air
  ice_accel: { value: 420, min: 100, max: 1500, step: 20 },      // on ice: slow to start, slow to stop

  // How hard: level 1 uses the first number of each pair, level 50 the second
  ledge_wide: { value: 185, min: 120, max: 300, step: 10 },      // width of a ledge on level 1
  ledge_narrow: { value: 70, min: 50, max: 160, step: 5 },       // and on level 50
  gap_small: { value: 32, min: 0, max: 80, step: 2 },            // sideways gap between ledges on level 1
  gap_big: { value: 98, min: 50, max: 104, step: 2 },            // and on level 50 (104 is the most a jump can clear)
  easy_size: { value: 1.3, min: 1, max: 1.8, step: 0.05 },       // easy: ledges this many times wider
  hard_size: { value: 0.85, min: 0.6, max: 1, step: 0.05 },      // hard: ledges this many times as wide

  // Hazards
  bounce_speed: { value: 1120, min: 800, max: 1500, step: 20 },  // a bouncer throws you up this fast
  wind_speed: { value: 115, min: 0, max: 260, step: 5 },         // the wind carries you sideways in the air
  wind_period: { value: 3.6, min: 1.5, max: 8, step: 0.1 },      // seconds before the wind turns
  crumble_delay: { value: 0.5, min: 0.15, max: 1.5, step: 0.05 },// seconds a crumbling ledge holds you
  crumble_back: { value: 2.6, min: 1, max: 8, step: 0.1 },       // seconds until it comes back
  bump_push: { value: 430, min: 150, max: 800, step: 10 },       // a red bumper or a turret's shot knocks you away this fast
  bump_stun: { value: 0.28, min: 0, max: 0.8, step: 0.02 },      // seconds you cannot steer after a knock

  fan_speed: { value: 430, min: 250, max: 700, step: 10 },       // a fan's column lifts you this fast
  fan_height: { value: 300, min: 150, max: 500, step: 10 },      // and reaches this far above the fan

  // Racing: shoving, hanging on to feet, and the bots
  push_speed: { value: 520, min: 250, max: 900, step: 10 },      // a shove throws a climber sideways this fast
  push_cool: { value: 1, min: 0.3, max: 4, step: 0.1 },          // seconds between shoves
  grab_time: { value: 3, min: 0.5, max: 8, step: 0.1 },          // seconds you can hang on to someone's feet before your grip gives
  grab_cool: { value: 4, min: 1, max: 12, step: 0.5 },           // seconds after letting go before you can grab again
  grab_reach: { value: 80, min: 40, max: 160, step: 5 },         // pixels sideways within which you can catch someone's feet
  grab_up: { value: 130, min: 40, max: 220, step: 10 },          // and how far above you their feet may be
  carry_jump: { value: 0.9, min: 0.6, max: 1, step: 0.02 },      // with someone on your feet, your jump is this many times as strong (each rider counts, up to three)
  carry_run: { value: 0.85, min: 0.5, max: 1, step: 0.05 },      // and you run this many times as fast
  carry_slow: { value: 1.35, min: 1, max: 2.5, step: 0.05 },     // a bot with you on its feet takes this many times as long over a jump
  hang_leap: { value: 1.1, min: 0.5, max: 1.5, step: 0.05 },     // leaping off the feet you hold is this many times a normal jump
  bot_grab: { value: 0.25, min: 0, max: 1, step: 0.05 },         // chance a bot that lands just under you catches your feet
  bot_pace: { value: 1.3, min: 0.6, max: 2.5, step: 0.05 },     // seconds a bot takes for one jump on level 1; smaller is a faster bot
  bot_slip: { value: 0.05, min: 0, max: 0.4, step: 0.01 },       // chance a bot slips after a jump on level 1; it grows with the levels
  room_size: { value: 8, min: 2, max: 25, step: 1 },             // seats in a room you make, until you change it on the room screen

  // Machines and the blaster
  ghost_time: { value: 6, min: 3, max: 12, step: 0.5 },          // seconds the timed ledges stay solid after their plate is pressed
  shot_speed: { value: 900, min: 400, max: 1600, step: 50 },     // the blaster's shot, pixels per second
  shot_cool: { value: 0.25, min: 0.08, max: 1, step: 0.01 },     // seconds between shots
  off_time: { value: 5, min: 2, max: 12, step: 0.5 },            // seconds a shot bumper or turret stays out
  turret_period: { value: 2.6, min: 1, max: 6, step: 0.1 },      // seconds between a turret's shots
  orb_speed: { value: 230, min: 100, max: 500, step: 10 },       // a turret's shot, pixels per second

  // Falls and the camera
  fall_min: { value: 4, min: 1, max: 15, step: 0.5 },            // metres dropped before it counts as a fall
  cam_lead: { value: 150, min: 0, max: 400, step: 10 },          // pixels of extra view above the climber
  cam_speed: { value: 7, min: 2, max: 20, step: 0.5 },           // how fast the camera catches up, per second
  banner_time: { value: 2.8, min: 1, max: 6, step: 0.1 },        // seconds a level's banner stays

  volume: { value: 0.5, min: 0, max: 1, step: 0.05 },
  music_volume: { value: 0.5, min: 0, max: 1, step: 0.05 },      // the built-in tunes and your songs
};
