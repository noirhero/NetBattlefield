// Copyright 2013-2022 AFI, Inc. All Rights Reserved.

using System;
using Unity.Mathematics;
using Unity.NetCode;

[Serializable]
public struct ChangeScoreBoard : IRpcCommand {
    public TeamColor teamColor;
    public uint      score;
}

[Serializable]
public struct ChangeTeamColor : IRpcCommand {
    public quaternion rotation;
    public TeamColor  teamColor;
    public int        networkId;
}