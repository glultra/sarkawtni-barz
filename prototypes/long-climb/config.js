// PROTOTYPE - NOT FOR PRODUCTION
// Question: Is one long, punishing climb fun to play and to watch for an hour?
// Date: 2026-11-15
// Every tuning value lives here, each with value, min, max and step. The game reads them live
// through tune.<name> (index.html); the tweak panel, an add-on, writes them back here.
// Distances are pixels: 40 pixels is one metre on the height bar.
window.CONFIG = {
  seed: 1, // the tower is the same for everyone; this seeds only what is random in play (none yet)

  // Your own songs, one per world (a world is 3 levels, so 10 in all). Put the files in
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
  edge: { value: 1, min: -8, max: 8, step: 1 },
  car_snap: { value: 18, min: 0, max: 40, step: 1 },             // feet this far under a car's top, falling, are lifted onto it: no slipping through a moving car                   // pixels of a ledge's edge that do not hold you; below 0 is forgiving

  // Running
  move_speed: { value: 260, min: 150, max: 420, step: 10 },      // pixels per second
  ground_accel: { value: 2600, min: 800, max: 6000, step: 100 }, // to speed and to rest on a ledge
  air_accel: { value: 2000, min: 400, max: 4000, step: 100 },    // steering in the air
  ice_accel: { value: 420, min: 100, max: 1500, step: 20 },      // on ice: slow to start, slow to stop

  // Leaning ledges: some furniture, boxes and ice are tilted; you stand on the slope and slide slowly down it
  tilt_share: { value: 0.4, min: 0, max: 1, step: 0.05 },        // share of those ledges that lean (0: every ledge flat)
  tilt_min: { value: 4, min: 0, max: 15, step: 1 },              // the least lean, degrees
  tilt_max: { value: 9, min: 1, max: 15, step: 1 },              // the most lean, degrees (past about 10 some jumps get very hard)
  tilt_slide: { value: 45, min: 0, max: 200, step: 5 },          // how fast you slide down the steepest lean, pixels per second (ice slides more)

  // How hard: each pair runs from its first number (fully easy) to its second (level 30).
  // start_hard is where level 1 begins on that run: 0 is fully easy, 1 is as hard as level 30.
  start_hard: { value: 0.33, min: 0, max: 0.8, step: 0.01 },     // 0.33: level 1 is about as hard as level 17 of the old 50
  ledge_wide: { value: 185, min: 120, max: 300, step: 10 },      // width of a ledge at the easy end
  ledge_narrow: { value: 70, min: 50, max: 160, step: 5 },       // and on level 30
  gap_small: { value: 32, min: 0, max: 80, step: 2 },            // sideways gap between ledges at the easy end
  gap_big: { value: 98, min: 50, max: 104, step: 2 },            // and on level 30 (104 is the most a jump can clear)
  easy_size: { value: 1.3, min: 1, max: 1.8, step: 0.05 },       // easy: ledges this many times wider
  hard_size: { value: 0.85, min: 0.6, max: 1, step: 0.05 },      // hard: ledges this many times as wide

  // Hazards
  bounce_speed: { value: 1120, min: 800, max: 1500, step: 20 },  // a bouncer throws you up this fast
  flip_share: { value: 0.5, min: 0, max: 0.9, step: 0.05 },      // Sun and moon (world 6, and 0.7 of this in the last world): the share of plain ledges that are sun or moon; every jump swaps which is solid
  wind_speed: { value: 75, min: 0, max: 260, step: 5 },          // the wind blows only on Hard (world 6 and the last world): it carries you sideways in the air (it was 115: too strong); it eases in and out around each turn
  wind_period: { value: 3.6, min: 1.5, max: 8, step: 0.1 },      // seconds before the wind turns
  crumble_delay: { value: 0.5, min: 0.15, max: 1.5, step: 0.05 },// seconds a crumbling ledge holds you
  crumble_back: { value: 1.2, min: 0.5, max: 8, step: 0.1 },       // seconds until it comes back
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
  room_size: { value: 8, min: 2, max: 25, step: 1 },

  // Power-ups: a bubble in each world's hardest level, of a kind that helps there (WORLDS[w].pu in index.html)
  pickup_back: { value: 30, min: 3, max: 120, step: 1 },          // seconds before a taken one comes back
  buff_wing: { value: 8, min: 2, max: 60, step: 1 },             // seconds of double jump
  buff_speed: { value: 6, min: 2, max: 60, step: 1 },             // seconds of speed
  buff_speed_mult: { value: 1.5, min: 1.1, max: 2.5, step: 0.05 }, // how much faster you run
  buff_feather: { value: 6, min: 2, max: 60, step: 1 },           // seconds of feather fall
  buff_feather_g: { value: 0.45, min: 0.15, max: 0.9, step: 0.05 }, // gravity on the way down, as a share of the usual
  buff_shield: { value: 12, min: 2, max: 60, step: 1 },           // seconds the shield lasts, if nothing hits it first

  // TikTok LIVE gifts (start-live.bat): each gift does one thing. By its worth in diamonds: a gift worth
  // at least the number does that, the highest that fits. Or by its name, which wins over its worth.
  // The things: 'wind' a gust that pushes you, 'jump' super jumps, 'bumper' red bumpers by you,
  // 'shake' an earthquake that throws you off your ledge, 'rocket' a blast about a level up,
  // 'piano' a grand piano that falls on you. A bigger gift does a bigger thing; the game lists them all
  // on screen under the likes card.
  gifts: {
    // [diamonds at least, what it does, how much (super jumps, bumpers or pianos; a rocket of 2 goes two levels), a gift's name to show]
    rules: [[1, 'piano', 1, 'Rose'], [10, 'jump', 2, 'Perfume'], [30, 'bumper', 2, 'Doughnut'], [99, 'jump', 3, 'Hat and Mustache'], [199, 'shake', 1, 'Corgi'], [500, 'rocket', 1, 'Money Gun'], [1000, 'rocket', 2, 'Galaxy']], // the names are one gift of each worth, to show on screen (TikTok's prices can change: any gift worth that much does the same)
    names: { /* 'Rose': 'wind', 'Lion': ['rocket', 2] */ }, // by name: what it does, or [what, how much]
    like_prize: 'rocket', // what reaching the like goal (like_goal) gives everyone: 'rocket' or 'jump'
    share: 'all', // in a room: 'chaos' sends wind, bumpers, earthquakes and falling things to everyone and keeps super jumps and rockets yours; 'all' sends everything; 'none' keeps it all yours
  },
  gift_gap: { value: 0, min: 0, max: 6, step: 0.1 },              // seconds between two gifts' effects (0: every gift at once, as it comes; above 0 the rest wait their turn)
  feed_lines: { value: 3, min: 0, max: 6, step: 1 },              // lines in the recent-gifts feed on screen (0 hides it)
  feed_time: { value: 4, min: 1, max: 10, step: 0.5 },            // seconds a line stays in the feed
  gift_wind_time: { value: 3, min: 1, max: 8, step: 0.5 },        // seconds a gust blows, for each gift in a streak (up to 4)
  gift_wind_speed: { value: 170, min: 50, max: 400, step: 10 },   // how hard a gust pushes, pixels per second
  gift_jump: { value: 1.4, min: 1.1, max: 2, step: 0.05 },         // a super jump is this many times a jump
  gift_bumper_time: { value: 10, min: 3, max: 30, step: 1 },      // seconds a gift's red bumper stays
  gift_rocket: { value: 2300, min: 1200, max: 3200, step: 50 },   // a rocket's upward speed, pixels per second (2300 is about a level)
  // Likes: every like_drop of them, a heart, a shoe or a flowerpot falls at you with the liker's name. A share gives a super jump.
  like_drop: { value: 5, min: 1, max: 500, step: 1 },            // likes for one falling thing (raise it as your LIVE grows: 50 or more for a busy one)
  drop_speed: { value: 430, min: 150, max: 900, step: 10 },       // how fast it falls, pixels per second
  like_goal: { value: 1000, min: 50, max: 100000, step: 50 },     // the like goal on screen: when the LIVE's likes reach it, everyone gets the prize (gifts.like_prize), and it starts again
  // A gift's grand piano: it falls slowly with a red mark and a shadow where it will land; it flattens you if it hits,
  // then stays a while as a ledge you can climb, playing notes under your feet
  piano_speed: { value: 210, min: 80, max: 600, step: 10 },      // how fast it falls, pixels per second (a falling heart: drop_speed)
  piano_stay: { value: 7, min: 2, max: 20, step: 0.5 },          // seconds it stays once landed
  piano_w: { value: 150, min: 90, max: 260, step: 10 },          // its width in pixels
  drop_push: { value: 1, min: 0, max: 2, step: 0.1 },             // how hard a hit knocks you, as a share of a red bumper's push (0: they never hurt)
  drop_gap: { value: 1, min: 0.5, max: 8, step: 0.5 },            // seconds at least between two falling things             // seats in a room you make, until you change it on the room screen

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

  // Funny lines: a speech bubble over the climber's head (the room sees yours too)
  quote_fall: { value: 12, min: 4, max: 60, step: 1 },           // metres a fall must cost for a sure line from the bigfall list (smaller falls: the fall list, sometimes)
  quote_idle: { value: 8, min: 3, max: 30, step: 1 },            // seconds standing still before the idle list teases you
  quote_chance: { value: 0.6, min: 0, max: 1, step: 0.05 },      // how often the other moments get a line (0: only big falls and standing still)
  quote_gap: { value: 3, min: 0, max: 15, step: 0.5 },           // seconds at least between two lines, big falls aside
  quote_time: { value: 2.6, min: 1, max: 6, step: 0.1 },         // seconds a bubble stays
  // The lines for each moment, in any number; add your own. They show in Kurdish in either language.
  quotes: {
    bigfall: ['ئەی لە قەبری بام!', 'ئاخ برا گیان...', 'ئەی هاوار!', 'وای دایکە گیان!', 'هەموو ڕەنجم بە با چوو', 'ئۆف خوایە، دیسانەوە؟', 'بەخوا ئەمجارە دەیبەمەوە'],
    fall: ['ئاخ برا گیان', 'ئۆف!', 'هێواش هێواش...', 'ئاخ پشتم!'],
    bump: ['سەرم گێژ بوو', 'ئەمە چی بوو؟!', 'ئەی لە قەبری بام!', 'کێ ئەمەی لێرە داناوە؟'],
    pushed: ['کێ پاڵی پێوەنام؟!', 'ئەی بێ ویژدان!', 'ئاخ برا گیان، بۆ؟', 'دەمبینیتەوە!'],
    rider: ['دابەزە لە پێم!', 'قورسی برا گیان!', 'ئەمە پاسە؟'],
    idle: ['چیە بۆ ناجوڵێیتەوە، ئەترسی شێرەکەم؟', 'خەوت لێکەوت؟', 'یەڵڵا، هەستە!', 'چاوەڕێی چی دەکەیت؟'],
    level: ['بژی شێرەکەم!', 'دەستت خۆش!', 'ئەوەتا!', 'کەس ناگاتە من!'],
    gift: ['قوربانت بم!', 'دەستت خۆش برا گیان!', 'ئەوەتا، بفڕە!'],
    finish: ['لووتکە هی منە!', 'بژی شێرەکەم!', 'کێ وەک من؟'],
    piano: ['پیانۆ؟! بەڕاستی؟', 'ئەی لە قەبری بام!', 'کێ پیانۆ فڕێ دەدات؟!', 'سەرم بوو بە دەهۆڵ'], // flattened by a gift's piano
  },

  volume: { value: 0.5, min: 0, max: 1, step: 0.05 },            // everything: the Settings screen's "Master volume"
  music_volume: { value: 0.5, min: 0, max: 1, step: 0.05 },      // the built-in tunes and your songs
  sfx_volume: { value: 0.8, min: 0, max: 1, step: 0.05 },        // jumps, landings, falls and every other sound effect
  // What the Settings screen starts from on a first visit; the player's own choices are kept in the browser.
  prefs: {
    btn_alpha: 0.85,      // how solid the phone's buttons look (0.2 to 1)
    left_handed: false,   // jump on the left, the arrows on the right
    haptics: true,        // a short buzz when a phone button is pressed
    scenery: 1,           // 0: plain and light for a slow phone, 1: the painted layers
    shake: 2,             // the screen's shake on a fall or a hit: 0 off, 1 a little, 2 full
    particles: true,      // dust, sparks and confetti
    fps: false,           // a frame-rate counter in the corner
    ghost: true,          // the see-through ghost of your best climb
    bubbles: true,        // the funny lines in a speech bubble
  },
  scenery_grain: { value: 0.07, min: 0, max: 0.2, step: 0.01 },  // the fine grain over the sky, so it reads painted rather than flat
  scenery_light: { value: 0.12, min: 0, max: 0.3, step: 0.01 },  // how strongly each world's light tints the backdrop (the ledges keep their own colours)
  scenery_haze: { value: 0.14, min: 0, max: 0.4, step: 0.01 },   // a haze of the sky's own colour over the backdrop, so it steps back behind the tower
  ledge_shadow: { value: 0.32, min: 0, max: 0.8, step: 0.02 },   // the soft shadow behind each ledge on a bright sky
  ledge_glow: { value: 0.13, min: 0, max: 0.5, step: 0.02 },     // the faint glow behind each ledge on a dark sky (night, space, the cavern)
  music_tempo: { value: 72, min: 55, max: 110, step: 1 },        // beats a minute of the built-in tunes in the first world; each world after adds 1.5
};
