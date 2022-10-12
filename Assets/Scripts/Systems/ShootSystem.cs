// Copyright 2013-2022 AFI, Inc. All Rights Reserved.

using Unity.Collections;
using Unity.Entities;
using Unity.NetCode;
using Unity.Physics;
using Unity.Physics.Systems;

[UpdateInWorld(TargetWorld.Server)]
[UpdateInGroup(typeof(FixedStepSimulationSystemGroup))]
[UpdateAfter(typeof(BuildPhysicsWorld)), UpdateBefore(typeof(EndFramePhysicsSystem))]
public partial class ShootSystem : SystemBase {
    private BuildPhysicsWorld _physicsWorld;

    protected override void OnCreate() {
        RequireSingletonForUpdate<PrefabSmallCubeComponent>();
        RequireForUpdate(GetEntityQuery(
                ComponentType.ReadOnly<ShootRequest>(),
                ComponentType.ReadOnly<ReceiveRpcCommandRequestComponent>()
            )
        );

        _physicsWorld = World.GetOrCreateSystem<BuildPhysicsWorld>();
    }

    protected override void OnStartRunning() {
        this.RegisterPhysicsRuntimeSystemReadOnly();
    }

    protected override void OnUpdate() {
        var physicsWorld = _physicsWorld.PhysicsWorld;

        var commandBuffer = new EntityCommandBuffer(Allocator.TempJob);
        Entities
            .WithReadOnly(physicsWorld)
            .WithNone<SendRpcCommandRequestComponent>()
            .ForEach((Entity rpcEntity, in ShootRequest shootRequest, in ReceiveRpcCommandRequestComponent requestEntity) => {
                var end = shootRequest.translate + shootRequest.direction * 1000.0f;
                var isHit = physicsWorld.CastRay(new RaycastInput {
                    Start = shootRequest.translate,
                    End = end,
                    Filter = CollisionFilter.Default
                }, out var hitResult);
                if (isHit) {
                    end = hitResult.Position;

                    var spawnSmallCubeEntity = commandBuffer.CreateEntity();
                    commandBuffer.AddComponent(spawnSmallCubeEntity, new SpawnSmallCubeComponent {
                        translate = end,
                        normal = hitResult.SurfaceNormal,
                    });
                }

                var sendRpcEntity = commandBuffer.CreateEntity();
                commandBuffer.AddComponent(sendRpcEntity, new SpawnShootLineRequest {
                    start = shootRequest.translate,
                    end = end,
                });
                commandBuffer.AddComponent(sendRpcEntity, new SendRpcCommandRequestComponent());

                commandBuffer.DestroyEntity(rpcEntity);
            })
            .Schedule();
        Dependency.Complete();

        commandBuffer.Playback(EntityManager);
        commandBuffer.Dispose();
    }
}