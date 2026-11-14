// PROTOTYPE - NOT FOR PRODUCTION
// Question: Is crossing between two overlaid worlds to solve a room satisfying for ten minutes?
// Date: 2026-11-15
// Every tuning value lives here, each with value, min, max and step. The game reads them live
// through tune.<name> (index.html); the tweak panel, an add-on, writes them back here.
window.CONFIG = {
  seed: 1, // every gameplay random call reads rng(), seeded from this (or ?seed= in the address)
  volume: { value: 0.5, min: 0, max: 1, step: 0.05 },

  // The walker
  move_speed: { value: 230, min: 120, max: 400, step: 10 },     // px per second (a tile is 40 px)
  gravity: { value: 1900, min: 800, max: 3200, step: 50 },      // px per second squared
  jump_height: { value: 1.3, min: 1.05, max: 1.9, step: 0.05 }, // tiles; under 2 so a 2-tile wall needs a crate
  coyote_s: { value: 0.1, min: 0, max: 0.25, step: 0.01 },      // seconds a jump still works after leaving a ledge
  jump_buffer_s: { value: 0.12, min: 0, max: 0.25, step: 0.01 },// seconds an early jump press is remembered

  // Crates
  pickup_reach: { value: 1.5, min: 1, max: 2.5, step: 0.1 },    // tiles, sideways, from the walker's middle

  // Portals
  portal_tall: { value: 4, min: 3, max: 6, step: 1 },           // tiles above the floor the seam reaches (blocks jumping over it)
  wipe_s: { value: 0.45, min: 0.1, max: 1.2, step: 0.05 },      // seconds the other life takes to spread from the portal

  // Doors
  door_open_speed: { value: 4, min: 1, max: 12, step: 0.5 },    // door travel per second (1 = fully open)
  door_close_speed: { value: 8, min: 1, max: 16, step: 0.5 },   // fast, so a switch cannot be outrun

  // Rooms and help
  level_done_s: { value: 0.9, min: 0.3, max: 2.5, step: 0.1 },  // seconds on the lit exit before the next room
  stuck_prompt_s: { value: 60, min: 15, max: 180, step: 5 },    // seconds with no crossing or carry before the quiet prompt
  undo_cap: { value: 400, min: 50, max: 2000, step: 50 },       // moves kept for undo
};
