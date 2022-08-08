// Copyright 2013-2022 AFI, Inc. All Rights Reserved.

using System;
using Unity.Entities;

[Serializable]
[GenerateAuthoringComponent]
public struct PrefabCylinderComponent : IComponentData {
    public Entity prefab;
}