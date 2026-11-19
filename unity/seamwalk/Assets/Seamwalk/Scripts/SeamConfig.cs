using System;
using UnityEngine;

namespace Seamwalk
{
    /// <summary>
    /// Every tuning value of the game, read from Resources/seamwalk-config.json so none is hardcoded.
    /// Angles are radians and distances metres, as in the browser game this was rebuilt from.
    /// </summary>
    [Serializable]
    public class SeamConfig
    {
        public float lookSens = 0.0021f;   // radians per pixel of mouse travel
        public float lookSmooth = 0.025f;  // seconds the view takes to catch the mouse; 0 is raw
        public float turnRate = 2.3f;      // radians per second on the Left and Right arrows
        public float fov = 74f;            // degrees, top to bottom
        public float moveSpeed = 4.3f;     // metres per second
        public float moveAccel = 38f;      // metres per second per second
        public float eyeHeight = 1.6f;     // metres above the feet
        public float bob = 0.03f;          // metres the view rises and falls with each step
        public float gravity = 22f;        // metres per second per second off a ledge
        public float reach = 2.6f;         // metres from which a cube can be picked up
        public float putDist = 1.3f;       // metres ahead a cube is put down
        public float doorSpeed = 1.6f;     // a door's bars sink or rise this many times per second
        public float gateLightDist = 9f;   // metres at which a gate with no door lights
        public float fadeTime = 0.5f;      // seconds to fade between spaces
        public int portalDepth = 2;        // arches seen through arches, this many deep
        public float volume = 0.5f;
        public float sunIntensity = 1.35f;
        public float ambient = 0.85f;
        public float bloom = 0.55f;
        public float shadowDistance = 70f;

        /// <summary>Loads the config from Resources, or the defaults above when the file is missing.</summary>
        public static SeamConfig Load()
        {
            var file = Resources.Load<TextAsset>("seamwalk-config");
            return file != null ? JsonUtility.FromJson<SeamConfig>(file.text) : new SeamConfig();
        }
    }
}
