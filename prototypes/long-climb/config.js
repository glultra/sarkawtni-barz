// PROTOTYPE - NOT FOR PRODUCTION
// Question: Is one long, punishing climb fun to play and to watch for an hour?
// Date: 2026-11-15
// Every tuning value lives here, each with value, min, max and step. The game reads them live
// through tune.<name> (index.html); the tweak panel, an add-on, writes them back here.
// Distances are metres and speeds metres per second.
window.CONFIG = {
  seed: 1, // the tower is the same for everyone; this seeds only what is random in play (none yet)

  // Your own songs, one per world (worlds are 5 levels each). Put the files in assets/music/ and
  // write their names here, for example "assets/music/1.mp3". An empty name plays the built-in tune.
  songs: ['', '', '', '', '', ''],

  // The jump: the first things to tune if the climb feels wrong
  jump_speed: { value: 8.4, min: 5, max: 12, step: 0.1 },        // upward speed at take-off
  gravity: { value: 21, min: 10, max: 35, step: 0.5 },           // while rising
  fall_mult: { value: 1.3, min: 1, max: 2.5, step: 0.05 },       // falling is this many times heavier than rising
  jump_cut: { value: 0.45, min: 0.2, max: 1, step: 0.05 },       // letting go early keeps this share of the rise
  coyote: { value: 0.1, min: 0, max: 0.25, step: 0.01 },         // seconds a jump still counts after leaving a ledge
  jump_buffer: { value: 0.12, min: 0, max: 0.25, step: 0.01 },   // seconds an early press is remembered
  max_fall: { value: 26, min: 12, max: 45, step: 1 },            // fastest fall

  // Running
  move_speed: { value: 5.2, min: 3, max: 9, step: 0.1 },
  ground_accel: { value: 55, min: 15, max: 120, step: 1 },       // to speed and to rest on a ledge
  air_accel: { value: 30, min: 8, max: 80, step: 1 },            // steering in the air
  ice_accel: { value: 7, min: 2, max: 30, step: 0.5 },           // on ice: slow to start, slow to stop

  // How hard: the first levels use the easy numbers, the last the hard ones
  ledge_wide: { value: 1.9, min: 1, max: 3, step: 0.05 },        // half the width of a ledge on level 1
  ledge_narrow: { value: 0.85, min: 0.5, max: 2, step: 0.05 },   // and on level 30
  gap_small: { value: 0.7, min: 0.2, max: 2, step: 0.05 },       // the gap between ledges on level 1
  gap_big: { value: 2.1, min: 1, max: 2.6, step: 0.05 },         // and on level 30
  easy_size: { value: 1.25, min: 1, max: 1.8, step: 0.05 },      // easy mode: ledges this many times wider
  hard_size: { value: 0.85, min: 0.5, max: 1, step: 0.05 },      // hard mode: ledges this many times as wide

  // Hazards
  bounce_speed: { value: 14.5, min: 10, max: 20, step: 0.5 },    // a bouncer throws you up this fast
  wind_speed: { value: 2.2, min: 0, max: 5, step: 0.1 },         // the wind carries you sideways in the air
  wind_period: { value: 3.6, min: 1.5, max: 8, step: 0.1 },      // seconds before the wind turns
  crumble_delay: { value: 0.55, min: 0.15, max: 1.5, step: 0.05 },// seconds a crumbling ledge holds you
  crumble_back: { value: 2.8, min: 1, max: 8, step: 0.1 },       // seconds until it comes back
  bump_push: { value: 8, min: 3, max: 15, step: 0.5 },           // a red bumper knocks you away this fast
  bump_stun: { value: 0.3, min: 0, max: 0.8, step: 0.02 },       // seconds you cannot steer after a bump

  // Falls and the camera
  fall_min: { value: 4, min: 1, max: 15, step: 0.5 },            // metres dropped before it counts as a fall
  cam_dist: { value: 12.5, min: 6, max: 18, step: 0.5 },         // metres the camera stands outside the climber
  cam_height: { value: 5, min: 1, max: 8, step: 0.2 },         // metres the camera stands above the climber
  cam_turn: { value: 5, min: 1.5, max: 12, step: 0.5 },          // how fast the camera swings round the tower
  fov: { value: 84, min: 55, max: 100, step: 1 },                // degrees, top to bottom
  banner_time: { value: 2.6, min: 1, max: 6, step: 0.1 },        // seconds a level's banner stays

  volume: { value: 0.5, min: 0, max: 1, step: 0.05 },
  music_volume: { value: 0.5, min: 0, max: 1, step: 0.05 },      // the built-in tunes and your songs
};
