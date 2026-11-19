using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

namespace Seamwalk
{
    // The game's rules run in the browser game's coordinates: x east, y up, z SOUTH, yaw turning
    // left from north. Unity's z points the other way, so everything drawn goes through World.U().

    /// <summary>The palette and light of one kind of place.</summary>
    public class Theme
    {
        public string name;
        public bool red, open;
        public Color horizon, zenith, sun, fog;
        public float sunScale = 1f;
        public int ground, hill, tileA, tileB, wall, top, hedge, hedgeTop, stone, wood, shed, roof, water, trunk, ceil;
        public int[] leafs, flowers;
        public Material floor;
    }

    /// <summary>One piece of space. A level is several, set far apart and joined only by portal faces.</summary>
    public class Zone
    {
        public int idx;
        public float ox;
        public Theme th;
        public Transform root;
        public MeshKit kit = new MeshKit();
    }

    /// <summary>A box the walker and cubes bump into, with the heights it covers.</summary>
    public class Solid
    {
        public float x0, z0, x1, z1, y0, y1;
        public bool off, water;
    }

    /// <summary>A patch of floor: flat, or a slope along one axis.</summary>
    public class Region
    {
        public float x0, z0, x1, z1, ya, yb;
        public bool alongX;
    }

    /// <summary>
    /// A portal face: a flat opening seen and entered from its front. It shows what its linked face
    /// looks out on, and walking through it carries you out of the linked face's front.
    /// </summary>
    public class Face
    {
        public Zone Z;
        public float x, y, z, yaw, w, h, nx, nz, rx, rz, rot;
        public Face target, twin;
        public GameObject go;
        public MeshRenderer mr;
        public Material mat;
        public Vector3 uPos, uNormal, uCentre;
        public float radius;
        public Bounds bounds;
        public Vector3[] corners;
        public Matrix4x4 l2w, w2l;

        /// <summary>Where a point (in rule coordinates) lands after going through this face.</summary>
        public Vector3 Map(Vector3 p)
        {
            float dx = p.x - x, dz = p.z - z, c = Mathf.Cos(yaw), s = Mathf.Sin(yaw);
            float lx = -(dx * c - dz * s), lz = -(dx * s + dz * c);
            float tc = Mathf.Cos(target.yaw), ts = Mathf.Sin(target.yaw);
            return new Vector3(target.x + lx * tc + lz * ts, p.y - y + target.y, target.z - lx * ts + lz * tc);
        }
    }

    public class Cube
    {
        public Zone Z, homeZ;
        public float x, y, z, hx, hy, hz, drop;
        public bool held;
        public Transform tr;
    }

    public class Switch
    {
        public Zone Z;
        public float x, y, z;
        public char key;
        public bool down;
        public Transform plate;
    }

    public class Door
    {
        public Zone Z;
        public char key;
        public Solid col;
        public Transform grp;
        public float open;
        public bool moving;
    }

    public class Gate
    {
        public Zone Z;
        public float x, y, z;
        public bool lit;
        public Transform star;
        public Material starMat;
        public GameObject sheet, bars;
        public Light glow;
    }

    /// <summary>Everything in the space being played.</summary>
    public class Level
    {
        public int index;
        public string name;
        public char gateKey;
        public Transform root;
        public readonly List<Zone> zones = new List<Zone>();
        public readonly List<Solid> solids = new List<Solid>();
        public readonly List<Region> regions = new List<Region>();
        public readonly List<Face> faces = new List<Face>();
        public readonly List<Cube> cubes = new List<Cube>();
        public readonly List<Switch> switches = new List<Switch>();
        public readonly List<Door> doors = new List<Door>();
        public readonly List<Solid> waters = new List<Solid>();
        public readonly List<Zone> waterZones = new List<Zone>();
        public Gate gate;
        public Zone startZ;
        public float sx, sy, sz, syaw;
    }

    /// <summary>Shared materials, palettes and the helpers every space is built with.</summary>
    public static class World
    {
        public const int CUBE = 0xff9a3c, CUBE_LIGHT = 0xffc27a, FRAME = 0xf9eaa2, RIM = 0xfffbd8, DOORWOOD = 0xb9824c;
        public static readonly Dictionary<string, Theme> Themes = new Dictionary<string, Theme>();
        static readonly Dictionary<string, Material> mats = new Dictionary<string, Material>();
        static Material litBase;
        public static Shader portalShader, skyShader;

        /// <summary>Rule coordinates to Unity's: z flips.</summary>
        public static Vector3 U(float x, float y, float z) => new Vector3(x, y, -z);

        public static Color Hex(int h) => new Color(((h >> 16) & 255) / 255f, ((h >> 8) & 255) / 255f, (h & 255) / 255f, 1f);

        public static Color KeyColor(char key) => Hex(key == 'y' ? 0xffd23f : 0xff6ec0);

        /// <summary>A shared lit material of one colour; <paramref name="glow"/> above 0 makes it shine.</summary>
        public static Material Mat(int hex, float glow = 0f, int glowHex = -1, float smooth = 0.12f)
        {
            string key = hex + "|" + glow + "|" + glowHex + "|" + smooth;
            if (mats.TryGetValue(key, out var m)) return m;
            m = NewLit("c" + hex.ToString("x6"));
            m.SetColor("_BaseColor", Hex(hex));
            m.SetFloat("_Smoothness", smooth);
            if (glow > 0f) Glow(m, Hex(glowHex < 0 ? hex : glowHex) * glow);
            mats[key] = m;
            return m;
        }

        public static Material NewLit(string name)
        {
            if (litBase == null)
            {
                litBase = Resources.Load<Material>("SeamLit");
                if (litBase == null) litBase = new Material(Shader.Find("Universal Render Pipeline/Lit"));
            }
            var m = new Material(litBase) { name = name };
            m.SetFloat("_Metallic", 0f);
            m.SetFloat("_Smoothness", 0.12f);
            return m;
        }

        public static void Glow(Material m, Color c)
        {
            m.EnableKeyword("_EMISSION");
            m.globalIlluminationFlags = MaterialGlobalIlluminationFlags.RealtimeEmissive;
            m.SetColor("_EmissionColor", c);
        }

        static Texture2D Checker(int a, int b)
        {
            const int n = 64;
            var t = new Texture2D(n, n, TextureFormat.RGBA32, true) { wrapMode = TextureWrapMode.Repeat, filterMode = FilterMode.Trilinear, anisoLevel = 8 };
            Color ca = Hex(a), cb = Hex(b);
            var px = new Color[n * n];
            for (int y = 0; y < n; y++) for (int x = 0; x < n; x++) px[y * n + x] = ((x < n / 2) ^ (y < n / 2)) ? ca : cb;
            t.SetPixels(px); t.Apply(true);
            return t;
        }

        /// <summary>Builds the palettes once. Safe to call again after a domain reload.</summary>
        public static void Init()
        {
            mats.Clear(); Themes.Clear(); litBase = null;
            portalShader = Resources.Load<Shader>("SeamPortal");
            skyShader = Resources.Load<Shader>("SeamSky");
            Themes["green"] = new Theme { name = "green", open = true, horizon = Hex(0xcfeeff), zenith = Hex(0x5fb4f2), fog = Hex(0xcfeeff), sun = Hex(0xfff2d6),
                ground = 0x86d06e, hill = 0x74c463, tileA = 0xb4ec9c, tileB = 0xa2e088, wall = 0xf6e9c8, top = 0xf8b6c9, hedge = 0x4aa85a, hedgeTop = 0x63bf6b,
                stone = 0xd9d2c4, wood = 0xc8915a, shed = 0xf4dcae, roof = 0xf07f95, water = 0x6fd2f7, trunk = 0x9b6b45,
                leafs = new[] { 0x5cc46a, 0xf8a9c4, 0x7fd67f, 0xfad0dc }, flowers = new[] { 0xffd23f, 0xffffff, 0xff8fb8 } };
            Themes["red"] = new Theme { name = "red", red = true, open = true, horizon = Hex(0xa62a40), zenith = Hex(0x4a0e22), fog = Hex(0x8e2034), sun = Hex(0xffb0a0), sunScale = 0.85f,
                ground = 0x8c1a2a, hill = 0x6e1222, tileA = 0xb5263a, tileB = 0x9e1e30, wall = 0x4c1c26, top = 0x8a2638, hedge = 0x6a1a2e, hedgeTop = 0x8a2238,
                stone = 0x4c242a, wood = 0x4a1c20, shed = 0x45181e, roof = 0x260a10, water = 0xff7a3a, trunk = 0x2a1014,
                leafs = new[] { 0xff5a7a, 0xff9ab0, 0xd42a50, 0xffc0cc }, flowers = new[] { 0xff9ab0, 0xffd0d8, 0xff5a7a } };
            Themes["hall"] = new Theme { name = "hall", horizon = Hex(0xf6e9c8), zenith = Hex(0xf6e9c8), fog = Hex(0xf6e9c8), sun = Hex(0xfff2d6),
                tileA = 0xf2d9a6, tileB = 0xe6c98e, wall = 0xf6e9c8, top = 0xf8b6c9, ceil = 0xfbf5e6, stone = 0xd9d2c4, wood = 0xc8915a,
                leafs = new[] { 0x5cc46a }, flowers = new[] { 0xffd23f } };
            Themes["cave"] = new Theme { name = "cave", red = true, horizon = Hex(0x2a0c14), zenith = Hex(0x2a0c14), fog = Hex(0x2a0c14), sun = Hex(0xffb0a0), sunScale = 0.85f,
                tileA = 0xa02438, tileB = 0x861c30, wall = 0x6e2436, top = 0xa23048, ceil = 0x3c1422, stone = 0x4c242a, wood = 0x4a1c20,
                leafs = new[] { 0xff5a7a, 0xff9ab0, 0xd42a50 }, flowers = new[] { 0xff9ab0 } };
            foreach (var th in Themes.Values)
            {
                th.floor = NewLit("floor-" + th.name);
                th.floor.SetTexture("_BaseMap", Checker(th.tileA, th.tileB));
                th.floor.SetColor("_BaseColor", Color.white);
                th.floor.SetFloat("_Smoothness", 0.2f);
            }
        }

        /// <summary>The same seed plants the same trees in every copy of a place.</summary>
        public static Func<float> Scatter(int s)
        {
            return () =>
            {
                unchecked
                {
                    s += 0x6D2B79F5;
                    int t = (s ^ (int)((uint)s >> 15)) * (s | 1);
                    t ^= t + (t ^ (int)((uint)t >> 7)) * (t | 61);
                    return ((uint)(t ^ (int)((uint)t >> 14))) / 4294967296f;
                }
            };
        }
    }

    /// <summary>The vocabulary a space is written in: walls, hedges, floors, arches, cubes, doors.</summary>
    public class Builder
    {
        public readonly Level L;
        const float PI = Mathf.PI;

        public Builder(Level level) { L = level; }

        public Zone NewZone(string theme)
        {
            var Z = new Zone { idx = L.zones.Count, th = World.Themes[theme] };
            Z.ox = Z.idx * 500f;
            var go = new GameObject("zone" + Z.idx + "-" + theme);
            go.transform.SetParent(L.root, false);
            Z.root = go.transform;
            L.zones.Add(Z);
            return Z;
        }

        /// <summary>Call once every zone is described: turns the collected shapes into meshes.</summary>
        public void Finish()
        {
            foreach (var Z in L.zones) Z.kit.Build("static", Z.root);
        }

        static float[] RotL(float yaw, float lx, float lz) => new[] { lx * Mathf.Cos(yaw) + lz * Mathf.Sin(yaw), -lx * Mathf.Sin(yaw) + lz * Mathf.Cos(yaw) };

        // ---- raw pieces -------------------------------------------------------------------------
        public void Box(Zone Z, float x0, float y0, float z0, float x1, float y1, float z1, Material m)
        {
            Z.kit.AddBox(m, new Vector3(x0 + Z.ox, y0, -z1), new Vector3(x1 + Z.ox, y1, -z0));
        }

        public void Box(Zone Z, float x0, float y0, float z0, float x1, float y1, float z1, int color) => Box(Z, x0, y0, z0, x1, y1, z1, World.Mat(color));

        public void Blob(Zone Z, MeshKit.Shape s, float x, float y, float z, float sx, float sy, float sz, Material m, float ry = 0f, float tilt = 0f)
        {
            Z.kit.Add(m, s, Matrix4x4.TRS(World.U(x + Z.ox, y, z), Quaternion.Euler(0f, -ry * Mathf.Rad2Deg, tilt * Mathf.Rad2Deg), new Vector3(sx, sy, sz)));
        }

        public Solid Block(Zone Z, float x0, float z0, float x1, float z1, float y0, float y1)
        {
            var c = new Solid { x0 = x0 + Z.ox, z0 = z0, x1 = x1 + Z.ox, z1 = z1, y0 = y0, y1 = y1 };
            L.solids.Add(c);
            return c;
        }

        public void Area(Zone Z, float x0, float z0, float x1, float z1, float ya, float yb = float.NaN, bool alongX = true)
        {
            L.regions.Add(new Region { x0 = x0 + Z.ox, z0 = z0, x1 = x1 + Z.ox, z1 = z1, ya = ya, yb = float.IsNaN(yb) ? ya : yb, alongX = alongX });
        }

        public void Tiles(Zone Z, float x0, float z0, float x1, float z1, float y = 0f, Theme from = null)
        {
            Z.kit.AddFloor((from ?? Z.th).floor, x0 + Z.ox, -z1, x1 + Z.ox, -z0, y + 0.012f, Z.ox);
        }

        public Light Lamp(Zone Z, float x, float y, float z, Color c, float intensity, float range)
        {
            var go = new GameObject("lamp");
            go.transform.SetParent(Z.root, false);
            go.transform.position = World.U(x + Z.ox, y, z);
            var l = go.AddComponent<Light>();
            l.type = LightType.Point; l.color = c; l.intensity = intensity; l.range = range; l.shadows = LightShadows.None;
            return l;
        }

        // ---- the garden's parts -------------------------------------------------------------------
        public void Ground(Zone Z)
        {
            Z.kit.AddBox(World.Mat(Z.th.ground), new Vector3(Z.ox - 196, -0.5f, -196), new Vector3(Z.ox + 196, -0.02f, 196));
        }

        public void Wall(Zone Z, float x0, float z0, float x1, float z1, float h = 3.4f, float y = 0f)
        {
            Box(Z, x0, y, z0, x1, y + h - 0.28f, z1, Z.th.wall);
            Box(Z, x0 - 0.07f, y + h - 0.28f, z0 - 0.07f, x1 + 0.07f, y + h, z1 + 0.07f, Z.th.top);
            Block(Z, x0, z0, x1, z1, y, y + h);
        }

        public void Hedge(Zone Z, float x0, float z0, float x1, float z1, float h = 2.8f)
        {
            Box(Z, x0, 0, z0, x1, h - 0.25f, z1, Z.th.hedge);
            Box(Z, x0 + 0.04f, h - 0.25f, z0 + 0.04f, x1 - 0.04f, h, z1 - 0.04f, Z.th.hedgeTop);
            Block(Z, x0, z0, x1, z1, 0, h);
            float lx = x1 - x0, lz = z1 - z0, len = Mathf.Max(lx, lz);
            int n = Mathf.FloorToInt(len / 1.5f);
            for (int i = 0; i < n; i++)
            {
                float t = (i + 0.5f) / n, bx = lx > lz ? x0 + lx * t : (x0 + x1) / 2f, bz = lx > lz ? (z0 + z1) / 2f : z0 + lz * t;
                Blob(Z, MeshKit.Ico, bx, h + 0.05f, bz, 0.42f, 0.32f, 0.42f, World.Mat(Z.th.hedgeTop), i * 1.3f);
            }
        }

        /// <summary>A floor in the air: walk on it or under it.</summary>
        public void Deck(Zone Z, float x0, float z0, float x1, float z1, float y)
        {
            Box(Z, x0, y - 0.3f, z0, x1, y, z1, Z.th.wood);
            Tiles(Z, x0, z0, x1, z1, y, World.Themes[Z.th.red ? "cave" : "hall"]);
            Area(Z, x0, z0, x1, z1, y);
        }

        public void Rail(Zone Z, float x0, float z0, float x1, float z1, float y)
        {
            Box(Z, x0, y, z0, x1, y + 0.55f, z1, Z.th.top);
            Block(Z, x0, z0, x1, z1, y, y + 1.2f);
        }

        public void Post(Zone Z, float x, float z, float y1)
        {
            Box(Z, x - 0.17f, 0, z - 0.17f, x + 0.17f, y1, z + 0.17f, Z.th.wood);
            Block(Z, x - 0.17f, z - 0.17f, x + 0.17f, z + 0.17f, 0, y1);
        }

        /// <summary>Stairs to look at, a smooth slope to walk on, rising along z from z0 to z1.</summary>
        public void RampZ(Zone Z, float x0, float z0, float x1, float z1, float ya, float yb)
        {
            Area(Z, x0, z0, x1, z1, ya, yb, false);
            const int n = 10;
            for (int i = 0; i < n; i++)
            {
                float a = i / (float)n, b = (i + 1) / (float)n, low = Mathf.Lerp(ya, yb, a), high = Mathf.Lerp(ya, yb, b);
                float sz0 = Mathf.Lerp(z0, z1, a), sz1 = Mathf.Lerp(z0, z1, b), top = Mathf.Max(low, high), bottom = Mathf.Min(low, high);
                Box(Z, x0, 0, sz0, x1, top, sz1, i % 2 == 1 ? Z.th.stone : Z.th.top);
                if (bottom > 0.5f) Block(Z, x0, sz0, x1, sz1, 0, bottom - 0.25f); // lower than the walker climbing it, so only the ground bumps it
            }
        }

        public void Water(Zone Z, float x0, float z0, float x1, float z1)
        {
            Box(Z, x0, 0.0f, z0, x1, 0.05f, z1, World.Mat(Z.th.water, 0.35f, -1, 0.92f));
            Box(Z, x0 - 0.12f, 0, z0 - 0.12f, x1 + 0.12f, 0.03f, z1 + 0.12f, Z.th.stone);
            var c = Block(Z, x0, z0, x1, z1, -1f, 0.6f);
            c.water = true;
            L.waters.Add(c); L.waterZones.Add(Z);
        }

        // A tree, bush and flower in the colourful life are a crystal, mushroom and shard in the red.
        public void Tree(Zone Z, float x, float z, float s = 1f, int i = 0)
        {
            var th = Z.th; int c = th.leafs[i % th.leafs.Length];
            if (th.red)
            {
                if (i % 2 == 1)
                {
                    Blob(Z, MeshKit.Cyl, x, 0.9f * s, z, 0.22f * s, 1.8f * s, 0.22f * s, World.Mat(0xf2d2c8));
                    Blob(Z, MeshKit.Ball, x, 1.8f * s, z, 1.1f * s, 0.55f * s, 1.1f * s, World.Mat(c, 0.35f));
                }
                else
                {
                    var m = World.Mat(c, 0.6f);
                    Blob(Z, MeshKit.Cone, x, 1.5f * s, z, 0.5f * s, 3f * s, 0.5f * s, m, i);
                    Blob(Z, MeshKit.Cone, x + 0.5f * s, 0.9f * s, z + 0.2f * s, 0.35f * s, 1.8f * s, 0.35f * s, m, i + 1, -0.35f);
                    Blob(Z, MeshKit.Cone, x - 0.45f * s, 0.7f * s, z - 0.2f * s, 0.3f * s, 1.4f * s, 0.3f * s, m, i + 2, 0.4f);
                }
            }
            else
            {
                Blob(Z, MeshKit.Cyl, x, 0.8f * s, z, 0.16f * s, 1.6f * s, 0.16f * s, World.Mat(th.trunk));
                Blob(Z, MeshKit.Ico, x, 2.3f * s, z, 1.1f * s, 1.0f * s, 1.1f * s, World.Mat(c), i);
                Blob(Z, MeshKit.Ico, x + 0.5f * s, 3.0f * s, z - 0.2f * s, 0.7f * s, 0.65f * s, 0.7f * s, World.Mat(c), i + 2);
            }
        }

        public void Bush(Zone Z, float x, float z, float s = 1f, int i = 0, float y = 0f)
        {
            var th = Z.th;
            if (th.red)
            {
                Blob(Z, MeshKit.Cyl, x, y + 0.2f * s, z, 0.1f * s, 0.4f * s, 0.1f * s, World.Mat(0xf2d2c8));
                Blob(Z, MeshKit.Ball, x, y + 0.42f * s, z, 0.45f * s, 0.25f * s, 0.45f * s, World.Mat(th.leafs[i % th.leafs.Length], 0.35f));
            }
            else Blob(Z, MeshKit.Ico, x, y + 0.35f * s, z, 0.7f * s, 0.5f * s, 0.7f * s, World.Mat(th.leafs[i % 3 == 1 ? 2 % th.leafs.Length : 0]), i);
        }

        public void Flower(Zone Z, float x, float z, int i = 0, float y = 0f)
        {
            var th = Z.th; int c = th.flowers[Mathf.Abs(i) % th.flowers.Length];
            if (th.red) Blob(Z, MeshKit.Octa, x, y + 0.22f, z, 0.12f, 0.26f, 0.12f, World.Mat(c, 0.45f), i);
            else
            {
                Blob(Z, MeshKit.Cyl, x, y + 0.14f, z, 0.025f, 0.28f, 0.025f, World.Mat(0x4aa85a));
                Blob(Z, MeshKit.Ico, x, y + 0.32f, z, 0.13f, 0.11f, 0.13f, World.Mat(c), i);
            }
        }

        public void Crystal(Zone Z, float x, float z, float s = 1f, int i = 0, float y = 0f, bool lamp = false)
        {
            int c = Z.th.leafs[i % Z.th.leafs.Length];
            var m = World.Mat(c, 0.85f);
            Blob(Z, MeshKit.Cone, x, y + 0.6f * s, z, 0.25f * s, 1.2f * s, 0.25f * s, m, i);
            Blob(Z, MeshKit.Cone, x + 0.22f * s, y + 0.35f * s, z + 0.1f * s, 0.16f * s, 0.7f * s, 0.16f * s, m, i + 1, -0.4f);
            if (lamp) Lamp(Z, x, y + 1.0f * s, z, World.Hex(c), 9f, 9f);
        }

        public void Bench(Zone Z, float x, float z)
        {
            Box(Z, x - 0.8f, 0.4f, z - 0.25f, x + 0.8f, 0.5f, z + 0.25f, Z.th.wood);
            Box(Z, x - 0.8f, 0.5f, z + 0.18f, x + 0.8f, 0.95f, z + 0.25f, Z.th.wood);
            Box(Z, x - 0.7f, 0, z - 0.2f, x - 0.58f, 0.4f, z + 0.2f, Z.th.trunk);
            Box(Z, x + 0.58f, 0, z - 0.2f, x + 0.7f, 0.4f, z + 0.2f, Z.th.trunk);
            Block(Z, x - 0.8f, z - 0.25f, x + 0.8f, z + 0.25f, 0, 0.95f);
        }

        /// <summary>What lies beyond the walls, the sky included: the same in every copy of a place.</summary>
        public void Scenery(Zone Z, float bx0, float bz0, float bx1, float bz1, int seed)
        {
            var th = Z.th; var r = World.Scatter(seed);
            for (int i = 0; i < 44; i++)
            {
                float x = (r() * 2 - 1) * 62, z = (r() * 2 - 1) * 62, s = 1 + r() * 1.5f;
                if (x > bx0 && x < bx1 && z > bz0 && z < bz1) continue;
                Tree(Z, x, z, s, i);
                if (i % 3 == 0) Bush(Z, x + 1.6f * s, z + 0.8f, 1.2f, i);
            }
            for (int i = 0; i < 8; i++)
            {
                float a = i / 8f * PI * 2 + r(), d = 78 + r() * 14, rr = 16 + r() * 10;
                Blob(Z, MeshKit.Ball, Mathf.Cos(a) * d, -2, Mathf.Sin(a) * d, rr, rr * 0.4f, rr, World.Mat(th.hill));
            }
            for (int i = 0; i < 9; i++)
            {
                float x = (r() * 2 - 1) * 70, z = (r() * 2 - 1) * 70, y = 24 + r() * 12, s = 3 + r() * 4;
                if (th.red) Blob(Z, MeshKit.Octa, x, y, z, s * 0.6f, s * 0.9f, s * 0.6f, World.Mat(0x7a1830, 0.5f), i);
                else
                {
                    var m = World.Mat(0xffffff, 0.55f);
                    Blob(Z, MeshKit.Ball, x, y, z, s * 1.6f, s * 0.55f, s, m);
                    Blob(Z, MeshKit.Ball, x + s, y + 0.5f, z + 0.6f, s, s * 0.5f, s * 0.8f, m);
                }
            }
            Sky(Z);
        }

        void Sky(Zone Z)
        {
            var kit = new MeshKit();
            var m = new Material(World.skyShader) { name = "sky-" + Z.th.name };
            m.SetColor("_Horizon", Z.th.horizon.linear); m.SetColor("_Zenith", Z.th.zenith.linear);
            m.SetColor("_Glow", (Z.th.red ? World.Hex(0xff6a5a) : World.Hex(0xfff0c0)).linear);
            m.SetVector("_SunDir", SeamGame.SunDirection);
            kit.Add(m, MeshKit.Ball, Matrix4x4.Scale(Vector3.one * 120f));
            var go = kit.Build("sky", Z.root, false);
            go.transform.position = new Vector3(Z.ox, -5f, 0f);
        }

        /// <summary>A tiny shed, its one door on the south side.</summary>
        public void Shed(Zone Z, float x, float z, bool open)
        {
            var th = Z.th; int w = th.shed; const float d = 0.2f;
            Box(Z, x - 1.4f, 0, z - 1.2f, x - 1.4f + d, 2.4f, z + 1.2f, w); Box(Z, x + 1.4f - d, 0, z - 1.2f, x + 1.4f, 2.4f, z + 1.2f, w);
            Box(Z, x - 1.4f, 0, z - 1.2f, x + 1.4f, 2.4f, z - 1.2f + d, w);
            Box(Z, x - 1.4f, 0, z + 1.2f - d, x - 0.65f, 2.4f, z + 1.2f, w); Box(Z, x + 0.65f, 0, z + 1.2f - d, x + 1.4f, 2.4f, z + 1.2f, w);
            Box(Z, x - 0.65f, 2.2f, z + 1.2f - d, x + 0.65f, 2.4f, z + 1.2f, w);
            Box(Z, x - 0.77f, 0, z + 1.2f, x - 0.65f, 2.32f, z + 1.26f, th.trunk); Box(Z, x + 0.65f, 0, z + 1.2f, x + 0.77f, 2.32f, z + 1.26f, th.trunk);
            Box(Z, x - 0.77f, 2.2f, z + 1.2f, x + 0.77f, 2.32f, z + 1.26f, th.trunk);
            Blob(Z, MeshKit.Pyr, x, 3.0f, z, 2.25f, 1.25f, 1.95f, World.Mat(th.roof));
            Box(Z, x - 1.5f, 2.36f, z - 1.3f, x + 1.5f, 2.44f, z + 1.3f, th.roof);
            Block(Z, x - 1.4f, z - 1.2f, x - 1.2f, z + 1.2f, 0, 3); Block(Z, x + 1.2f, z - 1.2f, x + 1.4f, z + 1.2f, 0, 3);
            Block(Z, x - 1.4f, z - 1.2f, x + 1.4f, z - 1.0f, 0, 3);
            Block(Z, x - 1.4f, z + 1.0f, x - 0.65f, z + 1.2f, 0, 3); Block(Z, x + 0.65f, z + 1.0f, x + 1.4f, z + 1.2f, 0, 3);
            if (!open)
            {
                Box(Z, x - 0.65f, 0, z + 1.08f, x + 0.65f, 2.2f, z + 1.16f, th.wood);
                Box(Z, x - 0.7f, 0.55f, z + 1.16f, x + 0.7f, 0.75f, z + 1.22f, th.trunk); Box(Z, x - 0.7f, 1.45f, z + 1.16f, x + 0.7f, 1.65f, z + 1.22f, th.trunk);
                Block(Z, x - 0.65f, z + 1.0f, x + 0.65f, z + 1.2f, 0, 3);
            }
        }

        static Vector2[] Outline(bool arch, float half, float h)
        {
            var p = new List<Vector2> { new Vector2(-half, 0) };
            if (arch)
            {
                float hs = h - half;
                for (int i = 0; i <= 14; i++) { float a = PI - i * PI / 14f; p.Add(new Vector2(half * Mathf.Cos(a), hs + half * Mathf.Sin(a))); }
            }
            else { p.Add(new Vector2(-half, h)); p.Add(new Vector2(half, h)); }
            p.Add(new Vector2(half, 0));
            return p.ToArray();
        }

        static Vector2[] OuterOutline(bool arch, float half, float t, float h)
        {
            if (arch)
            {
                // the same arc, wider, over the same centre
                var p = new List<Vector2> { new Vector2(-(half + t), 0) };
                float hs = h - half;
                for (int i = 0; i <= 14; i++) { float a = PI - i * PI / 14f; p.Add(new Vector2((half + t) * Mathf.Cos(a), hs + (half + t) * Mathf.Sin(a))); }
                p.Add(new Vector2(half + t, 0));
                return p.ToArray();
            }
            return new[] { new Vector2(-(half + t), 0), new Vector2(-(half + t), h + t), new Vector2(half + t, h + t), new Vector2(half + t, 0) };
        }

        /// <summary>The frame round an opening, with its glowing rim and two posts to bump into.</summary>
        public void Frame(Zone Z, bool arch, float x, float y, float z, float yaw, float w, float h)
        {
            float t = arch ? 0.4f : 0.26f, dep = arch ? 0.5f : 0.36f;
            var xf = Matrix4x4.TRS(World.U(x + Z.ox, y, z), Quaternion.Euler(0, 180f - yaw * Mathf.Rad2Deg, 0), Vector3.one);
            Z.kit.AddFrame(World.Mat(arch ? World.FRAME : World.DOORWOOD), Outline(arch, w / 2, h), OuterOutline(arch, w / 2, t, h), -dep / 2, dep / 2, xf);
            Z.kit.AddFrame(World.Mat(World.RIM, 1.25f, 0xffdf90), Outline(arch, w / 2 - 0.02f, h - 0.02f), OuterOutline(arch, w / 2 - 0.02f, 0.07f, h - 0.02f), -dep / 2 - 0.03f, dep / 2 + 0.03f, xf);
            for (int s = -1; s <= 1; s += 2)
            {
                var a = RotL(yaw, s * w / 2, -dep / 2); var b = RotL(yaw, s * (w / 2 + t), dep / 2);
                Block(Z, Mathf.Min(x + a[0], x + b[0]), Mathf.Min(z + a[1], z + b[1]), Mathf.Max(x + a[0], x + b[0]), Mathf.Max(z + a[1], z + b[1]), y, y + h + 0.6f);
            }
        }

        public Cube AddCube(Zone Z, float x, float z, float y = 0f)
        {
            var kit = new MeshKit();
            kit.Add(World.Mat(World.CUBE, 0.12f), MeshKit.Box, Matrix4x4.Scale(Vector3.one * 0.7f));
            for (int i = 0; i < 8; i++)
                kit.Add(World.Mat(World.CUBE_LIGHT, 0.25f), MeshKit.Box, Matrix4x4.TRS(new Vector3((i & 1) != 0 ? 0.27f : -0.27f, (i & 2) != 0 ? 0.27f : -0.27f, (i & 4) != 0 ? 0.27f : -0.27f), Quaternion.identity, Vector3.one * 0.2f));
            var go = kit.Build("cube", L.root);
            var c = new Cube { Z = Z, homeZ = Z, x = x + Z.ox, y = y, z = z, hx = x + Z.ox, hy = y, hz = z, tr = go.transform };
            L.cubes.Add(c);
            return c;
        }

        static MeshKit.Shape MarkShape(char key) => key == 'y' ? MeshKit.Cyl : MeshKit.Tri;

        public void Sw(Zone Z, float x, float z, char key, float y = 0f)
        {
            Box(Z, x - 0.65f, y, z - 0.65f, x + 0.65f, y + 0.06f, z + 0.65f, Z.th.stone);
            var kit = new MeshKit();
            var km = World.NewLit("key-" + key); km.SetColor("_BaseColor", World.KeyColor(key)); World.Glow(km, World.KeyColor(key) * 0.6f);
            kit.AddBox(km, new Vector3(-0.5f, 0, -0.5f), new Vector3(0.5f, 0.1f, 0.5f));
            kit.Add(World.Mat(0xffffff, 0.7f), MarkShape(key), Matrix4x4.TRS(new Vector3(0, 0.105f, 0), Quaternion.identity, new Vector3(0.22f, 0.02f, 0.22f)));
            var go = kit.Build("switch", Z.root, false);
            go.transform.position = World.U(x + Z.ox, y + 0.06f, z);
            L.switches.Add(new Switch { Z = Z, x = x + Z.ox, y = y, z = z, key = key, plate = go.transform });
        }

        /// <summary>Bars that sink when the switch of their colour is held down.</summary>
        public void Bars(Zone Z, float x0, float z0, float x1, float z1, char key, float y = 0f, float h = 2.8f)
        {
            var kit = new MeshKit();
            bool alongX = x1 - x0 > z1 - z0; float len = alongX ? x1 - x0 : z1 - z0; int n = Mathf.Max(3, Mathf.RoundToInt(len / 0.34f));
            var km = World.NewLit("bars-" + key); km.SetColor("_BaseColor", World.KeyColor(key)); World.Glow(km, World.KeyColor(key) * 0.45f);
            for (int i = 0; i < n; i++)
            {
                float t = ((i + 0.5f) / n - 0.5f) * len;
                var c = new Vector3(alongX ? t : 0, h / 2, alongX ? 0 : t);
                kit.AddBox(km, c - new Vector3(0.06f, h / 2, 0.06f), c + new Vector3(0.06f, h / 2, 0.06f));
            }
            var half = new Vector3(alongX ? len / 2 : 0.1f, 0.1f, alongX ? 0.1f : len / 2);
            kit.AddBox(km, new Vector3(0, h - 0.1f, 0) - half, new Vector3(0, h - 0.1f, 0) + half);
            for (int s = -1; s <= 1; s += 2)
                kit.Add(World.Mat(0xffffff, 0.7f), MarkShape(key), Matrix4x4.TRS(new Vector3(alongX ? 0 : s * 0.12f, h * 0.55f, alongX ? s * 0.12f : 0),
                    alongX ? Quaternion.Euler(90, 0, 0) : Quaternion.Euler(0, 0, 90), new Vector3(0.32f, 0.04f, 0.32f)));
            var go = kit.Build("door", Z.root);
            go.transform.position = World.U((x0 + x1) / 2 + Z.ox, y, (z0 + z1) / 2);
            var col = Block(Z, x0, z0, x1, z1, y, y + h + 2);
            L.doors.Add(new Door { Z = Z, key = key, col = col, grp = go.transform });
        }

        /// <summary>The way out: a star over a doorway that fills with light.</summary>
        public void AddGate(Zone Z, float x, float z, float yaw, float y = 0f)
        {
            var root = new GameObject("gate"); root.transform.SetParent(Z.root, false);
            root.transform.SetPositionAndRotation(World.U(x + Z.ox, y, z), Quaternion.Euler(0, -yaw * Mathf.Rad2Deg, 0));
            var kit = new MeshKit(); var white = World.Mat(0xffffff, 0.08f);
            kit.AddBox(white, new Vector3(-1.15f, 0, -0.15f), new Vector3(-0.85f, 2.8f, 0.15f));
            kit.AddBox(white, new Vector3(0.85f, 0, -0.15f), new Vector3(1.15f, 2.8f, 0.15f));
            kit.AddBox(white, new Vector3(-1.3f, 2.8f, -0.2f), new Vector3(1.3f, 3.1f, 0.2f));
            kit.Build("posts", root.transform);
            var g = new Gate { Z = Z, x = x + Z.ox, y = y, z = z };
            g.starMat = World.NewLit("star");
            kit.Add(g.starMat, MeshKit.Octa, Matrix4x4.Scale(new Vector3(0.42f, 0.6f, 0.42f)));
            g.star = kit.Build("star", root.transform).transform; g.star.localPosition = new Vector3(0, 3.75f, 0);
            kit.AddBox(World.Mat(0xfff3a0, 1.8f, 0xffe27a), new Vector3(-0.85f, 0, -0.03f), new Vector3(0.85f, 2.8f, 0.03f));
            g.sheet = kit.Build("sheet", root.transform, false); g.sheet.SetActive(false);
            foreach (float bx in new[] { -0.5f, 0f, 0.5f }) kit.AddBox(World.Mat(0x9aa0a8), new Vector3(bx - 0.04f, 0, -0.04f), new Vector3(bx + 0.04f, 2.8f, 0.04f));
            g.bars = kit.Build("bars", root.transform);
            var lg = new GameObject("glow"); lg.transform.SetParent(root.transform, false); lg.transform.localPosition = new Vector3(0, 1.6f, 0);
            g.glow = lg.AddComponent<Light>(); g.glow.type = LightType.Point; g.glow.color = World.Hex(0xffe27a); g.glow.range = 9f; g.glow.intensity = 0f; g.glow.shadows = LightShadows.None;
            for (int s = -1; s <= 1; s += 2)
            {
                var a = RotL(yaw, s * 0.85f, -0.15f); var b = RotL(yaw, s * 1.15f, 0.15f);
                Block(Z, Mathf.Min(x + a[0], x + b[0]), Mathf.Min(z + a[1], z + b[1]), Mathf.Max(x + a[0], x + b[0]), Mathf.Max(z + a[1], z + b[1]), y, y + 3);
            }
            L.gate = g;
        }

        // ---- portal faces -------------------------------------------------------------------------
        public Face Portal(Zone Z, float x, float y, float z, float yaw, float w, float h, bool arch = false)
        {
            var f = new Face { Z = Z, x = x + Z.ox, y = y, z = z, yaw = yaw, w = w, h = h, nx = Mathf.Sin(yaw), nz = Mathf.Cos(yaw), rx = Mathf.Cos(yaw), rz = -Mathf.Sin(yaw) };
            var kit = new MeshKit();
            f.mat = new Material(World.portalShader) { name = "face" };
            kit.AddPlate(f.mat, Outline(arch, w / 2, h), 0.2f); // the front sits on the plane; the rest backs it, so the view never tears as you step in
            f.go = kit.Build("face", L.root, false);
            f.go.transform.SetPositionAndRotation(World.U(f.x, y, z), Quaternion.Euler(0, 180f - yaw * Mathf.Rad2Deg, 0));
            f.mr = f.go.GetComponentInChildren<MeshRenderer>();
            f.mr.receiveShadows = false; f.mr.lightProbeUsage = LightProbeUsage.Off;
            f.uPos = World.U(f.x, y, z); f.uNormal = new Vector3(f.nx, 0, -f.nz);
            f.uCentre = f.uPos + Vector3.up * (h / 2); f.radius = 0.5f * Mathf.Sqrt(w * w + h * h);
            f.l2w = f.go.transform.localToWorldMatrix; f.w2l = f.go.transform.worldToLocalMatrix;
            var right = f.l2w.MultiplyVector(Vector3.right);
            f.corners = new[] { f.uPos - right * (w / 2), f.uPos + right * (w / 2), f.uPos + right * (w / 2) + Vector3.up * h, f.uPos - right * (w / 2) + Vector3.up * h };
            f.bounds = new Bounds(f.corners[0], Vector3.zero);
            foreach (var c in f.corners) f.bounds.Encapsulate(c);
            f.bounds.Expand(0.05f);
            var pa = RotL(yaw, -w / 2, -0.9f); var pb = RotL(yaw, w / 2, 0.15f); // floor under the pocket behind it
            Area(Z, Mathf.Min(x + pa[0], x + pb[0]), Mathf.Min(z + pa[1], z + pb[1]), Mathf.Max(x + pa[0], x + pb[0]), Mathf.Max(z + pa[1], z + pb[1]), y);
            foreach (var o in L.faces)
                if (o.Z == Z && Mathf.Abs(o.x - f.x) + Mathf.Abs(o.z - z) + Mathf.Abs(o.y - y) < 0.01f) { o.twin = f; f.twin = o; }
            L.faces.Add(f);
            return f;
        }

        /// <summary>Walk in at a's front, walk out of b's front.</summary>
        public void Link(Face a, Face b)
        {
            a.target = b; b.target = a;
            a.rot = b.yaw + PI - a.yaw; b.rot = a.yaw + PI - b.yaw;
        }

        /// <summary>The same opening in two places: step through and you are in the other.</summary>
        public void Seam(Zone A, Zone B, float x, float y, float z, float yaw, float w, float h, bool arch = false)
        {
            Face a1 = Portal(A, x, y, z, yaw, w, h, arch), a2 = Portal(A, x, y, z, yaw + PI, w, h, arch);
            Face b1 = Portal(B, x, y, z, yaw, w, h, arch), b2 = Portal(B, x, y, z, yaw + PI, w, h, arch);
            Link(a1, b2); Link(a2, b1);
        }

        /// <summary>An arch between the two lives, standing in both.</summary>
        public void Arch(Zone A, Zone B, float x, float z, float yaw)
        {
            Frame(A, true, x, 0, z, yaw, 2.2f, 2.8f); Frame(B, true, x, 0, z, yaw, 2.2f, 2.8f);
            Seam(A, B, x, 0, z, yaw, 2.2f, 2.8f, true);
            Lamp(A, x, 2.0f, z, World.Hex(0xffe9a0), 3.5f, 5f); Lamp(B, x, 2.0f, z, World.Hex(0xffe9a0), 3.5f, 5f);
        }

        public void Start(Zone Z, float x, float z, float yaw, float y = 0f)
        {
            L.startZ = Z; L.sx = x + Z.ox; L.sy = y; L.sz = z; L.syaw = yaw;
        }
    }
}
