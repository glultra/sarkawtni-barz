// PROTOTYPE - NOT FOR PRODUCTION
// Question: Is one long, punishing climb fun to play and to watch for an hour?
// Date: 2026-11-16
// Long Climb's room server. Run it on the PC that hosts the race:   node server.js   (or: node server.js 9000)
// It serves this folder as a website and passes messages between the players of a room.
// It needs nothing installed but Node. Players on the same Wi-Fi open the address it prints;
// players elsewhere need a tunnel to this port (for example: cloudflared tunnel --url http://localhost:8080).
'use strict';
const http = require('http'), fs = require('fs'), path = require('path'), crypto = require('crypto'), os = require('os');
const PORT = Number(process.argv[2]) || 8080, ROOT = __dirname, MAX = 5;
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
const GOALS = [3, 5, 10, 25, 50];
function roster(room, type, to) {
  const m = { t: type, id: room.id, host: room.host, mode: room.mode, goal: room.goal, players: room.players.map(p => ({ id: p.id, name: p.name })) };
  if (to) send(to, Object.assign({ you: to.id }, m)); else room.players.forEach(p => send(p, m));
}
function leave(c) {
  const room = c.room; if (!room) return; c.room = null;
  room.players = room.players.filter(p => p !== c);
  if (!room.players.length) { rooms.delete(room.id); return; }
  if (room.host === c.id) room.host = room.players[0].id; // the next player takes the room over
  roster(room, 'roster');
}
function onMessage(c, m) {
  if (!m || typeof m.t !== 'string') return;
  if (m.t === 'create' && !c.room) {
    const room = { id: newRoomId(), host: c.id, players: [c], mode: [0, 1, 2].includes(m.mode) ? m.mode : 1, goal: GOALS.includes(m.goal) ? m.goal : 5, racing: false, place: 0 };
    c.name = cleanName(m.name); c.room = room; rooms.set(room.id, room); roster(room, 'room', c); return;
  }
  if (m.t === 'join' && !c.room) {
    const room = rooms.get(String(m.id || '').toUpperCase());
    if (!room) { send(c, { t: 'err', msg: 'none' }); return; }
    if (room.players.length >= MAX) { send(c, { t: 'err', msg: 'full' }); return; }
    c.name = cleanName(m.name); c.done = false; c.room = room; room.players.push(c); roster(room, 'room', c); roster(room, 'roster');
    if (room.racing) send(c, { t: 'go', mode: room.mode, goal: room.goal }); // a late player starts climbing at once
    return;
  }
  const room = c.room; if (!room) return;
  if (m.t === 's') { const out = { t: 's', id: c.id, x: +m.x || 0, a: +m.a || 0, f: m.f < 0 ? -1 : 1, l: +m.l || 1 }; room.players.forEach(p => { if (p !== c) send(p, out); }); return; }
  if (m.t === 'hit') { const to = room.players.find(p => p.id === m.to); if (to && to !== c && (m.k === 'push' || m.k === 'grab')) send(to, { t: 'hit', from: c.id, k: m.k, d: m.d < 0 ? -1 : 1 }); return; }
  if (m.t === 'fin') { if (room.racing && !c.done) { c.done = true; room.place++; const out = { t: 'fin', id: c.id, cs: Math.max(0, Math.round(+m.cs || 0)), place: room.place }; room.players.forEach(p => send(p, out)); } return; }
  if (room.host !== c.id) return; // what follows is the host's alone
  if (m.t === 'set' && !room.racing) { if ([0, 1, 2].includes(m.mode)) room.mode = m.mode; if (GOALS.includes(m.goal)) room.goal = m.goal; roster(room, 'roster'); }
  else if (m.t === 'go' && !room.racing) { room.racing = true; room.place = 0; room.players.forEach(p => { p.done = false; send(p, { t: 'go', mode: room.mode, goal: room.goal }); }); }
  else if (m.t === 'end') { room.racing = false; room.players.forEach(p => send(p, { t: 'lobby' })); roster(room, 'roster'); }
}

// ---- WebSocket, by hand: text messages only, small ones ------------------------------------
function send(c, obj) {
  if (c.sock.destroyed) return;
  const data = Buffer.from(JSON.stringify(obj)), n = data.length;
  if (n >= 65536) return;
  const head = n < 126 ? Buffer.from([0x81, n]) : Buffer.from([0x81, 126, n >> 8, n & 255]);
  c.sock.write(Buffer.concat([head, data]));
}
server.on('upgrade', (req, sock) => {
  const key = req.headers['sec-websocket-key'];
  if (!key) { sock.destroy(); return; }
  const accept = crypto.createHash('sha1').update(key + '258EAFA5-E914-47DA-95CA-C5AB0DC85B11').digest('base64');
  sock.write('HTTP/1.1 101 Switching Protocols\r\nUpgrade: websocket\r\nConnection: Upgrade\r\nSec-WebSocket-Accept: ' + accept + '\r\n\r\n');
  const c = { id: nextId++, sock, room: null, name: '', done: false }; let buf = Buffer.alloc(0);
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
