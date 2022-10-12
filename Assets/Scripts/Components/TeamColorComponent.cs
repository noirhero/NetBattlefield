// Copyright 2013-2022 AFI, Inc. All Rights Reserved.

using System;
using Unity.Entities;
using Unity.NetCode;

public enum TeamColor {
    None,
    Red,
    Blue,
}

[GhostComponent]
public struct TeamColorComponent : ISharedComponentData, IEquatable<TeamColorComponent> {
    [GhostField] public TeamColor value;

    public bool Equals(TeamColorComponent other) {
        return value == other.value;
    }

    public override int GetHashCode() {
        return (int)value;
    }
}