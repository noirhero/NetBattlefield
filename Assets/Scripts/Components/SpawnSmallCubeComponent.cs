// Copyright 2013-2022 AFI, Inc. All Rights Reserved.

using System;
using Unity.Entities;
using Unity.Mathematics;

[Serializable]
public struct SpawnSmallCubeComponent : IComponentData {
    public float3 translate;
    public float3 normal;
}