// Copyright 2013-2022 AFI, Inc. All Rights Reserved.

using System;
using Unity.NetCode;

[Serializable]
public struct InputCommand : ICommandData {
    public uint Tick { get; set; }
    public int  horizontal;
    public int  vertical;
    public int  rotationHorizontal;
    public int  rotationVertical;
    public int  jump;
}