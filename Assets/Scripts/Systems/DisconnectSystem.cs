// Copyright 2013-2022 AFI, Inc. All Rights Reserved.

using Unity.Entities;
using Unity.NetCode;

[UpdateInGroup(typeof(NetworkReceiveSystemGroup))]
public partial class DisconnectSystem : SystemBase {
    private EndSimulationEntityCommandBufferSystem _commandBufferSystem;

    protected override void OnCreate() {
        RequireSingletonForUpdate<NetworkStreamInGame>();

        _commandBufferSystem = World.GetExistingSystem<EndSimulationEntityCommandBufferSystem>();
    }

    protected override void OnUpdate() {
        var commandBuffer = _commandBufferSystem.CreateCommandBuffer();
        Entities
            .WithAll<NetworkStreamDisconnected>()
            .ForEach((in CommandTargetComponent commandTarget) => {
                commandBuffer.DestroyEntity(commandTarget.targetEntity);
            })
            .Schedule();
        _commandBufferSystem.AddJobHandleForProducer(Dependency);
    }
}