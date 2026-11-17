// PROTOTYPE - NOT FOR PRODUCTION
// Question: Is one long, punishing climb fun to play and to watch for an hour?
// Date: 2026-11-16
// Long Climb's room server. Run it on the PC that hosts the race:   node server.js   (or: node server.js 9000)
// It serves this folder as a website and passes messages between the players of a room.
// It needs nothing installed but Node. Players on the same Wi-Fi open the address it prints;
// players elsewhere need a tunnel to this port (for example: cloudflared tunnel --url http://localhost:8080).
// A room seats up to 25 climbers, as many as its host chose, and lets others in to watch.
'use strict';
const http = require('http'), fs = require('fs'), path = require('path'), crypto = require('crypto'), os = require('os');
const PORT = Number(process.argv[2]) || 8080, ROOT = __dirname;
const MAXP = 25, MAXALL = 60, TICK = 66; // the most climbers in a room, the most people in it with those who watch, and how often (ms) positions go out
const TYPES = { '.html': 'text/html; charset=utf-8', '.js': 'text/javascript; charset=utf-8', '.mp3': 'audio/mpeg', '.ogg': 'audio/ogg', '.wav': 'audio/wav', '.png': 'image/png' };
const SERVED = /^(index\.html|config\.js|tune\.js|assets\/music\/[\w .()-]+\.(mp3|ogg|wav))$/; // only the game's own files

const server = http.createServer((req, res) => {
  let file = 'index.html';
  try { file = decodeURIComponent((req.url || '/').split('?')[0]).replace(/^\/+/, '') || 'index.html'; } catch (e) { file = ''; }
  if (!SERVED.test(file)) { res.writeHead(404); res.end('not found'); return; }
  fs.readFile(path.join(ROOT, file), (err, data) => {
    if (err) { res.writeHead(404); res.end('not found'); return; }
    res.writeHead(200, { 'Content-Type': TYPES[path.extname(file)] || 'application/octet-stream', 'Cache-Control': 'no-cache' }); res.end(data);
  });
});

// ---- rooms -------------------------------------------------------------------------------
const rooms = new Map(); let nextId = 1;
const LETTERS = '23456789ABCDEFGHJKLMNPQRSTUVWXYZ';
function newRoomId() { for (;;) { let s = ''; for (const b of crypto.randomBytes(4)) s += LETTERS[b % 32]; if (!rooms.has(s)) return s; } }
const cleanName = n => String(n || '').replace(/[\u0000-\u001f<>]/g, '').trim().slice(0, 14) || 'player';
const cleanCid = s => String(s || '').replace(/[^0-9A-Za-z]/g, '').slice(0, 24); // the name a browser keeps for itself, so a player can come back to their place
const GOALS = [3, 6, 10, 20, 30];
const clampInt = (v, a, b) => Math.max(a, Math.min(b, Math.round(Number(v) || 0)));
const LAN = []; for (const list of Object.values(os.networkInterfaces())) for (const a of list || []) if (a.family === 'IPv4' && !a.internal) LAN.push(a.address + ':' + PORT); // what players on the same Wi-Fi type
const racers = room => room.players.filter(p => !p.spec); // those who climb; the rest only watch
const all = (room, m) => { const f = frame(m); if (f) for (const p of room.players) put(p, f); };
function roster(room, type, to) {
  const m = { t: type, id: room.id, host: room.host, mode: room.mode, goal: room.goal, size: room.size, racing: room.racing ? 1 : 0, lan: LAN[0] || '', players: room.players.map(p => ({ id: p.id, name: p.name, spec: p.spec ? 1 : 0, c: p.color })) };
  if (to) send(to, Object.assign({ you: to.id }, m)); else all(room, m);
}
function freeColor(room) { for (let i = 0; ; i++) if (!room.players.some(p => p.color === i)) return i; }
function remember(room, c) { if (room.racing && !room.over && !c.spec && !c.done && c.cid) room.saved.set(c.cid, c.top || 0); } // how far a climber who leaves the race had got, for when they come back
function checkOver(room) { // the race is over once every climber still in the room has finished
  if (!room.racing || room.over || !racers(room).every(p => p.done)) return;
  room.over = true; all(room, { t: 'over' }); console.log('room ' + room.id + ': everyone is in');
}
function sendGo(room, c) { // start this player in the running race: at the bottom, or where they had got to before they left
  const top = !c.spec && c.cid && room.saved.has(c.cid) ? room.saved.get(c.cid) : 0; if (c.cid) room.saved.delete(c.cid);
  c.done = false; c.st = null; c.top = top;
  send(c, { t: 'go', mode: room.mode, goal: room.goal, spec: c.spec ? 1 : 0, el: Date.now() - room.t0, from: top });
}
function leave(c) {
  const room = c.room; if (!room) return; c.room = null;
  remember(room, c);
  room.players = room.players.filter(p => p !== c);
  console.log('room ' + room.id + ': ' + c.name + ' left');
  if (!room.players.length) { rooms.delete(room.id); return; }
  if (room.host === c.id) room.host = (racers(room)[0] || room.players[0]).id; // the next player takes the room over
  roster(room, 'roster'); checkOver(room);
}
function onMessage(c, m) {
  if (!m || typeof m.t !== 'string') return;
  if (m.t === 'create' && !c.room) {
    const room = { id: newRoomId(), host: c.id, players: [c], mode: [0, 1, 2].includes(m.mode) ? m.mode : 1, goal: GOALS.includes(m.goal) ? m.goal : 5, size: clampInt(m.size || 8, 2, MAXP), racing: false, over: false, place: 0, t0: 0, saved: new Map(), banned: new Set() };
    c.name = cleanName(m.name); c.cid = cleanCid(m.cid); c.spec = false; c.done = false; c.st = null; c.color = 0; c.room = room; rooms.set(room.id, room); roster(room, 'room', c);
    console.log('room ' + room.id + ' made by ' + c.name + ' for ' + room.size + ' players'); return;
  }
  if (m.t === 'join' && !c.room) {
    const room = rooms.get(String(m.id || '').toUpperCase()), cid = cleanCid(m.cid);
    if (!room) { send(c, { t: 'err', msg: 'none' }); return; }
    if (cid && room.banned.has(cid)) { send(c, { t: 'err', msg: 'kicked' }); return; } // the host removed this player: no way back in
    if (room.players.length >= MAXALL) { send(c, { t: 'err', msg: 'full' }); return; }
    const full = racers(room).length >= room.size; // with every seat taken, a newcomer watches
    c.name = cleanName(m.name); c.cid = cid; c.done = false; c.st = null; c.spec = full; c.color = freeColor(room); c.room = room; room.players.push(c);
    roster(room, 'room', c); roster(room, 'roster'); if (full) send(c, { t: 'note', msg: 'noslot' });
    console.log('room ' + room.id + ': ' + c.name + (full ? ' joined to watch' : ' joined') + ' (' + racers(room).length + ' of ' + room.size + ' playing, ' + (room.players.length - racers(room).length) + ' watching)');
    if (room.racing) { sendGo(room, c); if (room.over) send(c, { t: 'over' }); } // a late player starts climbing at once
    return;
  }
  const room = c.room; if (!room) return;
  if (m.t === 's') { // where this climber is now; it goes out to the room on the next tick
    if (!room.racing || c.spec || c.done) return;
    const h = room.players.some(p => p.id === m.h && p !== c && !p.spec && !p.done) ? m.h : 0; // whose feet they hang on, if anyone's
    c.st = [c.id, clampInt(m.x, -100, 900), clampInt(m.a, -1000, 200000), m.f < 0 ? -1 : 1, h]; c.top = clampInt((Number(m.l) || 1) - 1, 0, Math.max(0, room.goal - 1)); return;
  }
  if (m.t === 'hit') { const to = room.players.find(p => p.id === m.to); if (room.racing && to && to !== c && !c.spec && !to.spec && !to.done) send(to, { t: 'hit', from: c.id, d: m.d < 0 ? -1 : 1 }); return; }
  if (m.t === 'fin') {
    if (room.racing && !c.spec && !c.done) { c.done = true; room.place++; all(room, { t: 'fin', id: c.id, cs: Math.max(0, Math.round(+m.cs || 0)), place: room.place }); checkOver(room); }
    return;
  }
  if (m.t === 'spec') { // to the watchers, or back among the climbers: anyone for themselves, the host for anyone
    const who = m.id && m.id !== c.id ? (room.host === c.id ? room.players.find(p => p.id === m.id) : null) : c, on = !!m.on;
    if (!who || who.spec === on || (room.racing && who.done)) return;
    if (!on && racers(room).length >= room.size) { send(c, { t: 'note', msg: 'noslot' }); return; }
    if (on) remember(room, who);
    who.spec = on; who.st = null; who.done = false; roster(room, 'roster');
    if (room.racing && !room.over) { if (on) checkOver(room); else sendGo(room, who); }
    return;
  }
  if (room.host !== c.id) return; // what follows is the host's alone
  if (m.t === 'kick') { // out of the room, and not let back in
    const who = room.players.find(p => p.id === m.id); if (!who || who === c) return;
    if (who.cid && who.cid !== c.cid) room.banned.add(who.cid);
    send(who, { t: 'kicked' }); who.room = null; room.players = room.players.filter(p => p !== who);
    console.log('room ' + room.id + ': ' + who.name + ' was removed by the host'); roster(room, 'roster'); checkOver(room);
  }
  else if (m.t === 'set' && !room.racing) {
    if ([0, 1, 2].includes(m.mode)) room.mode = m.mode; if (GOALS.includes(m.goal)) room.goal = m.goal;
    if (m.size !== undefined) room.size = Math.max(racers(room).length, clampInt(m.size, 2, MAXP)); // never fewer seats than climbers already in them
    roster(room, 'roster');
  }
  else if (m.t === 'go' && !room.racing && racers(room).length) {
    room.racing = true; room.over = false; room.place = 0; room.t0 = Date.now(); room.saved = new Map();
    roster(room, 'roster'); room.players.forEach(p => sendGo(room, p)); console.log('room ' + room.id + ': the race to level ' + room.goal + ' has started with ' + racers(room).length + ' climbers');
  }
  else if (m.t === 'end' && room.racing) { room.racing = false; room.over = false; room.players.forEach(p => { p.done = false; p.st = null; }); all(room, { t: 'lobby' }); roster(room, 'roster'); }
}
setInterval(() => { // every tick, each racing room hears where all its climbers are, in one message
  for (const room of rooms.values()) {
    if (!room.racing) continue;
    const p = []; for (const c of room.players) if (c.st) p.push(c.st);
    if (p.length) all(room, { t: 'ss', p });
  }
}, TICK);

// ---- WebSocket, by hand: text messages only, small ones ------------------------------------
function frame(obj) {
  const data = Buffer.from(JSON.stringify(obj)), n = data.length;
  if (n >= 65536) return null;
  return Buffer.concat([n < 126 ? Buffer.from([0x81, n]) : Buffer.from([0x81, 126, n >> 8, n & 255]), data]);
}
function put(c, f) { if (!c.sock.destroyed) c.sock.write(f); }
function send(c, obj) { const f = frame(obj); if (f) put(c, f); }
server.on('upgrade', (req, sock) => {
  const key = req.headers['sec-websocket-key'];
  if (!key) { sock.destroy(); return; }
  const accept = crypto.createHash('sha1').update(key + '258EAFA5-E914-47DA-95CA-C5AB0DC85B11').digest('base64');
  sock.write('HTTP/1.1 101 Switching Protocols\r\nUpgrade: websocket\r\nConnection: Upgrade\r\nSec-WebSocket-Accept: ' + accept + '\r\n\r\n');
  const c = { id: nextId++, sock, room: null, name: '', cid: '', done: false, spec: false, st: null, top: 0, color: 0 }; let buf = Buffer.alloc(0);
  sock.on('data', chunk => {
    buf = Buffer.concat([buf, chunk]);
    if (buf.length > 1 << 16) { sock.destroy(); return; }
    for (;;) {
      if (buf.length < 2) return;
      const op = buf[0] & 15, masked = buf[1] & 128; let len = buf[1] & 127, off = 2;
      if (len === 126) { if (buf.length < 4) return; len = buf.readUInt16BE(2); off = 4; } else if (len === 127) { sock.destroy(); return; }
      if (!masked) { sock.destroy(); return; }
      if (buf.length < off + 4 + len) return;
      const mask = buf.subarray(off, off + 4), body = Buffer.from(buf.subarray(off + 4, off + 4 + len));
      for (let i = 0; i < len; i++) body[i] ^= mask[i & 3];
      buf = buf.subarray(off + 4 + len);
      if (op === 8) { sock.end(); return; }
      if (op === 9) { sock.write(Buffer.concat([Buffer.from([0x8a, body.length]), body])); continue; }
      if (op === 1) { let m = null; try { m = JSON.parse(body.toString('utf8')); } catch (e) {} try { onMessage(c, m); } catch (e) { console.error('message error:', e.message); } }
    }
  });
  const gone = () => leave(c);
  sock.on('close', gone); sock.on('error', gone);
});

server.listen(PORT, '0.0.0.0', () => {
  console.log('Long Climb is being served. Open one of these in a browser:');
  console.log('  on this PC:        http://localhost:' + PORT);
  for (const list of Object.values(os.networkInterfaces())) for (const a of list || []) if (a.family === 'IPv4' && !a.internal) console.log('  on the same Wi-Fi: http://' + a.address + ':' + PORT);
  console.log('Players far away need a tunnel to this port, for example:  cloudflared tunnel --url http://localhost:' + PORT);
  console.log('Stop the server with Ctrl+C.');
});
