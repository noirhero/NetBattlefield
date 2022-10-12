// Copyright 2013-2022 AFI, Inc. All Rights Reserved.

using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;using Unity.NetCode;
using Unity.Rendering;

[UpdateInGroup(typeof(ClientSimulationSystemGroup))]
public partial class ApplyTeamColorSystem : SystemBase {
    private readonly float4                        _redTeamColor = new (1.0f, 0.0f, 0.0f, 1.0f);
    private readonly float4                        _blueTeamColor = new (0.0f, 0.0f, 1.0f, 1.0f);
    private EntityQuery                            _redTeamQuery;
    private EntityQuery                            _blueTeamQuery;

    private EndSimulationEntityCommandBufferSystem _cmdBufferSystem;


    protected override void OnCreate() {
        _redTeamQuery = GetEntityQuery(
            ComponentType.ReadOnly<TeamColorComponent>(),
            ComponentType.Exclude<URPMaterialPropertyBaseColor>()
        );
        _redTeamQuery.SetSharedComponentFilter(new TeamColorComponent {
            value = TeamColor.Red
        });

        _blueTeamQuery = GetEntityQuery(
            ComponentType.ReadOnly<TeamColorComponent>(),
            ComponentType.Exclude<URPMaterialPropertyBaseColor>()
        );
        _blueTeamQuery.SetSharedComponentFilter(new TeamColorComponent {
            value = TeamColor.Blue
        });

        _cmdBufferSystem = World.GetOrCreateSystem<EndSimulationEntityCommandBufferSystem>();
    }

    protected override void OnUpdate() {
        if (0 == _redTeamQuery.CalculateEntityCount() + _blueTeamQuery.CalculateEntityCount()) {
            return;
        }

        var commandBuffer = _cmdBufferSystem.CreateCommandBuffer();
        foreach (var entity in _redTeamQuery.ToEntityArray(Allocator.Temp)) {
            commandBuffer.AddComponent(entity, new URPMaterialPropertyBaseColor {
                Value = _redTeamColor
            });
        }

        foreach (var entity in _blueTeamQuery.ToEntityArray(Allocator.Temp)) {
            commandBuffer.AddComponent(entity, new URPMaterialPropertyBaseColor {
                Value = _blueTeamColor
            });
        }
        _cmdBufferSystem.AddJobHandleForProducer(Dependency);
    }
}