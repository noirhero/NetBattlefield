// Copyright 2013-2022 AFI, Inc. All Rights Reserved.

using Unity.Entities;
using Unity.Mathematics;
using Unity.NetCode;
using Unity.Physics;
using Unity.Transforms;
using Random = Unity.Mathematics.Random;

[UpdateInGroup(typeof(ServerSimulationSystemGroup))]
public partial class SpawnSmallCubeSystem : SystemBase {
    private EndSimulationEntityCommandBufferSystem _cmdBufferSystem;

    protected override void OnCreate() {
        RequireSingletonForUpdate<PrefabSmallCubeComponent>();
        RequireForUpdate(GetEntityQuery(ComponentType.ReadOnly<SpawnSmallCubeComponent>()));

        _cmdBufferSystem = World.GetOrCreateSystem<EndSimulationEntityCommandBufferSystem>();
    }

    protected override void OnUpdate() {
        var prefab = GetSingleton<PrefabSmallCubeComponent>().prefab;

        var commandBuffer = _cmdBufferSystem.CreateCommandBuffer();
        Entities
            .ForEach((Entity entity, in SpawnSmallCubeComponent spawnSmallCube) => {
                var random = Random.CreateFromIndex((uint)entity.Index);
                var spawnCount = random.NextUInt(1, 4);

                for (uint i = 0; i < spawnCount; ++i) {
                    var spawnPosition = spawnSmallCube.translate + spawnSmallCube.normal + 0.1f;
                    var jitterPosition = random.NextFloat3(-3.0f, 3.0f);
                    var spawnDirection = math.normalizesafe(spawnPosition + jitterPosition - spawnSmallCube.translate);

                    var smallCube = commandBuffer.Instantiate(prefab);
                    commandBuffer.SetComponent(smallCube, new Translation {
                        Value = spawnSmallCube.translate + spawnSmallCube.normal * 0.1f
                    });

                    commandBuffer.SetComponent(smallCube, new PhysicsVelocity {
                        Angular = random.NextFloat3(1.0f, 2.0f),
                        Linear = spawnSmallCube.normal * random.NextFloat3(0.1f, 2.0f) + spawnDirection * random.NextFloat3(0.2f, 0.5f),
                    });
                }

                commandBuffer.DestroyEntity(entity);
            })
            .Schedule();
        _cmdBufferSystem.AddJobHandleForProducer(Dependency);
    }
}