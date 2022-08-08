// Copyright 2013-2022 AFI, Inc. All Rights Reserved.

using Unity.Entities;
using Unity.Mathematics;
using Unity.NetCode;
using Unity.Transforms;

[UpdateInGroup(typeof(ClientSimulationSystemGroup))]
public partial class SpawnCylinderSystem : SystemBase {
    private EndSimulationEntityCommandBufferSystem _cmdBufferSystem;

    protected override void OnCreate() {
        RequireSingletonForUpdate<PrefabCylinderComponent>();
        RequireForUpdate(GetEntityQuery(
                ComponentType.ReadOnly<SpawnCylinderRequest>(),
                ComponentType.ReadOnly<ReceiveRpcCommandRequestComponent>()
            )
        );

        _cmdBufferSystem = World.GetOrCreateSystem<EndSimulationEntityCommandBufferSystem>();
    }

    protected override void OnUpdate() {
        var prefabCylinder = GetSingleton<PrefabCylinderComponent>().prefab;

        var commandBuffer = _cmdBufferSystem.CreateCommandBuffer();
        Entities
            .WithNone<SendRpcCommandRequestComponent>()
            .ForEach((Entity rpcEntity, in SpawnCylinderRequest spawnCylinder, in ReceiveRpcCommandRequestComponent requestEntity) => {
                var at = spawnCylinder.end - spawnCylinder.start;
                var length = math.length(at);
                var halfLength = length * 0.5f;
                var direction = at * (1.0f / length);

                var cylinder = commandBuffer.Instantiate(prefabCylinder);
                commandBuffer.SetComponent(cylinder, new NonUniformScale {
                    Value = new float3(0.01f, halfLength, 0.01f)
                });
                commandBuffer.SetComponent(cylinder, new Rotation {
                    Value = math.mul(
                        quaternion.LookRotationSafe(direction, new float3(0.0f, 1.0f, 0.0f)),
                        quaternion.RotateX(math.radians(90.0f))
                    )
                });
                commandBuffer.SetComponent(cylinder, new Translation {
                    Value = spawnCylinder.start + direction * halfLength
                });

                commandBuffer.DestroyEntity(rpcEntity);
            })
            .Schedule();
        _cmdBufferSystem.AddJobHandleForProducer(Dependency);
    }
}