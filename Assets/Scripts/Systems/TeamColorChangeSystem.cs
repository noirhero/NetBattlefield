// Copyright 2013-2022 AFI, Inc. All Rights Reserved.

using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.NetCode;
using Unity.Rendering;
using Unity.Transforms;
using Quaternion = UnityEngine.Quaternion;

[UpdateInGroup(typeof(ClientSimulationSystemGroup))]
public partial class TeamColorChangeSystem : SystemBase {
    private EntityQuery _teamColorQuery;

    protected override void OnCreate() {
        _teamColorQuery = GetEntityQuery(ComponentType.ReadOnly<TeamColorComponent>());
        _teamColorQuery.SetSharedComponentFilter(new TeamColorComponent {
            value = TeamColor.None
        });
    }

    protected override void OnUpdate() {
        if (0 == _teamColorQuery.CalculateEntityCount()) {
            return;
        }

        var changeTeamColorBuffer = new NativeList<ChangeTeamColor>(Allocator.Temp);

        var commandBuffer = new EntityCommandBuffer(Allocator.Temp);
        Entities
            .WithNone<SendRpcCommandRequestComponent>()
            .ForEach((Entity rpcEntity, in ChangeTeamColor changeTeamColor, in ReceiveRpcCommandRequestComponent requestEntity) => {
                changeTeamColorBuffer.Add(changeTeamColor);

                commandBuffer.DestroyEntity(rpcEntity);
            })
            .Run();

        Entities
            .WithoutBurst()
            .WithSharedComponentFilter(new TeamColorComponent { value = TeamColor.None })
            .ForEach((Entity entity, in GhostOwnerComponent ghostOwner) => {
                foreach (var changeTeamColor in changeTeamColorBuffer) {
                    if (changeTeamColor.networkId == ghostOwner.NetworkId) {
                        commandBuffer.SetSharedComponent(entity, new TeamColorComponent {
                            value = changeTeamColor.teamColor
                        });
                        commandBuffer.SetComponent(entity, new Rotation {
                            Value = changeTeamColor.rotation
                        });

                        Quaternion oldQuaternion = changeTeamColor.rotation;
                        commandBuffer.SetComponent(entity, new CharacterControllerInternalData {
                            Entity = entity,
                            Input = new CharacterControllerInput(),
                            CurrentRotationAngle = math.radians(oldQuaternion.eulerAngles.y)
                        });

                        commandBuffer.AddComponent(entity, new URPMaterialPropertyBaseColor {
                            Value = GetTeamColor(changeTeamColor.teamColor)
                        });
                        break;
                    }
                }
            })
            .Run();
        commandBuffer.Playback(EntityManager);
    }

    private float4 GetTeamColor(TeamColor teamColor) {
        return teamColor switch {
            TeamColor.Blue => new float4(0.0f, 0.0f, 1.0f, 1.0f),
            TeamColor.Red => new float4(1.0f, 0.0f, 0.0f, 1.0f),
            _ => new float4(0.0f, 0.0f, 0.0f, 1.0f)
        };
    }
}