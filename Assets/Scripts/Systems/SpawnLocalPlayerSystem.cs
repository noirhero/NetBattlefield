// Copyright 2013-2022 AFI, Inc. All Rights Reserved.

using Unity.Collections;
using Unity.Entities;
using Unity.NetCode;
using Unity.Transforms;

[UpdateInGroup(typeof(ClientSimulationSystemGroup))]
public partial class SpawnLocalPlayerSystem : SystemBase {
    protected override void OnCreate() {
        RequireSingletonForUpdate<NetworkIdComponent>();
    }

    protected override void OnUpdate() {
        var localEntity = GetSingleton<CommandTargetComponent>().targetEntity;
        if (Entity.Null != localEntity) {
            Enabled = false;
            return;
        }

        var netOwnerEntity = FindNetworkOwnerEntity();
        if (Entity.Null == netOwnerEntity) {
            return;
        }

        EntityManager.SetComponentData(GetSingletonEntity<CommandTargetComponent>(), new CommandTargetComponent {
            targetEntity = netOwnerEntity
        });
        EntityManager.AddBuffer<InputCommand>(netOwnerEntity);

        FindAndAddCameraTargetComponent(netOwnerEntity);
    }

    private Entity FindNetworkOwnerEntity() {
        var localPlayerId = GetSingleton<NetworkIdComponent>().Value;
        var findEntity = Entity.Null;
        Entities
            .ForEach((Entity entity, in GhostOwnerComponent ghostOwner) => {
                if (ghostOwner.NetworkId == localPlayerId) {
                    findEntity = entity;
                }
            })
            .Run();

        return findEntity;
    }

    private void FindAndAddCameraTargetComponent(Entity netOwnerEntity) {
        var commandBuffer = new EntityCommandBuffer(Allocator.Temp);

        var gunEntity = Entity.Null;
        Entities
            .WithAll<GunComponent>()
            .ForEach((Entity entity, in Parent parent) => {
                if (parent.Value == netOwnerEntity) {
                    gunEntity = entity;
                    commandBuffer.AddComponent<NetOwnerGunComponent>(entity);
                }
            })
            .Run();

        Entities
            .WithAll<DisplayGunComponent>()
            .ForEach((Entity entity, in Parent parent) => {
                if (parent.Value == gunEntity) {
                    commandBuffer.AddComponent<NetOwnerGunDisplayComponent>(entity);
                }
            })
            .Run();

        var cameraEntity = GetSingletonEntity<CameraTagComponent>();
        Entities
            .WithAll<CameraTargetComponent>()
            .ForEach((Entity entity, in Parent parent) => {
                if (parent.Value == gunEntity) {
                    commandBuffer.AddComponent(cameraEntity, new Parent {
                        Value = entity
                    });
                    commandBuffer.AddComponent(cameraEntity, new LocalToParent());
                    commandBuffer.SetComponent(cameraEntity, new Translation());
                    commandBuffer.SetComponent(cameraEntity, new Rotation());
                }
            })
            .Run();

        commandBuffer.Playback(EntityManager);
    }
}