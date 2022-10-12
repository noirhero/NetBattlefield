// Copyright 2013-2022 AFI, Inc. All Rights Reserved.

using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.NetCode;
using Unity.Transforms;
using UnityEngine;
using Random = Unity.Mathematics.Random;

public struct TeamColorBuffer : IBufferElementData {
    public TeamColor value;
}

[UpdateInGroup(typeof(ServerSimulationSystemGroup))]
public partial class SpawnPlayerSystem : SystemBase {
    private Entity                  _teamColorBufferEntity;

    private EntityQuery             _spawnPointQuery;
    private NativeArray<float4>     _redTeamSpawnPoints;
    private NativeArray<quaternion> _redTeamRotations;
    private NativeArray<float4>     _blueTeamSpawnPoints;
    private NativeArray<quaternion> _blueTeamRotations;

    protected override void OnCreate() {
        RequireSingletonForUpdate<PrefabCapsuleComponent>();
        RequireForUpdate(GetEntityQuery(
                ComponentType.ReadOnly<GoInGameRequest>(),
                ComponentType.ReadOnly<ReceiveRpcCommandRequestComponent>()
            )
        );

        _teamColorBufferEntity = EntityManager.CreateEntity();
        EntityManager.AddBuffer<TeamColorBuffer>(_teamColorBufferEntity);

        _spawnPointQuery = GetEntityQuery(ComponentType.ReadOnly<SpawnPointComponent>(), ComponentType.ReadOnly<TeamColorComponent>());
    }

    protected override void OnDestroy() {
        if (_redTeamSpawnPoints.IsCreated) {
            _redTeamSpawnPoints.Dispose();
            _redTeamRotations.Dispose();
        }

        if (_blueTeamSpawnPoints.IsCreated) {
            _blueTeamSpawnPoints.Dispose();
            _blueTeamRotations.Dispose();
        }
    }

    private bool IsUpdate()  {
        return 0 < _redTeamSpawnPoints.Length * _blueTeamSpawnPoints.Length;
    }

    protected override void OnUpdate() {
        RefreshSpawnPointAndRotation(ref _redTeamSpawnPoints, ref _redTeamRotations, TeamColor.Red);
        RefreshSpawnPointAndRotation(ref _blueTeamSpawnPoints, ref _blueTeamRotations, TeamColor.Blue);
        if (false == IsUpdate()) {
            return;
        }

        var prefab = GetSingleton<PrefabCapsuleComponent>().prefab;
        var networkIdFromEntity = GetComponentDataFromEntity<NetworkIdComponent>(true);
        var teamColorBuffer = GetBufferFromEntity<TeamColorBuffer>()[_teamColorBufferEntity];

        var commandBuffer = new EntityCommandBuffer(Allocator.Temp);
        Entities
            .WithoutBurst()
            .WithNone<SendRpcCommandRequestComponent>()
            .ForEach((Entity rpcEntity, in GoInGameRequest request, in ReceiveRpcCommandRequestComponent requestEntity) => {
                var requestNetId = networkIdFromEntity[requestEntity.SourceConnection].Value;
                var teamColor = SelectTeamColor(ref teamColorBuffer);

                var player = commandBuffer.Instantiate(prefab);
                commandBuffer.SetSharedComponent(player, new TeamColorComponent {
                    value = teamColor
                });
                commandBuffer.SetComponent(player, new GhostOwnerComponent {
                    NetworkId = requestNetId
                });
                commandBuffer.AddBuffer<InputCommand>(player);

                var positionAndRotation = GetSpawnPositionAndRotation((uint)rpcEntity.Index, teamColor);
                commandBuffer.SetComponent(player, new Translation {
                    Value = positionAndRotation.Item1
                });
                commandBuffer.SetComponent(player, new Rotation {
                    Value = positionAndRotation.Item2
                });

                Quaternion oldQuaternion = positionAndRotation.Item2;
                commandBuffer.SetComponent(player, new CharacterControllerInternalData {
                    Entity = player,
                    Input = new CharacterControllerInput(),
                    CurrentRotationAngle = math.radians(oldQuaternion.eulerAngles.y)
                });

                commandBuffer.AddComponent<NetworkStreamInGame>(requestEntity.SourceConnection);
                commandBuffer.SetComponent(requestEntity.SourceConnection, new CommandTargetComponent {
                    targetEntity = player
                });

                var sendRpcEntity = commandBuffer.CreateEntity();
                commandBuffer.AddComponent(sendRpcEntity, new ChangeTeamColor {
                    rotation = positionAndRotation.Item2,
                    teamColor = teamColor,
                    networkId = requestNetId,
                });
                commandBuffer.AddComponent(sendRpcEntity, new SendRpcCommandRequestComponent());

                commandBuffer.DestroyEntity(rpcEntity);
            })
            .Run();
        commandBuffer.Playback(EntityManager);
    }

    private TeamColor SelectTeamColor(ref DynamicBuffer<TeamColorBuffer> teamColorBuffer) {
        var teamColor = TeamColor.Red;
        if (false == teamColorBuffer.IsEmpty) {
            teamColor = teamColorBuffer[^1].value switch {
                TeamColor.Red => TeamColor.Blue,
                TeamColor.Blue => TeamColor.Red,
                _ => teamColor
            };
        }

        if (100 < teamColorBuffer.Length) {
            teamColorBuffer.Clear();
        }

        teamColorBuffer.Add(new TeamColorBuffer { value = teamColor });

        return teamColor;
    }

    private int RefreshSpawnPointAndRotation(ref NativeArray<float4> spawnPoints, ref NativeArray<quaternion> rotations, TeamColor teamColor) {
        _spawnPointQuery.SetSharedComponentFilter(new TeamColorComponent { value = teamColor });
        var numSpawnPoint = _spawnPointQuery.CalculateEntityCount();
        if (spawnPoints.Length == numSpawnPoint) {
            return 0;
        }

        spawnPoints = new NativeArray<float4>(numSpawnPoint, Allocator.Persistent);
        rotations = new NativeArray<quaternion>(numSpawnPoint, Allocator.Persistent);
        new CopySpawnPointJob {
            copyTranslateAndScales = spawnPoints,
            rotation = rotations
        }.ScheduleParallel(_spawnPointQuery).Complete();
        return numSpawnPoint;
    }

    private (float3, quaternion) GetSpawnPositionAndRotation(uint randomSeed, TeamColor teamColor) {
        var random = new Random(randomSeed);
        return teamColor switch {
            TeamColor.Red => (GetSpawnPosition(ref random, ref _redTeamSpawnPoints), GetSpawnRotation(ref random, ref _redTeamRotations)),
            TeamColor.Blue => (GetSpawnPosition(ref random, ref _blueTeamSpawnPoints), GetSpawnRotation(ref random, ref _blueTeamRotations)),
            _ => (float3.zero, quaternion.identity)
        };
    }

    private float3 GetSpawnPosition(ref Random random, ref NativeArray<float4> spawnPoints) {
        var index = random.NextInt(0, spawnPoints.Length);
        var translateAndScale = spawnPoints[index];

        translateAndScale.xz += random.NextFloat2(-translateAndScale.w, translateAndScale.w);
        return translateAndScale.xyz;
    }

    private quaternion GetSpawnRotation(ref Random random, ref NativeArray<quaternion> rotations) {
        var index = random.NextInt(0, rotations.Length);
        return rotations[index];
    }
}