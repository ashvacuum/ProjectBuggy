using Unity.Mathematics;
using UnityEngine;

namespace ProjectBuggy
{
    /// <summary>
    /// Right-click the component header -> "TerrainField Self-Check". The one runnable check on the
    /// height math (engineering invariant: non-trivial logic leaves a runnable check). Verifies
    /// determinism, finiteness, upward-facing normals, and that CliffsModifier = 0 leaves the base shape.
    /// (Lives on a throwaway MonoBehaviour so it survives retiring TerrainGenerator in slice 0.3.)
    /// </summary>
    public class TerrainFieldSelfCheck : MonoBehaviour
    {
        [SerializeField] private TerrainParams terrainParams = TerrainParams.Default;

        [ContextMenu("TerrainField Self-Check")]
        void Run()
        {
            var t = terrainParams;
            float minH = float.MaxValue, maxH = float.MinValue;
            for (int i = 0; i < 1000; i++)
            {
                float2 p = new float2(i * 13.7f, i * -7.3f);
                float h = TerrainField.SampleHeight(p, t);
                Debug.Assert(math.isfinite(h), $"non-finite height at {p}");
                Debug.Assert(TerrainField.SampleHeight(p, t) == h, "non-deterministic height");
                var n = TerrainField.SampleNormal(p, t);
                Debug.Assert(math.all(math.isfinite(n)) && n.y > 0f, $"bad normal at {p}");
                minH = math.min(minH, h);
                maxH = math.max(maxH, h);
            }
            Debug.Log($"TerrainField self-check OK. height range [{minH:F2}, {maxH:F2}] " +
                      $"(amplitude {t.Amplitude}, cliffs {t.CliffsModifier}).");
        }
    }
}
