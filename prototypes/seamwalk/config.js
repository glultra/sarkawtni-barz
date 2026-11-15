// PROTOTYPE - NOT FOR PRODUCTION
// Question: Is walking through impossible spaces between two lives fun for ten minutes in first person?
// Date: 2026-11-15
// Every tuning value lives here, each with value, min, max and step. The game reads them live
// through tune.<name> (index.html); the tweak panel, an add-on, writes them back here.
window.CONFIG = {
  seed: 1, // every gameplay random call reads rng(), seeded from this (or ?seed= in the address)

  // Looking: the first things to tune if the mouse feels wrong
  look_sens: { value: 0.0021, min: 0.0005, max: 0.006, step: 0.0001 }, // radians per pixel of mouse travel
  look_smooth: { value: 0.025, min: 0, max: 0.12, step: 0.005 },       // seconds the view takes to catch the mouse; 0 is raw
  turn_rate: { value: 2.3, min: 0.8, max: 5, step: 0.1 },              // radians per second on the Left and Right arrows
  fov: { value: 74, min: 55, max: 100, step: 1 },                      // degrees, top to bottom

  // Walking
  move_speed: { value: 4.3, min: 2, max: 8, step: 0.1 },    // metres per second
  move_accel: { value: 38, min: 8, max: 90, step: 1 },      // metres per second per second, to speed and to rest
  eye_height: { value: 1.6, min: 1.2, max: 1.9, step: 0.05 }, // metres above the feet
  bob: { value: 0.03, min: 0, max: 0.08, step: 0.005 },     // metres the view rises and falls with each step
  gravity: { value: 22, min: 8, max: 40, step: 1 },         // metres per second per second off a ledge

  // Cubes, switches, doors and the gate
  reach: { value: 2.6, min: 1.5, max: 4, step: 0.1 },         // metres from which a cube can be picked up
  put_dist: { value: 1.3, min: 0.9, max: 2, step: 0.05 },     // metres ahead a cube is put down
  door_speed: { value: 1.6, min: 0.5, max: 4, step: 0.1 },    // a door's bars sink or rise this many times per second
  gate_light_dist: { value: 9, min: 4, max: 20, step: 0.5 },  // metres at which a gate with no door lights
  fade_time: { value: 0.5, min: 0.2, max: 1.5, step: 0.05 },  // seconds to fade between spaces

  // Seeing through arches
  portal_depth: { value: 2, min: 1, max: 3, step: 1 },        // arches seen through arches, this many deep

  volume: { value: 0.5, min: 0, max: 1, step: 0.05 },
};
