// Copyright 2013-2022 AFI, Inc. All Rights Reserved.

using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.NetCode;

[UpdateInGroup(typeof(ClientSimulationSystemGroup))]
public partial class RequestSpawnPlayerSystem : SystemBase {
    protected override void OnCreate() {
        RequireSingletonForUpdate<PrefabCapsuleComponent>();
        RequireForUpdate(GetEntityQuery(
                ComponentType.ReadOnly<NetworkIdComponent>(),
                ComponentType.Exclude<NetworkStreamInGame>()
            )
        );
    }

    protected override void OnUpdate() {
        var commandBuffer = new EntityCommandBuffer(Allocator.Temp);
        Entities
            .WithNone<NetworkStreamInGame>()
            .WithAll<NetworkIdComponent>()
            .ForEach((Entity entity) => {
                commandBuffer.AddComponent<NetworkStreamInGame>(entity);

                var requestEntity = commandBuffer.CreateEntity();
                commandBuffer.AddComponent(requestEntity, new GoInGameRequest());
                commandBuffer.AddComponent(requestEntity, new SendRpcCommandRequestComponent {
                    TargetConnection = entity
                });
            })
            .Run();
        commandBuffer.Playback(EntityManager);
    }
}