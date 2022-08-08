// Copyright 2013-2022 AFI, Inc. All Rights Reserved.

using Unity.Entities;
using UnityEngine;

[DisallowMultipleComponent]
public class CapsuleAuthoring : MonoBehaviour, IConvertGameObjectToEntity {
    public void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem) {
        dstManager.AddSharedComponentData(entity, new TeamColorComponent());
    }
}