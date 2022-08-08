// Copyright 2013-2022 AFI, Inc. All Rights Reserved.

using Unity.Entities;
using Unity.Mathematics;
using Unity.NetCode;
using Unity.Transforms;

[UpdateInWorld(TargetWorld.Server)]
[UpdateInGroup(typeof(FixedStepSimulationSystemGroup))]
[UpdateAfter(typeof(CharacterControllerSystem))]
public partial class GunRotationServerSystem : SystemBase {
    protected override void OnUpdate() {
        var deltaTime = Time.fixedDeltaTime;
        Entities
            .WithAll<GunComponent>()
            .WithoutBurst()
            .ForEach((ref Rotation rotation, in Parent parent) => {
                var internalData = EntityManager.GetComponentData<CharacterControllerInternalData>(parent.Value);
                rotation.Value.value.x = math.clamp(rotation.Value.value.x - internalData.Input.Looking.y * deltaTime * 0.1f, -0.3f, 0.3f);
            })
            .Run();
    }
}