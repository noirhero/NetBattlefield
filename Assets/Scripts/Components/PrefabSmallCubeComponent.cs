// Copyright 2013-2022 AFI, Inc. All Rights Reserved.

using System;
using Unity.Entities;

[Serializable]
[GenerateAuthoringComponent]
public struct PrefabSmallCubeComponent : IComponentData {
    public Entity prefab;
}