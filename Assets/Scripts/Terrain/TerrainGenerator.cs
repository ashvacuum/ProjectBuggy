using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

namespace ProjectBuggy
{
    /// <summary>
    /// Pours TerrainNoise into a Unity Terrain's heightmap. The heightmap is a render-only
    /// cache of the noise function; gameplay samples TerrainNoise directly, so the rendered
    /// surface and the sampled height agree by construction.
    /// </summary>
    [RequireComponent(typeof(Terrain))]
    public class TerrainGenerator : MonoBehaviour
    {
        [SerializeField] private NoiseParams noiseParams = NoiseParams.Default;
        [SerializeField] private bool generateOnStart = true;

        /// The noise this terrain was built from. Gameplay samplers read this so they
        /// can't drift from the rendered surface.
        public NoiseParams Noise => noiseParams;

        void Start()
        {
            if (generateOnStart) Generate();
            PushNoiseToEcs();
        }

        /// Publishes the noise params + terrain origin into ECS so gameplay samples the same surface.
        public void PushNoiseToEcs()
        {
            var world = World.DefaultGameObjectInjectionWorld;
            if (world == null || !world.IsCreated) return;

            var em = world.EntityManager;
            var query = em.CreateEntityQuery(typeof(TerrainNoiseSingleton));
            Entity e = query.IsEmpty ? em.CreateEntity(typeof(TerrainNoiseSingleton)) : query.GetSingletonEntity();
            em.SetComponentData(e, new TerrainNoiseSingleton { Noise = noiseParams, Origin = transform.position });
            query.Dispose();
        }

        [ContextMenu("Generate")]
        public void Generate()
        {
            var terrain = GetComponent<Terrain>();
            var data = terrain.terrainData;
            if (data == null)
            {
                // Bare Terrain with no TerrainData asset (added via Add Component / RequireComponent).
                // Create an in-memory one so the test works without GameObject > 3D Object > Terrain.
                data = new TerrainData
                {
                    heightmapResolution = 513,
                    size = new Vector3(500f, noiseParams.Amplitude, 500f),
                };
                terrain.terrainData = data;
                var col = GetComponent<TerrainCollider>();
                if (col != null) col.terrainData = data;
            }

            int res = data.heightmapResolution;          // 2^n + 1 (e.g. 513)
            float worldW = data.size.x;
            float worldL = data.size.z;

            // The coordinate invariant: world Y range == Amplitude, so normalized * size.y == SampleHeight.
            data.size = new Vector3(worldW, noiseParams.Amplitude, worldL);

            var heights = new float[res, res];
            for (int z = 0; z < res; z++)
            {
                float wz = (float)z / (res - 1) * worldL;
                for (int x = 0; x < res; x++)
                {
                    float wx = (float)x / (res - 1) * worldW;
                    float h = TerrainNoise.SampleHeight(new float2(wx, wz), noiseParams);
                    heights[z, x] = h / noiseParams.Amplitude; // normalize to 0..1
                }
            }
            data.SetHeights(0, 0, heights);
        }

        // Right-click the component header -> Self-check. The one runnable check on the noise math.
        [ContextMenu("Self-check")]
        void SelfCheck()
        {
            var p = noiseParams;
            float minH = float.MaxValue, maxH = float.MinValue;
            for (int i = 0; i < 1000; i++)
            {
                float2 pos = new float2(i * 13.7f, i * 7.3f);
                float h = TerrainNoise.SampleHeight(pos, p);
                minH = math.min(minH, h);
                maxH = math.max(maxH, h);
                Debug.Assert(math.all(math.isfinite(TerrainNoise.SampleGradient(pos, p))),
                    "Gradient not finite at " + pos);
            }
            Debug.Assert(minH >= -0.001f && maxH <= p.Amplitude + 0.001f,
                $"Height escaped [0,{p.Amplitude}]: min={minH}, max={maxH}");
            Debug.Log($"TerrainNoise self-check OK. height range [{minH:F2}, {maxH:F2}] / amplitude {p.Amplitude}");
        }
    }
}
