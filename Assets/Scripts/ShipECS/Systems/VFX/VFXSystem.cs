
using System.Runtime.InteropServices;
using ShipECS.Systems;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.VFX;
using static VFXReferences;


[VFXType(VFXTypeAttribute.Usage.GraphicsBuffer)]
public struct VFXHitSparksRequest
{
    public Vector3 Position;
    public Vector3 Color;
}

[VFXType(VFXTypeAttribute.Usage.GraphicsBuffer)]
public struct VFXExplosionRequest
{
    public Vector3 Position;
    public float Scale;
}


public struct VFXManager<T> where T : unmanaged
{
    public NativeReference<int> RequestsCount;
    public NativeArray<T> Requests;

    public bool GraphIsInitialized { get; private set; }

    public VFXManager(int maxRequests, ref GraphicsBuffer graphicsBuffer)
    {
        RequestsCount = new NativeReference<int>(0, Allocator.Persistent);
        Requests = new NativeArray<T>(maxRequests, Allocator.Persistent);

        graphicsBuffer = new GraphicsBuffer(GraphicsBuffer.Target.Structured, maxRequests,
            Marshal.SizeOf(typeof(T)));

        GraphIsInitialized = false;
    }

    public void Dispose(ref GraphicsBuffer graphicsBuffer)
    {
        graphicsBuffer?.Dispose();
        if (RequestsCount.IsCreated)
        {
            RequestsCount.Dispose();
        }
        if (Requests.IsCreated)
        {
            Requests.Dispose();
        }
    }

    public void Update(
        VisualEffect vfxGraph, 
        ref GraphicsBuffer graphicsBuffer, 
        float deltaTimeMultiplier, 
        int spawnBatchId, 
        int requestsCountId, 
        int requestsBufferId)
    {
        if (vfxGraph == null || graphicsBuffer == null) return;
        
        vfxGraph.playRate = deltaTimeMultiplier;
            
        if (!GraphIsInitialized)
        {
            vfxGraph.SetGraphicsBuffer(requestsBufferId, graphicsBuffer);
            GraphIsInitialized = true;
        }

        if (!graphicsBuffer.IsValid()) return;
        
        graphicsBuffer.SetData(Requests, 0, 0, RequestsCount.Value);
        vfxGraph.SetInt(requestsCountId, math.min(RequestsCount.Value, Requests.Length));
        vfxGraph.SendEvent(spawnBatchId);
        RequestsCount.Value = 0;
    }

    public void AddRequest(T request)
    {
        if (RequestsCount.Value < Requests.Length)
        {
            Requests[RequestsCount.Value] = request;
            RequestsCount.Value++;
        }
    }
}

public static class VFXReferences
{
    public static VisualEffect HitSparksGraph;
    public static GraphicsBuffer HitSparksRequestsBuffer;

    public static VisualEffect ExplosionsGraph;
    public static GraphicsBuffer ExplosionsRequestsBuffer;
}

public struct VFXHitSparksSingleton : IComponentData
{
    public VFXManager<VFXHitSparksRequest> Manager;
}

public struct VFXExplosionsSingleton : IComponentData
{
    public VFXManager<VFXExplosionRequest> Manager;
}

[UpdateInGroup(typeof(PausableSystemGroup))]
partial struct VFXSystem : ISystem
{
    private int _spawnBatchId;
    private int _requestsCountId;
    private int _requestsBufferId;
    private int _datasBufferId;

    private VFXManager<VFXHitSparksRequest> _hitSparksManager;
    private VFXManager<VFXExplosionRequest> _explosionsManager;

    private const int HitSparksCapacity = 1000;
    private const int ExplosionsCapacity = 1000;


    public void OnCreate(ref SystemState state)
    {
        // Names to Ids
        _spawnBatchId = Shader.PropertyToID("SpawnBatch");
        _requestsCountId = Shader.PropertyToID("SpawnRequestsCount");
        _requestsBufferId = Shader.PropertyToID("SpawnRequestsBuffer");
        _datasBufferId = Shader.PropertyToID("DatasBuffer");


        _hitSparksManager = new VFXManager<VFXHitSparksRequest>(HitSparksCapacity, ref HitSparksRequestsBuffer);

        _explosionsManager = new VFXManager<VFXExplosionRequest>(ExplosionsCapacity, ref ExplosionsRequestsBuffer);


        // Singletons
        state.EntityManager.AddComponentData(state.EntityManager.CreateEntity(), new VFXHitSparksSingleton
        {
            Manager = _hitSparksManager,
        });
        state.EntityManager.AddComponentData(state.EntityManager.CreateEntity(), new VFXExplosionsSingleton
        {
            Manager = _explosionsManager,
        });
        
    }


    public void OnUpdate(ref SystemState state)
    {
        SystemAPI.QueryBuilder().WithAll<VFXHitSparksSingleton>().Build().CompleteDependency();
        SystemAPI.QueryBuilder().WithAll<VFXExplosionsSingleton>().Build().CompleteDependency();

        var rateRatio = SystemAPI.Time.DeltaTime / Time.deltaTime;

        _hitSparksManager.Update(
            HitSparksGraph,
            ref HitSparksRequestsBuffer,
            rateRatio,
            _spawnBatchId,
            _requestsCountId,
            _requestsBufferId);

        _explosionsManager.Update(
            ExplosionsGraph,
            ref ExplosionsRequestsBuffer,
            rateRatio,
            _spawnBatchId,
            _requestsCountId,
            _requestsBufferId);
    }

    public void OnDestroy(ref SystemState state)
    {
        _hitSparksManager.Dispose(ref HitSparksRequestsBuffer);
        _explosionsManager.Dispose(ref ExplosionsRequestsBuffer);
    }
}
