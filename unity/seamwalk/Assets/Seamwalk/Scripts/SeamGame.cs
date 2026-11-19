using System.Collections.Generic;
using System.Globalization;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace Seamwalk
{
    /// <summary>
    /// The whole game: it builds each space in code, walks the player through it on a fixed 60 Hz step,
    /// draws the world through every portal face, and shows the HUD. It starts itself when Play begins,
    /// so the scene needs nothing placed in it.
    /// </summary>
    public class SeamGame : MonoBehaviour
    {
        public static SeamGame I;
        /// <summary>The direction towards the sun, shared by the light and the sky.</summary>
        public static readonly Vector3 SunDirection = new Vector3(-0.5f, 0.72f, -0.48f).normalized;

        const float STEP = 1f / 60f, PI = Mathf.PI;
        SeamConfig cfg;
        Level L;
        Camera cam;
        Light sun;
        readonly Camera[] vcams = new Camera[4];
        readonly List<List<RenderTexture>> pool = new List<List<RenderTexture>>();
        int poolW, poolH;

        // input, by the browser game's key names, so one input log drives both versions
        readonly HashSet<string> down = new HashSet<string>(), hit = new HashSet<string>();
        static readonly (Key key, string code)[] KEYS =
        {
            (Key.W, "KeyW"), (Key.A, "KeyA"), (Key.S, "KeyS"), (Key.D, "KeyD"), (Key.E, "KeyE"), (Key.R, "KeyR"), (Key.P, "KeyP"), (Key.M, "KeyM"),
            (Key.UpArrow, "ArrowUp"), (Key.DownArrow, "ArrowDown"), (Key.LeftArrow, "ArrowLeft"), (Key.RightArrow, "ArrowRight"),
            (Key.Space, "Space"), (Key.Enter, "Enter"), (Key.Escape, "Escape"),
        };
        bool scripted, locked, muted;

        // the walker
        class Walker
        {
            public float x, y, z, yaw, pitch, yawT, pitchT, vx, vz, vy, walk, ey;
            public float px, py, pz, pyaw, ppitch, pey;
            public Zone Z;
            public bool air;
            public int steps;
            public Cube hold;
            public Face cross;
        }
        Walker P;

        // the run
        string scene = "title", levelName = "", prompt = "", gained = "";
        int level;
        float t, fade, fadeDir, acc, alpha = 1f;
        bool won, solved, done;
        public readonly List<string> events = new List<string>();

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        static void Boot()
        {
            if (I != null) return;
            foreach (var c in FindObjectsByType<Camera>()) Destroy(c.gameObject);
            foreach (var l in FindObjectsByType<Light>()) Destroy(l.gameObject);
            foreach (var v in FindObjectsByType<Volume>()) Destroy(v.gameObject);
            new GameObject("Seamwalk").AddComponent<SeamGame>();
        }

        void Awake()
        {
            I = this;
            cfg = SeamConfig.Load();
            World.Init();
            SetUpRendering();
            SetUpAudio();
            ResetRun();
        }

        // ---- rendering set-up: the sun, the sky light, the camera and the finish on the picture --------
        void SetUpRendering()
        {
            if (GraphicsSettings.currentRenderPipeline is UniversalRenderPipelineAsset urp)
            {
                urp.msaaSampleCount = 4;
                urp.shadowDistance = cfg.shadowDistance;
            }
            var sgo = new GameObject("Sun"); sgo.transform.SetParent(transform, false);
            sun = sgo.AddComponent<Light>();
            sun.type = LightType.Directional; sun.shadows = LightShadows.Soft; sun.shadowStrength = 0.72f;
            sun.transform.rotation = Quaternion.LookRotation(-SunDirection);
            RenderSettings.sun = sun;
            RenderSettings.ambientMode = AmbientMode.Trilight;
            RenderSettings.ambientSkyColor = new Color(0.86f, 0.9f, 1f) * cfg.ambient;
            RenderSettings.ambientEquatorColor = new Color(0.8f, 0.78f, 0.78f) * cfg.ambient;
            RenderSettings.ambientGroundColor = new Color(0.56f, 0.52f, 0.5f) * cfg.ambient;
            RenderSettings.fog = true; RenderSettings.fogMode = FogMode.Linear; RenderSettings.fogStartDistance = 40f; RenderSettings.fogEndDistance = 118f;
            RenderSettings.skybox = null;

            var cgo = new GameObject("Eyes"); cgo.transform.SetParent(transform, false); cgo.tag = "MainCamera";
            cam = cgo.AddComponent<Camera>();
            cam.nearClipPlane = 0.03f; cam.farClipPlane = 180f; cam.clearFlags = CameraClearFlags.SolidColor; cam.allowHDR = true; cam.allowMSAA = true;
            cgo.AddComponent<AudioListener>();
            var data = cam.GetUniversalAdditionalCameraData();
            data.renderPostProcessing = true; data.antialiasing = AntialiasingMode.SubpixelMorphologicalAntiAliasing; data.renderShadows = true;

            for (int i = 0; i < vcams.Length; i++)
            {
                var vgo = new GameObject("Beyond" + i); vgo.transform.SetParent(transform, false);
                var vc = vgo.AddComponent<Camera>();
                vc.enabled = false; vc.clearFlags = CameraClearFlags.SolidColor; vc.allowHDR = true; vc.allowMSAA = true;
                var vd = vc.GetUniversalAdditionalCameraData();
                vd.renderPostProcessing = false; vd.renderShadows = true;
                vcams[i] = vc;
            }

            var pgo = new GameObject("Finish"); pgo.transform.SetParent(transform, false);
            var vol = pgo.AddComponent<Volume>(); vol.isGlobal = true;
            var prof = ScriptableObject.CreateInstance<VolumeProfile>(); vol.sharedProfile = prof;
            var bloom = prof.Add<Bloom>(true); bloom.intensity.value = cfg.bloom; bloom.threshold.value = 1.0f; bloom.scatter.value = 0.72f;
            var tone = prof.Add<Tonemapping>(true); tone.mode.value = TonemappingMode.Neutral;
            var grade = prof.Add<ColorAdjustments>(true); grade.contrast.value = 8f; grade.saturation.value = 10f; grade.postExposure.value = 0.05f;
            var vig = prof.Add<Vignette>(true); vig.intensity.value = 0.2f; vig.smoothness.value = 0.45f;
        }

        void ApplyTheme(Theme th, Camera c)
        {
            sun.color = th.sun; sun.intensity = cfg.sunIntensity * th.sunScale;
            RenderSettings.fogColor = th.fog;
            c.backgroundColor = th.horizon;
        }

        // ---- the run ------------------------------------------------------------------------------
        void ResetRun()
        {
            t = 0; fade = 0; fadeDir = 0; done = false; gained = ""; scene = "title";
            LoadLevel(0);
        }

        void LoadLevel(int i)
        {
            if (L != null)
            {
                foreach (var c in L.cubes) if (c.tr != null) Destroy(c.tr.gameObject);
                Destroy(L.root.gameObject);
            }
            L = new Level { index = i, root = new GameObject("Space" + (i + 1)).transform };
            Spaces.Build(i, new Builder(L));
            P = new Walker { x = L.sx, y = L.sy, z = L.sz, yaw = L.syaw, yawT = L.syaw, Z = L.startZ, px = L.sx, py = L.sy, pz = L.sz, pyaw = L.syaw };
            P.ey = P.pey = P.y + cfg.eyeHeight;
            level = i + 1; levelName = L.name; won = false; solved = false; prompt = "";
            StepWorld(0);
        }

        /// <summary>Puts the walker somewhere in a space: for looking at a spot, never part of play.</summary>
        public void Place(int space, float x, float z, float yawDeg, int zone = 0, float y = 0f)
        {
            if (L == null || L.index != space - 1) LoadLevel(space - 1);
            var Z = L.zones[Mathf.Clamp(zone, 0, L.zones.Count - 1)];
            P.Z = Z; P.x = P.px = x + Z.ox; P.z = P.pz = z; P.yaw = P.yawT = P.pyaw = yawDeg * Mathf.Deg2Rad; P.pitch = P.pitchT = P.ppitch = 0;
            P.y = P.py = y; P.ey = P.pey = y + cfg.eyeHeight; P.cross = null; P.vx = P.vz = P.vy = 0;
            scene = "play"; fade = 0; fadeDir = 0;
        }

        /// <summary>The run's state in one line, for a check to read.</summary>
        public string StateLine()
        {
            return string.Format(CultureInfo.InvariantCulture, "scene={0} level={1} life={2} zone={3} carrying={4} solved={5} won={6} done={7} x={8:0.00} z={9:0.00} y={10:0.00} t={11:0.00}",
                scene, level, P.Z.th.red ? "red" : "colour", P.Z.idx, P.hold != null, solved, won, done, P.x - P.Z.ox, P.z, P.y, t);
        }

        /// <summary>
        /// Plays an input log from a fresh start with no drawing: "Enter" is a press, "ArrowUp:1.5" a hold in
        /// seconds, "wait:2" a pause. The same log that finishes the browser game should finish this one.
        /// </summary>
        public string RunLog(string log)
        {
            scripted = true; down.Clear(); hit.Clear(); events.Clear();
            ResetRun();
            Step();
            foreach (var tok in log.Split(new[] { ' ' }, System.StringSplitOptions.RemoveEmptyEntries))
            {
                var parts = tok.Split(':');
                int n = parts.Length > 1 ? Mathf.RoundToInt(float.Parse(parts[1], CultureInfo.InvariantCulture) * 60f) : 2;
                if (parts[0] != "wait") { down.Add(parts[0]); hit.Add(parts[0]); }
                for (int i = 0; i < n; i++) Step();
                down.Remove(parts[0]);
            }
            for (int i = 0; i < 30; i++) Step();
            scripted = false;
            return StateLine();
        }

        // ---- frame and step -------------------------------------------------------------------------
        void Update()
        {
            if (!scripted) ReadInput();
            acc += Mathf.Min(Time.unscaledDeltaTime, 0.25f);
            while (acc >= STEP) { Step(); acc -= STEP; }
            alpha = acc / STEP;
        }

        void ReadInput()
        {
            var kb = Keyboard.current; var mouse = Mouse.current;
            if (kb != null)
                foreach (var (key, code) in KEYS)
                {
                    var k = kb[key];
                    if (k.wasPressedThisFrame) hit.Add(code);
                    if (k.isPressed) down.Add(code); else down.Remove(code);
                }
            if (mouse == null) return;
            if (mouse.leftButton.wasPressedThisFrame)
            {
                if (scene == "title") hit.Add("Enter"); else if (scene == "pause") hit.Add("KeyP");
                if (scene != "results") SetLock(true);
            }
            if (locked && scene == "play" && P != null)
            {
                var d = mouse.delta.ReadValue();
                if (Mathf.Abs(d.x) < 400 && Mathf.Abs(d.y) < 400) // a stray jump is not the hand
                {
                    P.yawT -= d.x * cfg.lookSens;
                    P.pitchT = Mathf.Clamp(P.pitchT + d.y * cfg.lookSens, -1.45f, 1.45f);
                }
            }
        }

        void SetLock(bool on)
        {
            locked = on;
            Cursor.lockState = on ? CursorLockMode.Locked : CursorLockMode.None;
            Cursor.visible = !on;
        }

        bool Pressed(string code) => hit.Contains(code);

        void Step()
        {
            if (Pressed("KeyM")) { muted = !muted; AudioListener.volume = muted ? 0f : cfg.volume; }
            switch (scene)
            {
                case "title":
                    t += STEP; StepWorld(0);
                    if (Pressed("Enter") || Pressed("Space")) { t = 0; scene = "play"; }
                    break;
                case "play": StepPlay(STEP); break;
                case "pause":
                    if (Pressed("KeyP") || Pressed("Escape") || Pressed("Enter")) { scene = "play"; if (!scripted) SetLock(true); }
                    break;
                case "results":
                    t += STEP; StepWorld(0);
                    if (Pressed("Enter")) { ResetRun(); scene = "play"; if (!scripted) SetLock(true); }
                    break;
            }
            hit.Clear();
        }

        void StepPlay(float dt)
        {
            if (Pressed("KeyP") || Pressed("Escape")) { SetLock(false); scene = "pause"; return; }
            t += dt;
            if (fadeDir != 0)
            {
                fade += fadeDir * dt / cfg.fadeTime;
                if (fadeDir > 0 && fade >= 1) { fade = 1; NextSpace(); if (scene != "play") return; }
                else if (fadeDir < 0 && fade <= 0) { fade = 0; fadeDir = 0; }
            }
            if (Pressed("KeyR") && !won) { float f = fade; LoadLevel(L.index); fade = f; events.Add("restart " + level); return; }
            StepWalker(dt); Interact(); StepWorld(dt);
        }

        void NextSpace()
        {
            if (L.index + 1 < Spaces.Count) { LoadLevel(L.index + 1); fadeDir = -1; return; }
            done = true; gained = "You walked every seam"; fade = 0; fadeDir = 0; SetLock(false); scene = "results";
        }

        // ---- the walker: the browser game's rules, line for line ---------------------------------------
        float FloorAt(float x, float z, float y)
        {
            float best = float.NegativeInfinity;
            foreach (var r in L.regions)
            {
                if (x < r.x0 || x > r.x1 || z < r.z0 || z > r.z1) continue;
                float h = r.ya == r.yb ? r.ya : Mathf.Lerp(r.ya, r.yb, r.alongX ? (x - r.x0) / (r.x1 - r.x0) : (z - r.z0) / (r.z1 - r.z0));
                if (h <= y + 0.55f && h > best) best = h;
            }
            return best;
        }

        static bool PushOut(ref float x, ref float z, float y, float r, float x0, float z0, float x1, float z1, float y0, float y1)
        {
            if (y + 1.7f <= y0 || y >= y1 - 0.01f) return false;
            float cx = Mathf.Clamp(x, x0, x1), cz = Mathf.Clamp(z, z0, z1), dx = x - cx, dz = z - cz, d2 = dx * dx + dz * dz;
            if (d2 >= r * r) return false;
            if (d2 > 1e-9f) { float d = Mathf.Sqrt(d2); x = cx + dx / d * r; z = cz + dz / d * r; return true; }
            float l = x - x0, rr = x1 - x, tt = z - z0, b = z1 - z, m = Mathf.Min(Mathf.Min(l, rr), Mathf.Min(tt, b));
            if (m == l) x = x0 - r; else if (m == rr) x = x1 + r; else if (m == tt) z = z0 - r; else z = z1 + r;
            return true;
        }

        void Collide(ref float x, ref float z, float y, float r = 0.3f)
        {
            for (int it = 0; it < 3; it++)
            {
                foreach (var c in L.solids) if (!c.off) PushOut(ref x, ref z, y, r, c.x0, c.z0, c.x1, c.z1, c.y0, c.y1);
                foreach (var c in L.cubes) if (!c.held) PushOut(ref x, ref z, y, r, c.x - 0.36f, c.z - 0.36f, c.x + 0.36f, c.z + 0.36f, c.y, c.y + 0.7f);
            }
        }

        bool Blocked(float x, float z, float y, float r)
        {
            foreach (var c in L.solids) { if (c.off || c.water) continue; float tx = x, tz = z; if (PushOut(ref tx, ref tz, y, r, c.x0, c.z0, c.x1, c.z1, c.y0, c.y1)) return true; }
            foreach (var c in L.cubes) { if (c.held) continue; float tx = x, tz = z; if (PushOut(ref tx, ref tz, y, r, c.x - 0.36f, c.z - 0.36f, c.x + 0.36f, c.z + 0.36f, c.y, c.y + 0.7f)) return true; }
            return false;
        }

        // the face this move walks into from the front, if any
        Face Crossing(float x0, float z0, float x1, float z1, float y)
        {
            foreach (var f in L.faces)
            {
                float d0 = (x0 - f.x) * f.nx + (z0 - f.z) * f.nz, d1 = (x1 - f.x) * f.nx + (z1 - f.z) * f.nz;
                if (!(d0 > 0 && d1 <= 0) || d0 > 5) continue;
                if (y < f.y - 0.6f || y > f.y + Mathf.Max(0.7f, f.h - 1.3f)) continue;
                float k = d0 / (d0 - d1), lx = (Mathf.Lerp(x0, x1, k) - f.x) * f.rx + (Mathf.Lerp(z0, z1, k) - f.z) * f.rz;
                if (Mathf.Abs(lx) <= f.w / 2) return f;
            }
            return null;
        }

        // carry the walker out of the linked face, with nothing to see but the new place
        void Through(Face f)
        {
            var tg = f.target; float oy = P.y;
            var v = f.Map(new Vector3(P.x, P.y, P.z)); P.x = v.x; P.y = v.y; P.z = v.z;
            v = f.Map(new Vector3(P.px, P.py, P.pz)); P.px = v.x; P.py = v.y; P.pz = v.z;
            P.ey += P.y - oy; P.pey += P.y - oy;
            float c = Mathf.Cos(f.rot), s = Mathf.Sin(f.rot), vx = P.vx;
            P.vx = vx * c + P.vz * s; P.vz = -vx * s + P.vz * c;
            P.yaw += f.rot; P.yawT += f.rot; P.pyaw += f.rot;
            float d = (P.x - tg.x) * tg.nx + (P.z - tg.z) * tg.nz;
            if (d < 0.002f) { P.x += tg.nx * (0.002f - d); P.z += tg.nz * (0.002f - d); }
            P.cross = tg;
            if (tg.Z.th.red != P.Z.th.red) { Sfx(tg.Z.th.red ? "toRed" : "toColour"); events.Add("life " + (tg.Z.th.red ? "red" : "colour")); }
            events.Add("through " + f.Z.idx + " " + tg.Z.idx);
            P.Z = tg.Z;
        }

        void StepWalker(float dt)
        {
            P.px = P.x; P.py = P.y; P.pz = P.z; P.pyaw = P.yaw; P.ppitch = P.pitch; P.pey = P.ey; P.cross = null;
            bool free = !won;
            if (free) P.yawT += ((down.Contains("ArrowLeft") ? 1 : 0) - (down.Contains("ArrowRight") ? 1 : 0)) * cfg.turnRate * dt;
            float k = cfg.lookSmooth > 0 ? 1 - Mathf.Exp(-dt / cfg.lookSmooth) : 1;
            P.yaw += (P.yawT - P.yaw) * k; P.pitch += (P.pitchT - P.pitch) * k;
            float f = 0, s = 0;
            if (free)
            {
                f = (down.Contains("KeyW") || down.Contains("ArrowUp") ? 1 : 0) - (down.Contains("KeyS") || down.Contains("ArrowDown") ? 1 : 0);
                s = (down.Contains("KeyD") ? 1 : 0) - (down.Contains("KeyA") ? 1 : 0);
            }
            float n = Mathf.Sqrt(f * f + s * s); if (n > 1) { f /= n; s /= n; }
            float sy = Mathf.Sin(P.yaw), cy = Mathf.Cos(P.yaw);
            float wx = (-sy * f + cy * s) * cfg.moveSpeed, wz = (-cy * f - sy * s) * cfg.moveSpeed;
            float dvx = wx - P.vx, dvz = wz - P.vz, dl = Mathf.Sqrt(dvx * dvx + dvz * dvz), md = cfg.moveAccel * dt;
            if (dl <= md) { P.vx = wx; P.vz = wz; } else { P.vx += dvx / dl * md; P.vz += dvz / dl * md; }
            float nx = P.x + P.vx * dt, nz = P.z + P.vz * dt;
            Collide(ref nx, ref nz, P.y);
            float fl = FloorAt(nx, nz, P.y);
            if (float.IsNegativeInfinity(fl))
            { // no floor that way: slide along what there is
                float ax = nx, az = P.z, bx = P.x, bz = nz;
                Collide(ref ax, ref az, P.y); Collide(ref bx, ref bz, P.y);
                if (!float.IsNegativeInfinity(FloorAt(ax, az, P.y))) { nx = ax; nz = az; }
                else if (!float.IsNegativeInfinity(FloorAt(bx, bz, P.y))) { nx = bx; nz = bz; }
                else { nx = P.x; nz = P.z; }
                fl = FloorAt(nx, nz, P.y);
                if (float.IsNegativeInfinity(fl)) fl = P.y;
            }
            float x0 = P.x, z0 = P.z, moved = Mathf.Sqrt((nx - x0) * (nx - x0) + (nz - z0) * (nz - z0));
            P.x = nx; P.z = nz;
            if (P.y > fl + 1e-4f)
            { // off a ledge: fall to the floor below
                P.vy -= cfg.gravity * dt; P.y += P.vy * dt; P.air = true;
                if (P.y <= fl) { if (P.vy < -5) Sfx("land"); P.y = fl; P.vy = 0; P.air = false; }
            }
            else { P.y = fl; P.vy = 0; P.air = false; }
            var fc = Crossing(x0, z0, P.x, P.z, P.y); if (fc != null) Through(fc);
            if (!P.air && moved > 0) { P.walk += moved; if (P.walk > 2.1f) { P.walk -= 2.1f; Sfx(P.steps++ % 2 == 1 ? "stepA" : "stepB"); } }
            float speed = Mathf.Sqrt(P.vx * P.vx + P.vz * P.vz), bob = P.air ? 0 : Mathf.Sin(P.walk / 2.1f * PI * 2) * cfg.bob * Mathf.Min(1, speed / 2);
            P.ey += (P.y + cfg.eyeHeight + bob - P.ey) * Mathf.Min(1, dt * 18);
        }

        // ---- cubes, switches, doors and the gate ----------------------------------------------------
        bool InWater(Zone Zt, float x, float z)
        {
            for (int i = 0; i < L.waters.Count; i++) { var w = L.waters[i]; if (L.waterZones[i] == Zt && x > w.x0 && x < w.x1 && z > w.z0 && z < w.z1) return true; }
            return false;
        }

        Cube Reachable()
        {
            Cube best = null; float bd = cfg.reach, fx = -Mathf.Sin(P.yaw), fz = -Mathf.Cos(P.yaw);
            foreach (var c in L.cubes)
            {
                if (c.held || c.Z != P.Z || Mathf.Abs(c.y - P.y) > 1.6f) continue;
                float dx = c.x - P.x, dz = c.z - P.z, d = Mathf.Sqrt(dx * dx + dz * dz);
                if (d < bd && (d < 0.8f || (dx * fx + dz * fz) / d > 0.55f)) { best = c; bd = d; }
            }
            return best;
        }

        void PutDown()
        {
            var c = P.hold; float fx = -Mathf.Sin(P.yaw), fz = -Mathf.Cos(P.yaw), d = cfg.putDist;
            float tx = P.x + fx * d, tz = P.z + fz * d, ty = P.y; var Zt = P.Z;
            var f = Crossing(P.x, P.z, tx, tz, P.y);
            if (f != null) { var v = f.Map(new Vector3(tx, ty, tz)); tx = v.x; ty = v.y; tz = v.z; Zt = f.target.Z; } // set down through an arch
            else if (Blocked(P.x + fx * d * 0.55f, P.z + fz * d * 0.55f, P.y, 0.2f)) { Sfx("no"); return; }
            bool wet = InWater(Zt, tx, tz);
            if (!wet && Blocked(tx, tz, ty, 0.36f)) { Sfx("no"); return; }
            float fl = FloorAt(tx, tz, ty);
            if (float.IsNegativeInfinity(fl)) { Sfx("no"); return; }
            c.tr.SetParent(L.root, false); c.tr.localRotation = Quaternion.identity; c.held = false; P.hold = null;
            if (wet) { c.Z = c.homeZ; c.x = c.hx; c.y = c.hy; c.z = c.hz; c.drop = 1.2f; Sfx("splash"); events.Add("cube returned"); } // lost to the water: it comes back where it began
            else { c.Z = Zt; c.x = tx; c.z = tz; c.y = fl; c.drop = Mathf.Max(0.5f, ty + 0.6f - fl); Sfx("put"); events.Add("cube down " + Zt.idx); }
        }

        void Interact()
        {
            var c = P.hold != null ? null : Reachable();
            prompt = P.hold != null ? "E: put the cube down" : c != null ? "E: pick up the cube" : "";
            if (won || !(Pressed("KeyE") || Pressed("Space"))) return;
            if (P.hold != null) PutDown();
            else if (c != null)
            {
                c.held = true; P.hold = c; c.tr.SetParent(cam.transform, false);
                c.tr.localPosition = new Vector3(0.42f, -0.46f, 1.05f); c.tr.localRotation = Quaternion.Euler(-8f, -28f, 0f);
                Sfx("pick"); events.Add("cube up");
            }
        }

        void StepWorld(float dt)
        {
            bool heldY = false, heldP = false;
            foreach (var s in L.switches)
            {
                bool on = (P.Z == s.Z && Mathf.Abs(P.y - s.y) < 0.5f && Dist(P.x - s.x, P.z - s.z) < 0.75f);
                if (!on) foreach (var c in L.cubes) if (!c.held && c.Z == s.Z && Mathf.Abs(c.y - s.y) < 0.5f && Dist(c.x - s.x, c.z - s.z) < 0.8f) { on = true; break; }
                if (on != s.down) { s.down = on; if (dt > 0) Sfx("sw"); events.Add("switch " + s.key + " " + on); }
                if (on) { if (s.key == 'y') heldY = true; else heldP = true; }
                var lp = s.plate.position; lp.y = s.y + (on ? 0.02f : 0.06f); s.plate.position = lp;
            }
            foreach (var d in L.doors)
            {
                float want = (d.key == 'y' ? heldY : heldP) ? 1 : 0;
                if (want != d.open && !d.moving) { d.moving = true; if (dt > 0) Sfx("door"); }
                d.open = want > d.open ? Mathf.Min(1, d.open + cfg.doorSpeed * dt) : Mathf.Max(0, d.open - cfg.doorSpeed * dt);
                if (d.open == want) d.moving = false;
                d.col.off = d.open > 0.85f;
                d.grp.localScale = new Vector3(1, Mathf.Max(0.02f, 1 - d.open), 1); d.grp.gameObject.SetActive(d.open < 0.99f);
            }
            foreach (var c in L.cubes)
            {
                if (c.held) continue;
                c.drop = Mathf.Max(0, c.drop - dt * Mathf.Max(2, c.drop * 6));
                c.tr.position = World.U(c.x, c.y + 0.35f + c.drop, c.z);
            }
            var g = L.gate;
            if (g == null) return;
            float near = P.Z == g.Z && Mathf.Abs(P.y - g.y) < 0.6f ? Dist(P.x - g.x, P.z - g.z) : float.PositiveInfinity;
            if (!g.lit)
            {
                bool open;
                if (L.gateKey != '\0') { open = true; foreach (var d in L.doors) if (d.key == L.gateKey && d.open <= 0.85f) open = false; }
                else open = near < cfg.gateLightDist;
                if (open)
                {
                    g.lit = true; solved = true; if (dt > 0) Sfx("solved"); events.Add("solved " + level);
                    g.starMat.SetColor("_BaseColor", World.Hex(0xffe066)); World.Glow(g.starMat, World.Hex(0xffd23f) * 2.2f);
                    g.sheet.SetActive(true); g.bars.SetActive(false); g.glow.intensity = 12f;
                }
            }
            if (!g.lit) g.starMat.SetColor("_BaseColor", World.Hex(0x9aa0a8));
            g.star.localRotation = Quaternion.Euler(0, t * (g.lit ? 115f : 23f), 0);
            if (g.lit && !won && near < 1.0f && dt > 0) { won = true; fadeDir = 1; Sfx("win"); events.Add("won " + level); }
        }

        static float Dist(float a, float b) => Mathf.Sqrt(a * a + b * b);

        // ---- drawing the world through its faces ------------------------------------------------------
        class Seen { public Face f; public Vector4 rect; public RenderTexture rt; }
        readonly List<Seen>[] seenAt = { new List<Seen>(), new List<Seen>(), new List<Seen>(), new List<Seen>(), new List<Seen>() };
        readonly Plane[] planes = new Plane[6];
        static readonly int MainTex = Shader.PropertyToID("_MainTex"), FlatMix = Shader.PropertyToID("_FlatMix"), FlatColor = Shader.PropertyToID("_FlatColor");
        static readonly Matrix4x4 Half = Matrix4x4.Rotate(Quaternion.Euler(0, 180, 0));
        static readonly Vector4 FULL = new Vector4(-1, -1, 1, 1);
        Matrix4x4 baseProj;

        void LateUpdate()
        {
            if (L == null || P == null) return;
            if (!Mathf.Approximately(cam.fieldOfView, cfg.fov)) cam.fieldOfView = cfg.fov;
            Vector3 eye; float yaw, pitch; Zone Zv;
            if (scene == "title" || scene == "results")
            {
                eye = new Vector3(L.sx, L.sy + cfg.eyeHeight, L.sz); yaw = L.syaw + Mathf.Sin(Time.unscaledTime * 0.35f) * (scene == "title" ? 0.12f : 0.5f); pitch = -0.02f; Zv = L.startZ;
            }
            else
            {
                float a = scene == "pause" ? 1f : alpha;
                eye = new Vector3(Mathf.Lerp(P.px, P.x, a), Mathf.Lerp(P.pey, P.ey, a), Mathf.Lerp(P.pz, P.z, a));
                yaw = Mathf.Lerp(P.pyaw, P.yaw, a); pitch = Mathf.Lerp(P.ppitch, P.pitch, a); Zv = P.Z;
                var tg = P.cross;
                if (tg != null && (eye.x - tg.x) * tg.nx + (eye.z - tg.z) * tg.nz <= 0.0005f)
                { // between two steps the eye is still on the side it came from
                    eye = tg.Map(eye); yaw += tg.rot; Zv = tg.target.Z;
                }
            }
            cam.transform.SetPositionAndRotation(World.U(eye.x, eye.y, eye.z), Quaternion.Euler(-pitch * Mathf.Rad2Deg, -yaw * Mathf.Rad2Deg, 0));
            if (Screen.width != poolW || Screen.height != poolH) FreeTargets();
            cam.ResetProjectionMatrix(); baseProj = cam.projectionMatrix;
            View(cam, 0, null, null, Zv.th, FULL);
        }

        void FreeTargets()
        {
            foreach (var row in pool) foreach (var rt in row) if (rt != null) { rt.Release(); Destroy(rt); }
            pool.Clear(); poolW = Screen.width; poolH = Screen.height;
        }

        RenderTexture Target(int depth, int i)
        {
            while (pool.Count <= depth) pool.Add(new List<RenderTexture>());
            var row = pool[depth];
            while (row.Count <= i) row.Add(null);
            if (row[i] == null)
            {
                row[i] = new RenderTexture(Mathf.Max(16, poolW), Mathf.Max(16, poolH), 24, RenderTextureFormat.DefaultHDR) { name = "beyond-" + depth + "-" + i, useMipMap = false };
                row[i].Create();
            }
            return row[i];
        }

        Vector4 ScreenRect(Face f, Matrix4x4 vp)
        {
            float x0 = 1, y0 = 1, x1 = -1, y1 = -1;
            foreach (var c in f.corners)
            {
                var v = vp * new Vector4(c.x, c.y, c.z, 1);
                if (v.w <= 0.02f) return FULL;
                float x = v.x / v.w, y = v.y / v.w;
                x0 = Mathf.Min(x0, x); y0 = Mathf.Min(y0, y); x1 = Mathf.Max(x1, x); y1 = Mathf.Max(y1, y);
            }
            return new Vector4(Mathf.Max(-1, x0), Mathf.Max(-1, y0), Mathf.Min(1, x1), Mathf.Min(1, y1));
        }

        void View(Camera c, int depth, RenderTexture into, Face skip, Theme theme, Vector4 rect)
        {
            Vector3 cp = c.transform.position; Quaternion cr = c.transform.rotation;
            var seen = seenAt[depth]; seen.Clear();
            if (depth < Mathf.Min(cfg.portalDepth, vcams.Length))
            {
                var vp = baseProj * Matrix4x4.Scale(new Vector3(1, 1, -1)) * Matrix4x4.TRS(cp, cr, Vector3.one).inverse;
                GeometryUtility.CalculateFrustumPlanes(vp, planes);
                foreach (var f in L.faces)
                {
                    if (skip != null && (f == skip || f == skip.twin)) continue;
                    float d = Vector3.Dot(cp - f.uPos, f.uNormal);
                    if (d <= 0.0005f || d > 70f || Mathf.Abs(cp.x - f.uPos.x) > 200f) continue;
                    if (skip != null && Vector3.Dot(f.uCentre - skip.uPos, skip.uNormal) < -f.radius) continue;
                    if (!GeometryUtility.TestPlanesAABB(planes, f.bounds)) continue;
                    var r = ScreenRect(f, vp);
                    float x0 = Mathf.Max(r.x, rect.x), y0 = Mathf.Max(r.y, rect.y), x1 = Mathf.Min(r.z, rect.z), y1 = Mathf.Min(r.w, rect.w);
                    if (x1 <= x0 || y1 <= y0) continue;
                    seen.Add(new Seen { f = f, rect = new Vector4(x0, y0, x1, y1) });
                }
            }
            for (int i = 0; i < seen.Count; i++)
            { // first what each face looks out on, from where this view would stand beyond it
                var e = seen[i]; var vc = vcams[depth];
                var m = e.f.target.l2w * Half * e.f.w2l;
                vc.transform.SetPositionAndRotation(m.MultiplyPoint3x4(cp), m.rotation * cr);
                e.rt = Target(depth, i);
                View(vc, depth + 1, e.rt, e.f.target, e.f.target.Z.th, e.rect);
            }
            foreach (var f in L.faces)
            {
                float d = Vector3.Dot(cp - f.uPos, f.uNormal);
                f.mr.enabled = d > 0.0005f && !(skip != null && (f == skip || f == skip.twin));
                f.mat.SetTexture(MainTex, Texture2D.blackTexture); f.mat.SetFloat(FlatMix, 1f); f.mat.SetColor(FlatColor, f.target.Z.th.fog.linear);
            }
            foreach (var e in seen) { e.f.mat.SetTexture(MainTex, e.rt); e.f.mat.SetFloat(FlatMix, 0f); }
            ApplyTheme(theme, c);
            if (into == null) return; // the eyes' own picture is drawn by Unity after this
            c.fieldOfView = cfg.fov; c.aspect = (float)poolW / Mathf.Max(1, poolH); c.nearClipPlane = 0.03f; c.farClipPlane = 180f;
            c.ResetProjectionMatrix();
            float back = Vector3.Dot(skip.uPos - cp, skip.uNormal);
            if (back > 0.12f)
            { // cut away everything between this view and the face it looks out of
                var w2c = c.worldToCameraMatrix;
                Vector3 pc = w2c.MultiplyPoint(skip.uPos), nc = w2c.MultiplyVector(skip.uNormal).normalized;
                c.projectionMatrix = c.CalculateObliqueMatrix(new Vector4(nc.x, nc.y, nc.z, -Vector3.Dot(pc, nc)));
            }
            var req = new UniversalRenderPipeline.SingleCameraRequest { destination = into };
            if (RenderPipeline.SupportsRenderRequest(c, req)) RenderPipeline.SubmitRenderRequest(c, req);
        }

        // ---- sound: made in code --------------------------------------------------------------------
        AudioSource voice;
        readonly Dictionary<string, AudioClip> clips = new Dictionary<string, AudioClip>();

        void SetUpAudio()
        {
            voice = gameObject.AddComponent<AudioSource>(); voice.playOnAwake = false; voice.spatialBlend = 0f;
            AudioListener.volume = cfg.volume;
            clips["stepA"] = Clip((150, 0.06f, 'T', 0.05f, 0, 0)); clips["stepB"] = Clip((170, 0.06f, 'T', 0.05f, 0, 0));
            clips["toRed"] = Clip((520, 0.45f, 'S', 0.16f, 180, 0), (780, 0.4f, 'T', 0.07f, 260, 0));
            clips["toColour"] = Clip((260, 0.45f, 'S', 0.16f, 660, 0), (390, 0.4f, 'T', 0.07f, 990, 0));
            clips["pick"] = Clip((440, 0.12f, 'T', 0.16f, 660, 0)); clips["put"] = Clip((330, 0.12f, 'T', 0.16f, 220, 0));
            clips["no"] = Clip((140, 0.14f, 'Q', 0.06f, 0, 0));
            clips["splash"] = Clip((700, 0.25f, 'S', 0.12f, 200, 0), (300, 0.3f, 'T', 0.08f, 120, 0.05f));
            clips["sw"] = Clip((600, 0.08f, 'Q', 0.07f, 900, 0)); clips["door"] = Clip((110, 0.5f, 'W', 0.06f, 70, 0));
            clips["land"] = Clip((90, 0.12f, 'T', 0.18f, 50, 0));
            clips["solved"] = Clip((660, 0.5f, 'S', 0.14f, 0, 0), (880, 0.5f, 'S', 0.14f, 0, 0.09f), (1320, 0.5f, 'S', 0.14f, 0, 0.18f));
            clips["win"] = Clip((523, 0.35f, 'T', 0.14f, 0, 0), (659, 0.35f, 'T', 0.14f, 0, 0.1f), (784, 0.35f, 'T', 0.14f, 0, 0.2f), (1047, 0.35f, 'T', 0.14f, 0, 0.3f));
        }

        // Each tone: frequency, seconds, wave (S sine, T triangle, Q square, W saw), loudness, glide-to frequency (0 for none), delay.
        static AudioClip Clip(params (float f, float dur, char wave, float vol, float to, float delay)[] tones)
        {
            const int rate = 44100; float len = 0;
            foreach (var tn in tones) len = Mathf.Max(len, tn.delay + tn.dur + 0.03f);
            var data = new float[Mathf.CeilToInt(len * rate)];
            foreach (var tn in tones)
            {
                double phase = 0; int start = (int)(tn.delay * rate), n = (int)(tn.dur * rate);
                for (int i = 0; i < n && start + i < data.Length; i++)
                {
                    float u = i / (float)n, tsec = i / (float)rate;
                    float fr = tn.to > 0 ? tn.f * Mathf.Pow(tn.to / tn.f, u) : tn.f;
                    phase += fr / rate; float ph = (float)(phase - System.Math.Floor(phase));
                    float w = tn.wave == 'S' ? Mathf.Sin(ph * PI * 2) : tn.wave == 'T' ? 4 * Mathf.Abs(ph - 0.5f) - 1 : tn.wave == 'Q' ? (ph < 0.5f ? 1 : -1) : 2 * ph - 1;
                    float env = tsec < 0.015f ? tsec / 0.015f : Mathf.Pow(0.001f / tn.vol, (tsec - 0.015f) / Mathf.Max(0.01f, tn.dur - 0.015f));
                    data[start + i] += w * tn.vol * env;
                }
            }
            var clip = AudioClip.Create("tone", data.Length, 1, rate, false);
            clip.SetData(data, 0);
            return clip;
        }

        void Sfx(string name)
        {
            if (scripted || muted || voice == null) return;
            if (clips.TryGetValue(name, out var c)) voice.PlayOneShot(c);
        }

        // ---- the HUD ----------------------------------------------------------------------------------
        Texture2D white, ring;
        GUIStyle label;

        void OnGUI()
        {
            if (L == null || P == null) return;
            if (white == null)
            {
                white = Texture2D.whiteTexture;
                ring = new Texture2D(32, 32, TextureFormat.RGBA32, false);
                for (int y = 0; y < 32; y++) for (int x = 0; x < 32; x++)
                {
                    float d = Mathf.Sqrt((x - 15.5f) * (x - 15.5f) + (y - 15.5f) * (y - 15.5f));
                    float a = Mathf.Clamp01(1.6f - Mathf.Abs(d - 9f)), b = Mathf.Clamp01(1.4f - Mathf.Abs(d - 12.5f)) * 0.45f;
                    ring.SetPixel(x, y, a > 0.01f ? new Color(1, 1, 1, a) : new Color(0, 0, 0, b));
                }
                ring.Apply();
                label = new GUIStyle(GUI.skin.label) { richText = false, wordWrap = false };
            }
            float s = Screen.height / 720f, W = Screen.width / s; const float H = 720f;
            GUI.matrix = Matrix4x4.Scale(new Vector3(s, s, 1));
            if (scene == "title")
            {
                Fill(new Rect(W / 2 - 440, 70, 880, 560), new Color(0.055f, 0.08f, 0.11f, 0.84f));
                Text("Seamwalk", W / 2, 168, 100, Color.white, TextAnchor.MiddleCenter, true);
                Fill(new Rect(W / 2 - 150, 236, 150, 6), Rgb(0x3fe0c5)); Fill(new Rect(W / 2, 236, 150, 6), Rgb(0xff4d5e));
                Text("one garden, two lives: every arch is a hole into the other", W / 2, 284, 25, Rgb(0xffeaa0));
                Text("W A S D: walk     Mouse: look around (click the game first)", W / 2, 356, 24, Color.white);
                Text("E: pick up a cube, or put it down", W / 2, 402, 24, Color.white);
                Text("R: restart the space     P: pause     M: mute     Esc: free the mouse", W / 2, 448, 24, Color.white);
                Text("Lost? R starts the space again. Nothing here can hurt you.", W / 2, 504, 22, Rgb(0xcfd6e0));
                Text("Click or press Enter to start", W / 2, 582, 30, Rgb(0xffe066), TextAnchor.MiddleCenter, true);
                return;
            }
            if (scene == "results")
            {
                Fill(new Rect(W / 2 - 400, 170, 800, 380), new Color(0.055f, 0.08f, 0.11f, 0.86f));
                Text(gained, W / 2, 262, 60, Color.white, TextAnchor.MiddleCenter, true);
                Fill(new Rect(W / 2 - 150, 312, 150, 6), Rgb(0x3fe0c5)); Fill(new Rect(W / 2, 312, 150, 6), Rgb(0xff4d5e));
                Text("All 5 spaces, both lives, and not one straight line home.", W / 2, 372, 25, Rgb(0xffeaa0));
                Text("Enter to walk it again", W / 2, 468, 28, Rgb(0xffe066), TextAnchor.MiddleCenter, true);
                return;
            }
            bool red = P.Z.th.red; float y0 = H - 30;
            Fill(new Rect(0, H - 60, W, 60), new Color(0.047f, 0.055f, 0.078f, 0.94f));
            Fill(new Rect(0, H - 62, W, 3), red ? Rgb(0xff4d5e) : Rgb(0x3fe0c5));
            Text("Space " + level + " of " + Spaces.Count + ": " + levelName, 28, y0, 20, Color.white, TextAnchor.MiddleLeft, true);
            var pivot = new Vector2(W / 2 - 80, y0);
            var keep = GUI.matrix; GUIUtility.RotateAroundPivot(45, new Vector2(pivot.x * s, pivot.y * s));
            Fill(new Rect(pivot.x - 9, pivot.y - 9, 18, 18), Color.white); Fill(new Rect(pivot.x - 7, pivot.y - 7, 14, 14), red ? Rgb(0xd22a3e) : Rgb(0x58c86a));
            GUI.matrix = keep;
            Text(red ? "Red life" : "Colourful life", W / 2 - 60, y0, 20, Color.white, TextAnchor.MiddleLeft, true);
            Text("E cube   R restart   P pause", W - 28, y0, 19, Rgb(0xcfd6e0), TextAnchor.MiddleRight);
            GUI.color = Color.white; GUI.DrawTexture(new Rect(W / 2 - 8, (H - 60) / 2 - 8, 16, 16), ring);
            string tip = prompt != "" ? prompt : (!locked && scene == "play" ? "Click the game to look with the mouse" : "");
            if (tip != "") { Fill(new Rect(W / 2 - 210, 22, 420, 44), new Color(0.047f, 0.055f, 0.078f, 0.82f)); Text(tip, W / 2, 44, 21, Color.white); }
            if (scene == "pause")
            {
                Fill(new Rect(0, 0, W, H), new Color(0.03f, 0.04f, 0.06f, 0.55f));
                Text("Paused", W / 2, H / 2 - 30, 64, Color.white, TextAnchor.MiddleCenter, true);
                Text("Click, or press P, to go on", W / 2, H / 2 + 36, 26, Rgb(0xffe066));
            }
            if (fade > 0) Fill(new Rect(0, 0, W, H), new Color(1, 1, 1, fade));
        }

        static Color Rgb(int h) => World.Hex(h);

        void Fill(Rect r, Color c) { GUI.color = c; GUI.DrawTexture(r, white); GUI.color = Color.white; }

        void Text(string str, float x, float y, int size, Color c, TextAnchor anchor = TextAnchor.MiddleCenter, bool bold = false)
        {
            label.fontSize = size; label.alignment = anchor; label.fontStyle = bold ? FontStyle.Bold : FontStyle.Normal; label.normal.textColor = c;
            var r = anchor == TextAnchor.MiddleLeft ? new Rect(x, y - size, 1400, size * 2) : anchor == TextAnchor.MiddleRight ? new Rect(x - 1400, y - size, 1400, size * 2) : new Rect(x - 700, y - size, 1400, size * 2);
            GUI.Label(r, str, label);
        }
    }
}
