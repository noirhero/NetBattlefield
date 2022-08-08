// Copyright 2013-2022 AFI, Inc. All Rights Reserved.

using Unity.Entities;

public partial class LifeTimeSystem : SystemBase {
    private EndSimulationEntityCommandBufferSystem _commandBufferSystem;

    protected override void OnCreate() {
        _commandBufferSystem = World.GetExistingSystem<EndSimulationEntityCommandBufferSystem>();
    }

    protected override void OnUpdate() {
        var deltaTime = Time.DeltaTime;

        var commandBuffer = _commandBufferSystem.CreateCommandBuffer();
        Entities
            .ForEach((Entity entity, ref LifeTimeComponent lifeTime) => {
                lifeTime.value -= deltaTime;
                if (0.0f >= lifeTime.value) {
                    commandBuffer.DestroyEntity(entity);
                }
            })
            .Schedule();
        _commandBufferSystem.AddJobHandleForProducer(Dependency);
    }
}