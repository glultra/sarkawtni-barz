using UnityEngine;

namespace Seamwalk
{
    /// <summary>
    /// The five spaces, in the builder's vocabulary. Coordinates are the rules' own: x east, z south,
    /// metres. Each layout is the browser game's, with lamps added where a roof shuts the sun out.
    /// </summary>
    public static class Spaces
    {
        const float PI = Mathf.PI;
        public static readonly string[] Names = { "A hole in the garden", "Round the corner, twice", "The long way back", "A door with two sides", "Everything at once" };
        static readonly char[] GateKeys = { '\0', '\0', 'y', 'y', 'p' };

        public static int Count => Names.Length;

        /// <summary>Builds space <paramref name="i"/> (0 to 4) into the level the builder holds.</summary>
        public static void Build(int i, Builder B)
        {
            B.L.name = Names[i]; B.L.gateKey = GateKeys[i];
            switch (i)
            {
                case 0: Space1(B); break;
                case 1: Space2(B); break;
                case 2: Space3(B); break;
                case 3: Space4(B); break;
                default: Space5(B); break;
            }
            B.Finish();
        }

        static void GardenBase(Builder B, Zone Z, float x0, float z0, float x1, float z1, int seed, string skip = "")
        {
            B.Ground(Z); B.Area(Z, x0 - 1, z0 - 1, x1 + 1, z1 + 1, 0); B.Tiles(Z, x0, z0, x1, z1);
            if (!skip.Contains("N")) B.Wall(Z, x0 - 0.5f, z0 - 0.5f, x1 + 0.5f, z0);
            if (!skip.Contains("S")) B.Wall(Z, x0 - 0.5f, z1, x1 + 0.5f, z1 + 0.5f);
            if (!skip.Contains("W")) B.Wall(Z, x0 - 0.5f, z0, x0, z1);
            if (!skip.Contains("E")) B.Wall(Z, x1, z0, x1 + 0.5f, z1);
            B.Scenery(Z, x0 - 4, z0 - 4, x1 + 4, z1 + 4, seed);
        }

        // A hedge path round a great clipped block.
        static void RingGarden(Builder B, Zone Z, int seed, float bx1 = 9, float bz1 = 17)
        {
            var th = Z.th;
            B.Ground(Z); B.Area(Z, -8, -8, 8, 8, 0); B.Tiles(Z, -6.5f, -6.5f, 6.5f, 6.5f);
            B.Box(Z, -3, 0, -3, 3, 0.7f, 3, th.stone); B.Box(Z, -2.9f, 0.7f, -2.9f, 2.9f, 4.0f, 2.9f, th.hedge); B.Box(Z, -2.8f, 4.0f, -2.8f, 2.8f, 4.25f, 2.8f, th.hedgeTop);
            for (int i = 0; i < 4; i++) B.Blob(Z, MeshKit.Ico, (i % 2 == 1 ? 1 : -1) * 1.5f, 4.5f, (i < 2 ? 1 : -1) * 1.5f, 1.0f, 0.7f, 1.0f, World.Mat(th.hedgeTop), i);
            B.Block(Z, -3, -3, 3, 3, 0, 4.3f);
            B.Hedge(Z, -7.1f, -7.1f, 7.1f, -6.5f); B.Hedge(Z, -7.1f, -6.5f, -6.5f, 6.5f); B.Hedge(Z, 6.5f, -6.5f, 7.1f, 6.5f);
            B.Hedge(Z, -7.1f, 6.5f, -2, 7.1f); B.Hedge(Z, 2, 6.5f, 7.1f, 7.1f);
            B.Scenery(Z, -9, -9, bx1, bz1, seed);
        }

        // The same path indoors, under a low ceiling, lit by its crystals.
        static void RingHall(Builder B, Zone Z)
        {
            var th = Z.th; const float h = 3.6f;
            B.Area(Z, -8, -8, 8, 8, 0); B.Tiles(Z, -6.5f, -6.5f, 6.5f, 6.5f);
            B.Wall(Z, -7.1f, -7.1f, 7.1f, -6.5f, h); B.Wall(Z, -7.1f, -6.5f, -6.5f, 6.5f, h); B.Wall(Z, 6.5f, -6.5f, 7.1f, 6.5f, h);
            B.Wall(Z, -7.1f, 6.5f, -0.65f, 7.1f, h); B.Wall(Z, 0.65f, 6.5f, 7.1f, 7.1f, h); B.Box(Z, -0.65f, 2.2f, 6.5f, 0.65f, h, 7.1f, th.wall);
            B.Box(Z, -0.77f, 0, 6.44f, -0.65f, 2.32f, 6.5f, World.DOORWOOD); B.Box(Z, 0.65f, 0, 6.44f, 0.77f, 2.32f, 6.5f, World.DOORWOOD); B.Box(Z, -0.77f, 2.2f, 6.44f, 0.77f, 2.32f, 6.5f, World.DOORWOOD);
            B.Box(Z, -3, 0, -3, 3, h, 3, th.top); B.Block(Z, -3, -3, 3, 3, 0, h);
            B.Box(Z, -7.6f, h, -7.6f, 7.6f, h + 0.5f, 7.6f, th.ceil);
            B.Box(Z, -7.6f, -0.5f, -7.6f, 7.6f, -0.02f, 7.6f, th.stone);
            int k = 0;
            foreach (var c in new[] { new Vector2(-5.9f, -5.9f), new Vector2(5.9f, -5.9f), new Vector2(-5.9f, 5.9f), new Vector2(5.9f, 5.9f) }) B.Crystal(Z, c.x, c.y, 1.3f, k++, 0, true);
            B.Crystal(Z, -3.25f, 0, 0.8f, 0); B.Crystal(Z, 3.25f, 0, 0.8f, 1); B.Crystal(Z, 0, -3.25f, 0.8f, 2); B.Crystal(Z, 0, 3.25f, 0.8f, 1);
            B.Lamp(Z, 0, 2.6f, 5.0f, World.Hex(0xffc0b0), 6f, 8f);
        }

        static void Each(float[][] pts, System.Action<float, float, int> f)
        {
            for (int i = 0; i < pts.Length; i++) f(pts[i][0], pts[i][1], i);
        }

        static float[][] P(params float[] xz)
        {
            var r = new float[xz.Length / 2][];
            for (int i = 0; i < r.Length; i++) r[i] = new[] { xz[i * 2], xz[i * 2 + 1] };
            return r;
        }

        static void Space1(Builder B)
        {
            Zone G = B.NewZone("green"), R = B.NewZone("red"), H = B.NewZone("hall");
            foreach (var Z in new[] { G, R })
            {
                GardenBase(B, Z, -7, -21, 7, 11, 11);
                B.Hedge(Z, -7, 1.7f, -1.5f, 2.3f); B.Hedge(Z, 1.5f, 1.7f, 7, 2.3f);            // the only way north is the arch
                B.Hedge(Z, -2.5f, -6.3f, -1.9f, -3.7f); B.Hedge(Z, -2.5f, -0.7f, -1.9f, 1.7f);   // and a second arch splits east from west
                B.Shed(Z, 0, -13, Z == G);
                Each(P(-5, 9.5f, 5, 9.3f, -5.6f, 5, 5.8f, 4.6f), (x, z, i) => B.Bush(Z, x, z, 1, i));
                Each(P(-3, 6, 3.2f, 7.2f, -4.2f, 3.4f, 4.4f, 3.2f, 2.5f, 9.8f, -2.2f, 10, 5, -1, 4.5f, -4.5f, -5.5f, -9, 5.5f, -9.5f, -4, -17, 4, -18), (x, z, i) => B.Flower(Z, x, z, i));
                B.Tree(Z, -5.2f, -18.5f, 0.8f, 1); B.Tree(Z, 5.4f, -15.5f, 0.8f, 3);
            }
            B.Wall(R, -7, -6.3f, 7, -5.7f); // a wall here, none there
            B.Arch(G, R, 0, 2, 0); B.Arch(G, R, -2.2f, -2.2f, PI / 2);
            B.Link(B.Portal(G, 0, 0, -11.8f, 0, 1.3f, 2.2f), B.Portal(H, 0, 0, 0, PI, 1.3f, 2.2f)); // the shed's door opens on a hall
            var th = H.th;
            B.Area(H, -5, -35, 5, 1, 0); B.Tiles(H, -4, -34, 4, 0);
            B.Wall(H, -4.5f, -34.5f, -4, 0.5f, 5); B.Wall(H, 4, -34.5f, 4.5f, 0.5f, 5); B.Wall(H, -4, -34.5f, 4, -34, 5);
            B.Wall(H, -4, 0, -0.65f, 0.5f, 5); B.Wall(H, 0.65f, 0, 4, 0.5f, 5); B.Box(H, -0.65f, 2.2f, 0, 0.65f, 5, 0.5f, th.wall);
            B.Box(H, -0.77f, 0, -0.06f, -0.65f, 2.32f, 0, World.DOORWOOD); B.Box(H, 0.65f, 0, -0.06f, 0.77f, 2.32f, 0, World.DOORWOOD); B.Box(H, -0.77f, 2.2f, -0.06f, 0.77f, 2.32f, 0, World.DOORWOOD);
            B.Box(H, -5, 5, -35, 5, 5.5f, 1, th.ceil); B.Box(H, -5, -0.5f, -35, 5, -0.02f, 1, th.stone);
            B.Box(H, -0.9f, 0.013f, -33, 0.9f, 0.03f, -1, 0xf8b6c9);
            foreach (float z in new[] { -5f, -11f, -17f, -23f, -29f })
                for (int s = -1; s <= 1; s += 2)
                {
                    B.Box(H, s * 2.6f - 0.3f, 0, z - 0.3f, s * 2.6f + 0.3f, 5, z + 0.3f, th.top); B.Block(H, s * 2.6f - 0.3f, z - 0.3f, s * 2.6f + 0.3f, z + 0.3f, 0, 5);
                    if (z > -29f)
                    {
                        B.Box(H, s * 3.99f - 0.03f, 1.2f, z - 4.2f, s * 3.99f + 0.03f, 4.0f, z - 1.8f, World.Mat(0xdff3ff, 1.15f)); // a tall bright window
                        B.Lamp(H, s * 3.3f, 2.8f, z - 3f, World.Hex(0xfff0d8), 14f, 11f);
                    }
                    B.Bush(H, s * 3.3f, z, 0.8f, 0); B.Flower(H, s * 3.3f, z + 0.9f, (int)-z);
                }
            B.Lamp(H, 0, 3.2f, -1.5f, World.Hex(0xfff0d8), 8f, 9f);
            B.AddGate(H, 0, -31.5f, 0);
            B.Start(G, 0, 8.5f, 0);
        }

        static void Space2(Builder B)
        {
            Zone A = B.NewZone("green"), C = B.NewZone("green");
            foreach (var Z in new[] { A, C })
            {
                RingGarden(B, Z, 22);
                B.Area(Z, -5, 6.4f, 5, 15, 0); B.Tiles(Z, -4, 7.1f, 4, 14); B.Tiles(Z, -2, 6.5f, 2, 7.1f);
                B.Hedge(Z, -4.6f, 7.1f, -4, 14.6f); B.Hedge(Z, 4, 7.1f, 4.6f, 14.6f); B.Hedge(Z, -4, 14, 4, 14.6f);
            }
            B.Bench(A, -2.6f, 12.6f);
            Each(P(2.6f, 12.8f, 2.9f, 11.6f, 1.8f, 13.2f, -2.8f, 9), (x, z, i) => B.Flower(A, x, z, i));
            B.Bush(A, 3, 8.2f, 1, 1);
            B.AddGate(C, 0, 12.6f, PI); // not where you began
            int k = 0;
            foreach (float x in new[] { -1.9f, 1.9f }) { int kk = k++; foreach (float z in new[] { 8.2f, 9.6f, 11f }) B.Flower(C, x, z, kk++); }
            B.Seam(A, C, 0, 0, -4.75f, PI / 2, 3.48f, 9);
            B.Start(A, 0, 12, 0);
        }

        static void Space3(Builder B)
        {
            Zone GA = B.NewZone("green"), GB = B.NewZone("green"), RA = B.NewZone("red"), RB = B.NewZone("red");
            foreach (var Z in new[] { GA, GB, RA, RB })
            {
                RingGarden(B, Z, 33, 12, 18);
                B.Hedge(Z, 3, -0.3f, 3.25f, 0.3f); B.Hedge(Z, 6.25f, -0.3f, 6.5f, 0.3f);          // the east path is the arch and nothing else
                B.Area(Z, -6, 6.4f, 11, 16, 0); B.Tiles(Z, -5, 7.1f, 5, 15); B.Tiles(Z, -2, 6.5f, 2, 7.1f);
                B.Hedge(Z, -5.6f, 7.1f, -5, 15.6f); B.Hedge(Z, -5, 15, 5.6f, 15.6f);
                if (Z == GA) { B.Hedge(Z, 5, 7.1f, 5.6f, 10); B.Hedge(Z, 5, 12, 5.6f, 15); } else B.Hedge(Z, 5, 7.1f, 5.6f, 15);
                if (!Z.th.red) B.Water(Z, -6.5f, -1.5f, -3, 1.5f);                               // a pond across the west path, in this life only
                B.Bush(Z, -4, 8.2f, 1, 0); B.Bush(Z, 4, 14, 1, 1);
            }
            B.Arch(GA, RA, 4.75f, 0, 0); B.Arch(GB, RB, 4.75f, 0, 0);
            B.Seam(GA, GB, 0, 0, -4.75f, PI / 2, 3.48f, 9); B.Seam(RA, RB, 0, 0, -4.75f, PI / 2, 3.48f, 9);
            B.Sw(GA, 0, 10.5f, 'y'); B.Bars(GA, 5, 10, 5.6f, 12, 'y');
            B.Tiles(GA, 5.6f, 10, 9.6f, 12); B.Hedge(GA, 5.6f, 9.4f, 10.2f, 10); B.Hedge(GA, 5.6f, 12, 10.2f, 12.6f); B.Hedge(GA, 9.6f, 10, 10.2f, 12);
            B.AddGate(GA, 8.6f, 11, -PI / 2); B.Bench(GA, -3.4f, 14.2f);
            B.AddCube(RB, 0, 10.5f);
            Each(P(-3.5f, 13.5f, 3.6f, 9, -3.8f, 9.4f), (x, z, i) => B.Crystal(RB, x, z, 1.2f, i, 0, i == 0));
            foreach (var Z in new[] { GB, RA }) Each(P(-3.5f, 13.5f, 3.6f, 9), (x, z, i) => B.Flower(Z, x, z, i));
            B.Start(GA, -2.5f, 13.5f, 0);
        }

        static void Space4(Builder B)
        {
            Zone G = B.NewZone("green"), C = B.NewZone("cave");
            GardenBase(B, G, -8, -12, 8, 10, 44);
            B.Frame(G, false, 0, 0, 2, 0, 1.3f, 2.2f);                                            // one door, standing alone
            B.Deck(G, -8, -10, 8, -7, 2.6f);                                                     // a walkway high over the north end
            foreach (float x in new[] { -7.7f, -2.7f, 2.7f, 7.7f }) { B.Post(G, x, -7.2f, 2.3f); B.Post(G, x, -9.8f, 2.3f); }
            B.Frame(G, false, -5.5f, 2.6f, -8.5f, PI / 2, 1.3f, 2.2f);
            B.Bars(G, 0.9f, -10, 1.3f, -7, 'y', 2.6f, 2.6f); B.AddGate(G, 6, -8.5f, -PI / 2, 2.6f);
            B.Sw(G, 4, -2, 'y');
            Each(P(-6, 7, 6, 6.5f, -6.5f, -3, 6.6f, 1), (x, z, i) => B.Bush(G, x, z, 1, i));
            Each(P(-3, 5, 3, 6, -5, 1, 2, -4, -4, -5, 6, -5, -2, 8.5f), (x, z, i) => B.Flower(G, x, z, i));
            B.Tree(G, -6.3f, 3, 0.7f, 1); B.Tree(G, 6.4f, -4, 0.7f, 2);
            var th = C.th;
            B.Area(C, -5, -1, 5, 10, 0); B.Tiles(C, -4, 0, 4, 9);
            B.Wall(C, -4.5f, -0.5f, -0.65f, 0, 3.2f); B.Wall(C, 0.65f, -0.5f, 4.5f, 0, 3.2f); B.Box(C, -0.65f, 2.2f, -0.5f, 0.65f, 3.2f, 0, th.wall);
            B.Box(C, -0.77f, 0, 0, -0.65f, 2.32f, 0.06f, World.DOORWOOD); B.Box(C, 0.65f, 0, 0, 0.77f, 2.32f, 0.06f, World.DOORWOOD); B.Box(C, -0.77f, 2.2f, 0, 0.77f, 2.32f, 0.06f, World.DOORWOOD);
            B.Wall(C, -4.5f, 9, 4.5f, 9.5f, 3.2f); B.Wall(C, -4.5f, 0, -4, 9, 3.2f); B.Wall(C, 4, 0, 4.5f, 9, 3.2f);
            B.Box(C, -5, 3.2f, -1, 5, 3.7f, 10, th.ceil); B.Box(C, -5, -0.5f, -1, 5, -0.02f, 10, th.stone);
            B.AddCube(C, 2, 6);
            Each(P(-3.3f, 1, 3.3f, 8.2f, -3.2f, 8, 3.4f, 1.2f, -3.4f, 4.5f), (x, z, i) => B.Crystal(C, x, z, 1.2f, i, 0, i < 3));
            Each(P(-1.5f, 6.5f, 1, 3, -2.5f, 2.8f), (x, z, i) => B.Bush(C, x, z, 1, i));
            B.Lamp(C, 0, 2.6f, 4.5f, World.Hex(0xffb0a8), 7f, 9f);
            B.Link(B.Portal(G, 0, 0, 2, 0, 1.3f, 2.2f), B.Portal(G, -5.5f, 2.6f, -8.5f, PI / 2, 1.3f, 2.2f)); // from the south: up onto the walkway
            B.Link(B.Portal(G, 0, 0, 2, PI, 1.3f, 2.2f), B.Portal(C, 0, 0, 0, 0, 1.3f, 2.2f));              // from the north: down into a red cellar
            B.Start(G, 0, 8, 0);
        }

        static void Space5(Builder B)
        {
            Zone G = B.NewZone("green"), R = B.NewZone("red"), HA = B.NewZone("cave"), HB = B.NewZone("cave");
            foreach (var Z in new[] { G, R })
            {
                GardenBase(B, Z, -9, -14, 9, 12, 55, "E");
                if (Z == G) { B.Wall(Z, 9, -14, 9.5f, 8); B.Wall(Z, 9, 10, 9.5f, 12); } else B.Wall(Z, 9, -14, 9.5f, 12);
                B.Shed(Z, -5, 5, Z == R);                                                          // boarded in one life, open in the other
                B.Box(Z, -8, 0, -14, -2, 2.6f, -11.5f, Z.th.stone); B.Block(Z, -8, -14, -2, -11.5f, 0, 2.5f); // a balcony with no stair
                B.Area(Z, -8, -14, -2, -11.5f, 2.6f); B.Tiles(Z, -8, -14, -2, -11.5f, 2.6f);
                B.Frame(Z, false, -5, 2.6f, -13, 0, 1.3f, 2.2f);
                B.RampZ(Z, 5, -13, 8, -7, 0, 2.6f);                                                // stairs up to a bridge
                float end = Z == G ? 3 : -4;                                                       // whole in one life, broken in the other
                B.Deck(Z, 5, -7, 8, end, 2.6f); B.Rail(Z, 4.85f, -7, 5, end, 2.6f); B.Rail(Z, 8, -7, 8.15f, end, 2.6f);
                B.Post(Z, 5.2f, -3.6f, 2.3f); B.Post(Z, 7.8f, -3.6f, 2.3f);
                if (Z == G) { B.Post(Z, 5.2f, 1.9f, 2.3f); B.Post(Z, 7.8f, 1.9f, 2.3f); }
                Each(P(-7.5f, 10.5f, 7.5f, 11, -7.8f, -3.5f, 3, -12.5f), (x, z, i) => B.Bush(Z, x, z, 1, i));
                Each(P(-2, 9, 2.5f, 10.5f, -7, 8, 1, 3, -6, -4, 2, -5, -1, -11, 3.5f, -3.5f), (x, z, i) => B.Flower(Z, x, z, i));
                B.Tree(Z, -7.5f, 2.4f, 0.7f, 2);
            }
            B.Water(G, -9, -2, 9, 1); B.Wall(R, -9, -0.8f, 9, -0.2f);                             // a pond in one life, a wall in the other
            B.Sw(G, -1, 4, 'p'); B.Bars(G, 9, 8, 9.5f, 10, 'p');
            B.Area(G, 9, 7, 13.5f, 11, 0); B.Tiles(G, 9.5f, 7.4f, 13, 10.6f);
            B.Wall(G, 9.5f, 6.9f, 13.5f, 7.4f); B.Wall(G, 9.5f, 10.6f, 13.5f, 11.1f); B.Wall(G, 13, 7.4f, 13.5f, 10.6f); B.AddGate(G, 12, 9, -PI / 2);
            B.AddCube(R, -6, -6); B.Crystal(R, -7.6f, -7, 1, 0, 0, true); B.Crystal(R, -4.6f, -4.4f, 1, 1);
            B.Arch(G, R, 4, 6.5f, PI / 2); B.Arch(G, R, 0, -8, 0);
            RingHall(B, HA); RingHall(B, HB);
            B.Seam(HA, HB, 0, 0, -4.75f, PI / 2, 3.48f, 3.6f);
            B.Link(B.Portal(R, -5, 0, 6.2f, 0, 1.3f, 2.2f), B.Portal(HA, 0, 0, 6.5f, PI, 1.3f, 2.2f));   // the red shed is a room round a pillar
            B.Link(B.Portal(HB, 0, 0, 6.5f, PI, 1.3f, 2.2f), B.Portal(R, -5, 2.6f, -13, 0, 1.3f, 2.2f)); // go round it, and its door opens on the balcony
            B.Start(G, 0, 10.5f, 0);
        }
    }
}
