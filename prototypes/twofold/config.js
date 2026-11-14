// PROTOTYPE - NOT FOR PRODUCTION
// Question: Is crossing between two overlaid worlds to solve a room satisfying for ten minutes?
// Date: 2026-11-15
// Every tuning value lives here, each with value, min, max and step. The game reads them live
// through tune.<name> (index.html); the tweak panel, an add-on, writes them back here.
window.CONFIG = {
  seed: 1, // every gameplay random call reads rng(), seeded from this (or ?seed= in the address)
  volume: { value: 0.5, min: 0, max: 1, step: 0.05 },

  // The walker
  move_speed: { value: 4.2, min: 2, max: 8, step: 0.1 },        // tiles per second
  turn_speed: { value: 14, min: 4, max: 30, step: 1 },          // how fast the walker turns to face its way, per second

  // The camera: a fixed tilt from the south, leaning toward the walker
  cam_height: { value: 10.5, min: 6, max: 16, step: 0.5 },     // tiles above the floor
  cam_back: { value: 6.5, min: 3, max: 12, step: 0.5 },       // tiles south of what it looks at
  cam_fov: { value: 46, min: 30, max: 70, step: 1 },            // degrees
  cam_follow: { value: 0.35, min: 0, max: 1, step: 0.05 },      // 0 looks at the room's middle, 1 at the walker

  // Portals
  flash_s: { value: 0.4, min: 0.1, max: 1.2, step: 0.05 },      // seconds the crossing flash lasts

  // Doors
  door_open_speed: { value: 4, min: 1, max: 12, step: 0.5 },    // door travel per second (1 = fully open)
  door_close_speed: { value: 8, min: 1, max: 16, step: 0.5 },   // fast, so a switch cannot be outrun

  // Rooms and help
  level_done_s: { value: 0.9, min: 0.3, max: 2.5, step: 0.1 },  // seconds on the lit exit before the next room
  stuck_prompt_s: { value: 60, min: 15, max: 180, step: 5 },    // seconds with no crossing or carry before the quiet prompt
  undo_cap: { value: 400, min: 50, max: 2000, step: 50 },       // moves kept for undo
};
