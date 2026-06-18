using UnityEngine;
using UnityEngine.Rendering;

namespace ProjectBuggy
{
    /// <summary>
    /// Generates a flat XZ grid mesh once at startup. Each frame, snaps its XZ center to the
    /// target (buggy) in cell-sized steps so GPU vertex displacement always samples stable world
    /// coordinates — no surface "swimming" as the buggy moves.
    /// Vertex displacement and lighting live in the TerrainLit ShaderGraph
    /// (Assets/Shaders/TerrainDisplace_Custom.hlsl + Assets/Shaders/TerrainLit.shadergraph).
    /// </summary>
    [RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
    public class TerrainGridMono : MonoBehaviour
    {
        [Tooltip("Cells per side. 512 at CellSize 1 → 512 m visible.")]
        public int GridCells = 512;
        [Tooltip("World units per cell.")]
        public float CellSize = 1f;
        [Tooltip("Buggy Transform to follow (cell-snapped). Assign in Inspector.")]
        public Transform Target;

        void Start()
        {
            GetComponent<MeshFilter>().sharedMesh = BuildMesh(GridCells, CellSize);
        }

        void LateUpdate()
        {
            if (Target == null) return;
            float s = CellSize;
            float cx = Mathf.Floor(Target.position.x / s) * s;
            float cz = Mathf.Floor(Target.position.z / s) * s;
            // Y stays 0; the vertex shader owns height
            transform.position = new Vector3(cx, 0f, cz);
        }

        static Mesh BuildMesh(int cells, float cellSize)
        {
            int edge  = cells + 1;              // vertices per side
            int vCount = edge * edge;
            float half = cells * cellSize * 0.5f;

            var positions = new Vector3[vCount];
            var uvs       = new Vector2[vCount];
            for (int z = 0; z < edge; z++)
            for (int x = 0; x < edge; x++)
            {
                int i = z * edge + x;
                positions[i] = new Vector3(x * cellSize - half, 0f, z * cellSize - half);
                uvs[i]       = new Vector2((float)x / cells, (float)z / cells);
            }

            // ponytail: UInt32 needed — 513² = 263k verts exceeds the 16-bit limit
            var tris = new int[cells * cells * 6];
            int t = 0;
            for (int z = 0; z < cells; z++)
            for (int x = 0; x < cells; x++)
            {
                int bl = z * edge + x;
                int br = bl + 1;
                int tl = bl + edge;
                int tr = tl + 1;
                tris[t++] = bl; tris[t++] = tl; tris[t++] = tr;
                tris[t++] = bl; tris[t++] = tr; tris[t++] = br;
            }

            var mesh = new Mesh { name = "TerrainGrid", indexFormat = IndexFormat.UInt32 };
            mesh.SetVertices(positions);
            mesh.SetUVs(0, uvs);
            mesh.SetTriangles(tris, 0);
            // Oversized Y bounds — vertex shader displaces height, frustum culling must not clip
            mesh.bounds = new Bounds(Vector3.zero,
                new Vector3(cells * cellSize, 200f, cells * cellSize));
            return mesh;
        }
    }
}
