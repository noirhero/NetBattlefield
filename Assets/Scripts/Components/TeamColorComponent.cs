// Copyright 2013-2022 AFI, Inc. All Rights Reserved.

using System;
using Unity.Entities;

public enum TeamColor {
    None,
    Red,
    Blue,
}

[Serializable]
public struct TeamColorComponent : ISharedComponentData, IEquatable<TeamColorComponent> {
    public TeamColor value;

    public bool Equals(TeamColorComponent other) {
        return value == other.value;
    }

    public override int GetHashCode() {
        return (int)value;
    }
}