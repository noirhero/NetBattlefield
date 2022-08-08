// Copyright 2013-2022 AFI, Inc. All Rights Reserved.

using Unity.NetCode;

[UnityEngine.Scripting.Preserve]
public class GameBootstrap : ClientServerBootstrap {
    public override bool Initialize(string defaultWorldName) {
        AutoConnectPort = 7979;
        return base.Initialize(defaultWorldName);
    }
}