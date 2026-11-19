using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

namespace Seamwalk
{
    /// <summary>
    /// Builds flat-shaded low-poly meshes in code, one mesh per material, so a whole place is a
    /// handful of draw calls. Every triangle gets its own normal: the faceted look is the art style.
    /// </summary>
    public class MeshKit
    {
        class Buf
        {
            public readonly List<Vector3> v = new List<Vector3>();
            public readonly List<Vector3> n = new List<Vector3>();
            public readonly List<Vector2> uv = new List<Vector2>();
            public readonly List<int> t = new List<int>();
        }

        readonly Dictionary<Material, Buf> bufs = new Dictionary<Material, Buf>();

        /// <summary>A convex unit shape centred on the origin: its corners and its triangles.</summary>
        public class Shape
        {
            public Vector3[] v;
            public int[] t;
        }

        public static readonly Shape Box = MakeBox();
        public static readonly Shape Ico = MakeIco(0);
        public static readonly Shape Ball = MakeIco(1);
        public static readonly Shape Octa = MakeOcta();
        public static readonly Shape Cone = MakeCone(5, 0f, true);
        public static readonly Shape Pyr = MakeCone(4, Mathf.PI / 4f, true);
        public static readonly Shape Cyl = MakeCone(8, 0f, false);
        public static readonly Shape Tri = MakeCone(3, 0f, false);

        Buf For(Material m)
        {
            if (!bufs.TryGetValue(m, out var b)) { b = new Buf(); bufs[m] = b; }
            return b;
        }

        /// <summary>Adds one triangle facing the side that <paramref name="hint"/> points to.</summary>
        public void Tri3(Material m, Vector3 a, Vector3 b, Vector3 c, Vector3 hint, Vector2 ua = default, Vector2 ub = default, Vector2 uc = default)
        {
            var n = Vector3.Cross(b - a, c - a);
            if (Vector3.Dot(n, hint) < 0f) { var s = b; b = c; c = s; var us = ub; ub = uc; uc = us; n = -n; }
            n.Normalize();
            var buf = For(m);
            int i = buf.v.Count;
            buf.v.Add(a); buf.v.Add(b); buf.v.Add(c);
            buf.n.Add(n); buf.n.Add(n); buf.n.Add(n);
            buf.uv.Add(ua); buf.uv.Add(ub); buf.uv.Add(uc);
            buf.t.Add(i); buf.t.Add(i + 1); buf.t.Add(i + 2);
        }

        /// <summary>Adds a four-cornered face (corners in order round its edge).</summary>
        public void Quad(Material m, Vector3 a, Vector3 b, Vector3 c, Vector3 d, Vector3 hint)
        {
            Tri3(m, a, b, c, hint);
            Tri3(m, a, c, d, hint);
        }

        /// <summary>Adds a unit shape moved, turned and scaled by <paramref name="xf"/>.</summary>
        public void Add(Material m, Shape s, Matrix4x4 xf)
        {
            var centre = xf.MultiplyPoint3x4(Vector3.zero);
            for (int i = 0; i < s.t.Length; i += 3)
            {
                Vector3 a = xf.MultiplyPoint3x4(s.v[s.t[i]]), b = xf.MultiplyPoint3x4(s.v[s.t[i + 1]]), c = xf.MultiplyPoint3x4(s.v[s.t[i + 2]]);
                var local = (s.v[s.t[i]] + s.v[s.t[i + 1]] + s.v[s.t[i + 2]]) / 3f;
                Tri3(m, a, b, c, xf.MultiplyVector(local));
            }
        }

        /// <summary>Adds a box between two corners.</summary>
        public void AddBox(Material m, Vector3 min, Vector3 max)
        {
            Add(m, Box, Matrix4x4.TRS((min + max) / 2f, Quaternion.identity, max - min));
        }

        /// <summary>Adds a flat floor facing up, with tile coordinates from its place in the world (2 m a tile pair).</summary>
        public void AddFloor(Material m, float x0, float z0, float x1, float z1, float y, float uvShiftX)
        {
            Vector3 a = new Vector3(x0, y, z0), b = new Vector3(x1, y, z0), c = new Vector3(x1, y, z1), d = new Vector3(x0, y, z1);
            System.Func<Vector3, Vector2> uv = p => new Vector2((p.x - uvShiftX) / 2f, p.z / 2f);
            Tri3(m, a, b, c, Vector3.up, uv(a), uv(b), uv(c));
            Tri3(m, a, c, d, Vector3.up, uv(a), uv(c), uv(d));
        }

        /// <summary>
        /// Adds a frame round an opening: the band between an inner and an outer outline (same number of
        /// points, both from the left foot over the top to the right foot), given a thickness along z.
        /// </summary>
        public void AddFrame(Material m, Vector2[] inner, Vector2[] outer, float z0, float z1, Matrix4x4 xf)
        {
            Vector3 P(Vector2 p, float z) => xf.MultiplyPoint3x4(new Vector3(p.x, p.y, z));
            Vector3 D(Vector3 d) => xf.MultiplyVector(d);
            float midY = 0f;
            foreach (var p in inner) midY = Mathf.Max(midY, p.y);
            var heart = new Vector3(0f, midY * 0.5f, (z0 + z1) / 2f);
            for (int i = 0; i + 1 < inner.Length; i++)
            {
                Vector2 a = inner[i], b = inner[i + 1], c = outer[i + 1], d = outer[i];
                Quad(m, P(a, z1), P(b, z1), P(c, z1), P(d, z1), D(Vector3.forward));
                Quad(m, P(a, z0), P(b, z0), P(c, z0), P(d, z0), D(Vector3.back));
                var midIn = new Vector3((a.x + b.x) / 2f, (a.y + b.y) / 2f, heart.z);
                Quad(m, P(a, z0), P(b, z0), P(b, z1), P(a, z1), D(heart - midIn));
                var midOut = new Vector3((c.x + d.x) / 2f, (c.y + d.y) / 2f, heart.z);
                Quad(m, P(d, z0), P(c, z0), P(c, z1), P(d, z1), D(midOut - heart));
            }
            int last = inner.Length - 1;
            Quad(m, P(inner[0], z0), P(outer[0], z0), P(outer[0], z1), P(inner[0], z1), D(Vector3.down));
            Quad(m, P(inner[last], z0), P(outer[last], z0), P(outer[last], z1), P(inner[last], z1), D(Vector3.down));
        }

        /// <summary>Adds a convex outline as a plate with a backing, front on z = 0 and the rest behind it.</summary>
        public void AddPlate(Material m, Vector2[] outline, float depth)
        {
            var c = Vector2.zero;
            foreach (var p in outline) c += p;
            c /= outline.Length;
            for (int i = 0; i < outline.Length; i++)
            {
                Vector2 a = outline[i], b = outline[(i + 1) % outline.Length];
                Tri3(m, new Vector3(c.x, c.y, 0), new Vector3(a.x, a.y, 0), new Vector3(b.x, b.y, 0), Vector3.forward);
                Tri3(m, new Vector3(c.x, c.y, -depth), new Vector3(a.x, a.y, -depth), new Vector3(b.x, b.y, -depth), Vector3.back);
                var mid = new Vector3((a.x + b.x) / 2f - c.x, (a.y + b.y) / 2f - c.y, 0);
                Quad(m, new Vector3(a.x, a.y, 0), new Vector3(b.x, b.y, 0), new Vector3(b.x, b.y, -depth), new Vector3(a.x, a.y, -depth), mid);
            }
        }

        /// <summary>Turns what was added into GameObjects under <paramref name="parent"/>, one per material.</summary>
        public GameObject Build(string name, Transform parent, bool castShadows = true)
        {
            var root = new GameObject(name);
            root.transform.SetParent(parent, false);
            foreach (var kv in bufs)
            {
                if (kv.Value.t.Count == 0) continue;
                var mesh = new Mesh { name = name, indexFormat = kv.Value.v.Count > 65000 ? IndexFormat.UInt32 : IndexFormat.UInt16 };
                mesh.SetVertices(kv.Value.v);
                mesh.SetNormals(kv.Value.n);
                mesh.SetUVs(0, kv.Value.uv);
                mesh.SetTriangles(kv.Value.t, 0);
                mesh.RecalculateBounds();
                var go = new GameObject(kv.Key.name);
                go.transform.SetParent(root.transform, false);
                go.AddComponent<MeshFilter>().sharedMesh = mesh;
                var mr = go.AddComponent<MeshRenderer>();
                mr.sharedMaterial = kv.Key;
                mr.shadowCastingMode = castShadows ? ShadowCastingMode.On : ShadowCastingMode.Off;
            }
            bufs.Clear();
            return root;
        }

        static Shape MakeBox()
        {
            var v = new Vector3[8];
            for (int i = 0; i < 8; i++) v[i] = new Vector3((i & 1) != 0 ? 0.5f : -0.5f, (i & 2) != 0 ? 0.5f : -0.5f, (i & 4) != 0 ? 0.5f : -0.5f);
            return new Shape { v = v, t = new[] { 0, 1, 3, 0, 3, 2, 4, 5, 7, 4, 7, 6, 0, 1, 5, 0, 5, 4, 2, 3, 7, 2, 7, 6, 0, 2, 6, 0, 6, 4, 1, 3, 7, 1, 7, 5 } };
        }

        static Shape MakeOcta()
        {
            var v = new[] { Vector3.up, Vector3.down, Vector3.right, Vector3.left, Vector3.forward, Vector3.back };
            return new Shape { v = v, t = new[] { 0, 2, 4, 0, 4, 3, 0, 3, 5, 0, 5, 2, 1, 2, 4, 1, 4, 3, 1, 3, 5, 1, 5, 2 } };
        }

        static Shape MakeIco(int detail)
        {
            float p = (1f + Mathf.Sqrt(5f)) / 2f;
            var v = new List<Vector3>
            {
                new Vector3(-1, p, 0), new Vector3(1, p, 0), new Vector3(-1, -p, 0), new Vector3(1, -p, 0),
                new Vector3(0, -1, p), new Vector3(0, 1, p), new Vector3(0, -1, -p), new Vector3(0, 1, -p),
                new Vector3(p, 0, -1), new Vector3(p, 0, 1), new Vector3(-p, 0, -1), new Vector3(-p, 0, 1),
            };
            for (int i = 0; i < v.Count; i++) v[i] = v[i].normalized;
            var t = new List<int> { 0, 11, 5, 0, 5, 1, 0, 1, 7, 0, 7, 10, 0, 10, 11, 1, 5, 9, 5, 11, 4, 11, 10, 2, 10, 7, 6, 7, 1, 8,
                3, 9, 4, 3, 4, 2, 3, 2, 6, 3, 6, 8, 3, 8, 9, 4, 9, 5, 2, 4, 11, 6, 2, 10, 8, 6, 7, 9, 8, 1 };
            for (int d = 0; d < detail; d++)
            {
                var nt = new List<int>();
                for (int i = 0; i < t.Count; i += 3)
                {
                    int a = t[i], b = t[i + 1], c = t[i + 2];
                    int ab = v.Count; v.Add(((v[a] + v[b]) / 2f).normalized);
                    int bc = v.Count; v.Add(((v[b] + v[c]) / 2f).normalized);
                    int ca = v.Count; v.Add(((v[c] + v[a]) / 2f).normalized);
                    nt.AddRange(new[] { a, ab, ca, b, bc, ab, c, ca, bc, ab, bc, ca });
                }
                t = nt;
            }
            return new Shape { v = v.ToArray(), t = t.ToArray() };
        }

        // A cone (pointed) or a cylinder of radius 1 and height 1, centred, with n sides.
        static Shape MakeCone(int n, float turn, bool pointed)
        {
            var v = new List<Vector3>();
            var t = new List<int>();
            for (int i = 0; i < n; i++)
            {
                float a = turn + i * Mathf.PI * 2f / n;
                v.Add(new Vector3(Mathf.Sin(a), -0.5f, Mathf.Cos(a)));
            }
            int baseC = v.Count; v.Add(new Vector3(0, -0.5f, 0));
            if (pointed)
            {
                int apex = v.Count; v.Add(new Vector3(0, 0.5f, 0));
                for (int i = 0; i < n; i++) { int j = (i + 1) % n; t.AddRange(new[] { i, j, apex, i, j, baseC }); }
            }
            else
            {
                for (int i = 0; i < n; i++) v.Add(new Vector3(v[i].x, 0.5f, v[i].z));
                int topC = v.Count; v.Add(new Vector3(0, 0.5f, 0));
                for (int i = 0; i < n; i++)
                {
                    int j = (i + 1) % n, ti = n + 1 + i, tj = n + 1 + j;
                    t.AddRange(new[] { i, j, tj, i, tj, ti, i, j, baseC, ti, tj, topC });
                }
            }
            return new Shape { v = v.ToArray(), t = t.ToArray() };
        }
    }
}
