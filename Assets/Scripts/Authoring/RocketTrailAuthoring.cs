using System;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Serialization;

namespace Authoring
{
    public class RocketTrailAuthoring : MonoBehaviour
    {
        public RocketDataObject rocketData;

        private class RocketTrailBaker : Baker<RocketTrailAuthoring>
        {
            public override void Bake(RocketTrailAuthoring authoring)
            {
                Entity entity = GetEntity(authoring, TransformUsageFlags.None);
                AddComponent(entity, new RocketTrailData()
                {
                    RocketData = BlobAuthoringUtility.BakeToBlob(this, authoring.rocketData)
                });
            }
        }
    
    }

    public struct RocketTrailData : IComponentData
    {
        public int RocketVFXIndex;
        public BlobAssetReference<RocketData> RocketData;
    }
    
    [Serializable]
    public struct RocketData
    {
        public float3 ThrusterLocalPosition;
        public float ThrusterSize;
        public float ThrusterLength;

        public static RocketData Default()
        {
            return new RocketData()
            {
                ThrusterSize = 1f,
                ThrusterLength = 1f,
                ThrusterLocalPosition = float3.zero
            };
        }
    }
    
    [System.Serializable]
    public class RocketDataObject : IBlobAuthoring<RocketData>
    {
        public RocketData Data = RocketData.Default();

        public void BakeToBlobData(ref RocketData data, ref BlobBuilder blobBuilder)
        {
            data = Data;
        }
    }
}
