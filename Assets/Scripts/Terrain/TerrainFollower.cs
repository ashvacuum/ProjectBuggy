using Unity.Mathematics;
using UnityEngine;

namespace ProjectBuggy
{
    /// <summary>
    /// Pins this object's Y to terrain height sampled from the SAME function the Terrain
    /// mesh was built from. Proof of the core seam: gameplay sits exactly on the rendered
    /// surface, no bake, no sync. Drag this around in the Scene view (ExecuteAlways) and
    /// watch it hug the dunes.
    /// </summary>
    [ExecuteAlways]
    public class TerrainFollower : MonoBehaviour
    {
        [Tooltip("The TerrainGenerator whose noise to ride. Auto-found if left empty.")]
        [SerializeField] private TerrainGenerator terrain;

        [Tooltip("Vertical offset above the surface (e.g. half the object's height so a cube rests on top).")]
        [SerializeField] private float yOffset = 0f;

        void OnEnable()
        {
            if (terrain == null) terrain = FindFirstObjectByType<TerrainGenerator>();
        }

        void LateUpdate()
        {
            if (terrain == null) return;

            // Terrain heights are relative to the Terrain GameObject's transform.
            Vector3 origin = terrain.transform.position;
            var localXZ = new float2(transform.position.x - origin.x, transform.position.z - origin.z);

            float h = TerrainNoise.SampleHeight(localXZ, terrain.Noise);

            var p = transform.position;
            transform.position = new Vector3(p.x, origin.y + h + yOffset, p.z);
        }
    }
}
