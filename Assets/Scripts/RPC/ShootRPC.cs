// Copyright 2013-2022 AFI, Inc. All Rights Reserved.

using System;
using Unity.Mathematics;
using Unity.NetCode;

[Serializable]
public struct ShootRequest : IRpcCommand {
    public float3 direction;
    public float3 translate;
}

[Serializable]
public struct SpawnShootLineRequest : IRpcCommand {
    public float3 start;
    public float3 end;
}