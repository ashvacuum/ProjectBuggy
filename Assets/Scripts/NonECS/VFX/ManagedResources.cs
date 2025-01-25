using UnityEngine;
using UnityEngine.VFX;

namespace NonECS.VFX
{
    public class ManagedResources : MonoBehaviour
    {
        public VisualEffect HitSparksGraph;
        public VisualEffect ExplosionsGraph;

        public void Awake()
        {
            VFXReferences.HitSparksGraph = HitSparksGraph;
            VFXReferences.ExplosionsGraph = ExplosionsGraph;
        }
    }
}