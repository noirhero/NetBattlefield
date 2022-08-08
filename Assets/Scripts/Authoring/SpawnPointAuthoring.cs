// Copyright 2013-2022 AFI, Inc. All Rights Reserved.

using System;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

[Serializable]
public struct SpawnPointComponent : IComponentData {
    public float4     translateAndScale;
    public quaternion rotation;
}

[DisallowMultipleComponent]
public class SpawnPointAuthoring : MonoBehaviour, IConvertGameObjectToEntity {
    public TeamColor teamColor;

    public void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem) {
        var spawnPointEntity = dstManager.CreateEntity();
        dstManager.AddSharedComponentData(spawnPointEntity, new TeamColorComponent {
            value = teamColor
        });
        dstManager.AddComponentData(spawnPointEntity, new SpawnPointComponent {
            translateAndScale = new float4(dstManager.GetComponentData<Translation>(entity).Value, GetScale(entity, ref dstManager)),
            rotation = dstManager.GetComponentData<Rotation>(entity).Value
        });

        dstManager.DestroyEntity(entity);
    }

    private float GetScale(Entity entity, ref EntityManager entityManager) {
        if (entityManager.HasComponent<NonUniformScale>(entity)) {
            var scale = entityManager.GetComponentData<NonUniformScale>(entity).Value;
            return math.max(math.max(scale.x, scale.y), scale.z);
        }

        if (entityManager.HasComponent<Scale>(entity)) {
            return entityManager.GetComponentData<Scale>(entity).Value;
        }

        if (entityManager.HasComponent<CompositeScale>(entity)) {
            var compositeScale = entityManager.GetComponentData<CompositeScale>(entity).Value;
            return math.max(math.max(compositeScale.c1.x, compositeScale.c2.y), compositeScale.c3.z);
        }

        return 1.0f;
    }
}