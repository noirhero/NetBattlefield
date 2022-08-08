// Copyright 2013-2022 AFI, Inc. All Rights Reserved.

using System;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

[Serializable]
public struct ScoreBoardComponent : IComponentData {
    public uint2 ccu;
    public uint2 score;
}

[DisallowMultipleComponent]
public class GameBoardAuthoring : MonoBehaviour, IConvertGameObjectToEntity {
    public void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem) {
        dstManager.AddComponent<ScoreBoardComponent>(entity);
        dstManager.RemoveComponent<LocalToWorld>(entity);
        dstManager.RemoveComponent<Rotation>(entity);
        dstManager.RemoveComponent<Translation>(entity);
    }
}